using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MetaADBConflictResolver : EditorWindow
{
    private const string WINDOW_TITLE = "Meta ADB Conflict Resolver";
    private const int PROCESS_TIMEOUT_MS = 5000;
    
    private Vector2 scrollPosition;
    private string diagnosticOutput = "";
    private List<string> detectedADBPaths = new List<string>();
    private List<Process> runningADBProcesses = new List<Process>();
    
    [MenuItem("Meta/ADB Conflict Resolver")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaADBConflictResolver>(WINDOW_TITLE);
        window.minSize = new Vector2(550, 650);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Meta ADB Conflict Resolver", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "ISSUE: Getting 4 authorization requests instead of 1?\n\n" +
            "This happens when multiple ADB servers are running simultaneously from:\n" +
            "• Unity Android SDK\n" +
            "• Meta Quest Developer Hub\n" +
            "• Android Studio\n" +
            "• Meta XR SDK tools\n\n" +
            "This tool helps identify and resolve these conflicts.",
            MessageType.Warning);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("1. Diagnose Multiple ADB Instances", GUILayout.Height(35)))
        {
            DiagnoseADBInstances();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("2. Kill ALL ADB Servers (Recommended)", GUILayout.Height(35)))
        {
            KillAllADBServers();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("3. Start Single ADB Server", GUILayout.Height(35)))
        {
            StartSingleADBServer();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("4. Verify Single Server Running", GUILayout.Height(35)))
        {
            VerifyADBServer();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Quick Fix: Kill All & Restart", GUILayout.Height(40)))
        {
            QuickFixADB();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Diagnostic Output:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(diagnosticOutput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "COMPLETE WORKFLOW:\n" +
            "1. Click 'Diagnose Multiple ADB Instances'\n" +
            "2. Click 'Kill ALL ADB Servers' (this kills Meta Hub, Android Studio, Unity ADB)\n" +
            "3. Close Meta Quest Developer Hub if it's running\n" +
            "4. Click 'Start Single ADB Server'\n" +
            "5. Reconnect Quest headset (unplug/replug)\n" +
            "6. You should now get only ONE authorization request\n" +
            "7. Accept the request with 'Always allow from this computer' checked\n\n" +
            "OR use 'Quick Fix' button to automate steps 2-4",
            MessageType.Info);
    }
    
    private void DiagnoseADBInstances()
    {
        diagnosticOutput = "=== DIAGNOSING ADB INSTANCES ===\n\n";
        detectedADBPaths.Clear();
        
        diagnosticOutput += "Searching for ADB installations...\n\n";
        
        string unityADB = GetUnityADBPath();
        if (!string.IsNullOrEmpty(unityADB))
        {
            detectedADBPaths.Add(unityADB);
            diagnosticOutput += $"✓ Unity Android SDK ADB:\n  {unityADB}\n\n";
        }
        else
        {
            diagnosticOutput += "✗ Unity Android SDK ADB: Not found\n\n";
        }
        
        List<string> systemADBPaths = FindSystemADBPaths();
        foreach (string path in systemADBPaths)
        {
            if (!detectedADBPaths.Contains(path))
            {
                detectedADBPaths.Add(path);
                diagnosticOutput += $"✓ System ADB Found:\n  {path}\n\n";
            }
        }
        
        diagnosticOutput += $"TOTAL ADB INSTALLATIONS FOUND: {detectedADBPaths.Count}\n\n";
        
        if (detectedADBPaths.Count > 1)
        {
            diagnosticOutput += "⚠ WARNING: Multiple ADB installations detected!\n";
            diagnosticOutput += "This is likely causing the 4 authorization requests.\n\n";
        }
        
        diagnosticOutput += "Checking for running ADB processes...\n\n";
        
        int runningServers = CheckRunningADBProcesses();
        
        if (runningServers > 0)
        {
            diagnosticOutput += $"⚠ FOUND {runningServers} RUNNING ADB PROCESS(ES)\n";
            diagnosticOutput += "Each running ADB server will trigger an authorization request!\n\n";
            diagnosticOutput += "RECOMMENDED: Click 'Kill ALL ADB Servers' button\n";
        }
        else
        {
            diagnosticOutput += "No ADB processes currently running.\n";
        }
        
        Repaint();
    }
    
    private void KillAllADBServers()
    {
        diagnosticOutput = "=== KILLING ALL ADB SERVERS ===\n\n";
        
        int killedCount = 0;
        
        foreach (string adbPath in detectedADBPaths)
        {
            if (File.Exists(adbPath))
            {
                diagnosticOutput += $"Executing kill-server on:\n{adbPath}\n";
                string result = ExecuteADBCommand(adbPath, "kill-server");
                diagnosticOutput += $"Result: {(string.IsNullOrEmpty(result) ? "Success" : result)}\n\n";
                killedCount++;
            }
        }
        
        if (detectedADBPaths.Count == 0)
        {
            diagnosticOutput += "No ADB paths detected. Running kill-server on default ADB...\n";
            string result = ExecuteADBCommand(GetUnityADBPath(), "kill-server");
            diagnosticOutput += $"Result: {(string.IsNullOrEmpty(result) ? "Success" : result)}\n\n";
            killedCount++;
        }
        
        System.Threading.Thread.Sleep(1000);
        
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            diagnosticOutput += "Forcefully killing any remaining ADB.exe processes...\n";
            ExecuteSystemCommand("taskkill", "/F /IM adb.exe");
            diagnosticOutput += "Done.\n\n";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor)
        {
            diagnosticOutput += "Forcefully killing any remaining ADB processes...\n";
            ExecuteSystemCommand("killall", "adb");
            diagnosticOutput += "Done.\n\n";
        }
        
        diagnosticOutput += $"✓ Killed {killedCount} ADB server(s)\n";
        diagnosticOutput += "All ADB servers should now be stopped.\n\n";
        diagnosticOutput += "NEXT STEP: Click 'Start Single ADB Server'\n";
        
        Repaint();
    }
    
    private void StartSingleADBServer()
    {
        diagnosticOutput = "=== STARTING SINGLE ADB SERVER ===\n\n";
        
        string primaryADB = GetUnityADBPath();
        
        if (string.IsNullOrEmpty(primaryADB))
        {
            diagnosticOutput += "ERROR: Cannot find Unity Android SDK ADB path.\n";
            diagnosticOutput += "Please configure Android SDK in:\n";
            diagnosticOutput += "Edit > Preferences > External Tools > Android SDK Tools\n";
            Repaint();
            return;
        }
        
        diagnosticOutput += $"Using Unity's ADB:\n{primaryADB}\n\n";
        
        diagnosticOutput += "Starting ADB server...\n";
        string result = ExecuteADBCommand(primaryADB, "start-server");
        diagnosticOutput += $"Result:\n{result}\n\n";
        
        System.Threading.Thread.Sleep(1500);
        
        diagnosticOutput += "Checking server status...\n";
        string versionResult = ExecuteADBCommand(primaryADB, "version");
        diagnosticOutput += $"{versionResult}\n\n";
        
        diagnosticOutput += "✓ Single ADB server started.\n\n";
        diagnosticOutput += "NEXT STEPS:\n";
        diagnosticOutput += "1. Unplug your Quest headset\n";
        diagnosticOutput += "2. Wait 3 seconds\n";
        diagnosticOutput += "3. Plug it back in\n";
        diagnosticOutput += "4. You should now get ONLY ONE authorization request in the headset\n";
        diagnosticOutput += "5. Accept with 'Always allow from this computer' checked\n";
        
        Repaint();
    }
    
    private void VerifyADBServer()
    {
        diagnosticOutput = "=== VERIFYING ADB SERVER ===\n\n";
        
        string primaryADB = GetUnityADBPath();
        
        if (string.IsNullOrEmpty(primaryADB))
        {
            diagnosticOutput += "ERROR: Cannot find Unity Android SDK ADB path.\n";
            Repaint();
            return;
        }
        
        diagnosticOutput += "Checking devices...\n";
        string devices = ExecuteADBCommand(primaryADB, "devices -l");
        diagnosticOutput += $"{devices}\n\n";
        
        int runningProcesses = CheckRunningADBProcesses();
        
        if (runningProcesses == 0)
        {
            diagnosticOutput += "⚠ No ADB server is currently running.\n";
            diagnosticOutput += "Click 'Start Single ADB Server' to start it.\n";
        }
        else if (runningProcesses == 1)
        {
            diagnosticOutput += "✓ GOOD: Only ONE ADB server is running.\n";
            diagnosticOutput += "You should get only one authorization request.\n";
        }
        else
        {
            diagnosticOutput += $"⚠ WARNING: {runningProcesses} ADB processes detected!\n";
            diagnosticOutput += "You will get multiple authorization requests.\n";
            diagnosticOutput += "Click 'Kill ALL ADB Servers' and restart.\n";
        }
        
        Repaint();
    }
    
    private void QuickFixADB()
    {
        diagnosticOutput = "=== QUICK FIX: RESETTING ADB ===\n\n";
        
        diagnosticOutput += "Step 1: Killing all ADB servers...\n";
        KillAllADBServers();
        
        diagnosticOutput += "\nStep 2: Waiting for processes to terminate...\n";
        EditorUtility.DisplayProgressBar("Quick Fix", "Waiting...", 0.5f);
        System.Threading.Thread.Sleep(2000);
        
        diagnosticOutput += "\nStep 3: Starting single ADB server...\n";
        StartSingleADBServer();
        
        EditorUtility.ClearProgressBar();
        
        diagnosticOutput += "\n=== QUICK FIX COMPLETE ===\n\n";
        diagnosticOutput += "Now reconnect your Quest headset:\n";
        diagnosticOutput += "1. Unplug the USB cable\n";
        diagnosticOutput += "2. Wait 3 seconds\n";
        diagnosticOutput += "3. Plug it back in\n";
        diagnosticOutput += "4. Accept the SINGLE authorization request in the headset\n";
        
        Repaint();
    }
    
    private int CheckRunningADBProcesses()
    {
        int count = 0;
        
        try
        {
            Process[] processes = Process.GetProcessesByName("adb");
            count = processes.Length;
            
            if (count > 0)
            {
                diagnosticOutput += $"Found {count} ADB process(es):\n";
                foreach (var proc in processes)
                {
                    try
                    {
                        diagnosticOutput += $"  PID: {proc.Id}, Path: {proc.MainModule?.FileName ?? "Unknown"}\n";
                    }
                    catch
                    {
                        diagnosticOutput += $"  PID: {proc.Id}\n";
                    }
                }
                diagnosticOutput += "\n";
            }
        }
        catch (System.Exception e)
        {
            diagnosticOutput += $"Warning: Could not enumerate processes: {e.Message}\n\n";
        }
        
        return count;
    }
    
    private List<string> FindSystemADBPaths()
    {
        List<string> paths = new List<string>();
        
        string[] searchLocations = new string[]
        {
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                "Android", "Sdk", "platform-tools"),
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), 
                "Android", "Android Studio", "platform-tools"),
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), 
                "AppData", "Local", "Android", "Sdk", "platform-tools"),
            "/usr/local/bin",
            "/opt/android-sdk/platform-tools",
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), 
                "Library", "Android", "sdk", "platform-tools")
        };
        
        foreach (string location in searchLocations)
        {
            if (!string.IsNullOrEmpty(location))
            {
                string adbPath = Path.Combine(location, Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb");
                
                if (File.Exists(adbPath))
                {
                    paths.Add(adbPath);
                }
            }
        }
        
        string pathEnv = System.Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            string[] pathDirs = pathEnv.Split(Path.PathSeparator);
            foreach (string dir in pathDirs)
            {
                string adbPath = Path.Combine(dir, Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb");
                if (File.Exists(adbPath) && !paths.Contains(adbPath))
                {
                    paths.Add(adbPath);
                }
            }
        }
        
        return paths;
    }
    
    private string GetUnityADBPath()
    {
        string androidSdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
        
        if (string.IsNullOrEmpty(androidSdkRoot))
        {
            androidSdkRoot = System.Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        }
        
        if (string.IsNullOrEmpty(androidSdkRoot))
        {
            androidSdkRoot = System.Environment.GetEnvironmentVariable("ANDROID_HOME");
        }
        
        if (string.IsNullOrEmpty(androidSdkRoot))
        {
            return null;
        }
        
        string adbPath = Path.Combine(androidSdkRoot, "platform-tools", "adb");
        
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            adbPath += ".exe";
        }
        
        return File.Exists(adbPath) ? adbPath : null;
    }
    
    private string ExecuteADBCommand(string adbPath, string arguments)
    {
        if (string.IsNullOrEmpty(adbPath) || !File.Exists(adbPath))
        {
            return "ERROR: ADB path not found.";
        }
        
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(startInfo))
            {
                if (process.WaitForExit(PROCESS_TIMEOUT_MS))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    return output + error;
                }
                else
                {
                    process.Kill();
                    return "ERROR: Command timed out.";
                }
            }
        }
        catch (System.Exception e)
        {
            return $"ERROR: {e.Message}";
        }
    }
    
    private void ExecuteSystemCommand(string command, string arguments)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit(PROCESS_TIMEOUT_MS);
            }
        }
        catch
        {
        }
    }
}
