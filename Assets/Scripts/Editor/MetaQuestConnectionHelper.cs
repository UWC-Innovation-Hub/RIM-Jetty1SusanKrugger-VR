using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Text;

public class MetaQuestConnectionHelper : EditorWindow
{
    private const string WINDOW_TITLE = "Meta Quest Connection Helper";
    private const int PROCESS_TIMEOUT_MS = 5000;
    
    private Vector2 scrollPosition;
    private string lastCommandOutput = "No commands executed yet.";
    private bool isProcessing = false;
    
    [MenuItem("Meta/Quest Connection Helper")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaQuestConnectionHelper>(WINDOW_TITLE);
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Meta Quest Device Troubleshooting", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool helps diagnose and fix authorization and connection issues with Meta Quest 2/3 devices.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(isProcessing);
        
        if (GUILayout.Button("Check Device Status", GUILayout.Height(30)))
        {
            CheckDeviceStatus();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Restart ADB Server", GUILayout.Height(30)))
        {
            RestartADBServer();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Clear ADB Authorization Keys", GUILayout.Height(30)))
        {
            ClearADBKeys();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Check ADB Version", GUILayout.Height(30)))
        {
            CheckADBVersion();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("List All Connected Devices", GUILayout.Height(30)))
        {
            ListDevices();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Get Device Properties", GUILayout.Height(30)))
        {
            GetDeviceProperties();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Check for Multiple ADB Instances", GUILayout.Height(30)))
        {
            CheckMultipleADBInstances();
        }
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Command Output:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(lastCommandOutput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Common Steps:\n" +
            "1. Check Device Status - See if device is connected and authorized\n" +
            "2. If 'unauthorized', put on headset and look for USB debugging prompt\n" +
            "3. If no prompt appears, try 'Restart ADB Server'\n" +
            "4. If still failing, try 'Clear ADB Authorization Keys'\n" +
            "5. Try different USB cable/port\n" +
            "6. Ensure Developer Mode is enabled in Meta Quest mobile app",
            MessageType.None);
    }
    
    private void CheckDeviceStatus()
    {
        isProcessing = true;
        lastCommandOutput = "Checking device status...\n\n";
        
        string output = ExecuteADBCommand("devices -l");
        
        if (string.IsNullOrEmpty(output))
        {
            lastCommandOutput += "ERROR: Could not execute ADB command.\n";
            lastCommandOutput += "Make sure Android SDK is properly configured in Unity preferences.\n";
            lastCommandOutput += "Check: Edit > Preferences > External Tools > Android SDK Tools";
        }
        else
        {
            lastCommandOutput += "ADB Devices Output:\n";
            lastCommandOutput += output + "\n\n";
            
            if (output.Contains("unauthorized"))
            {
                lastCommandOutput += "⚠ ISSUE DETECTED: Device is UNAUTHORIZED\n\n";
                lastCommandOutput += "ACTION REQUIRED:\n";
                lastCommandOutput += "1. Put on your Quest headset\n";
                lastCommandOutput += "2. Look for 'Allow USB debugging' prompt\n";
                lastCommandOutput += "3. Check 'Always allow from this computer'\n";
                lastCommandOutput += "4. Click 'OK'\n\n";
                lastCommandOutput += "If no prompt appears, try:\n";
                lastCommandOutput += "- Restart ADB Server button above\n";
                lastCommandOutput += "- Clear ADB Authorization Keys button above\n";
                lastCommandOutput += "- Try a different USB cable or port";
            }
            else if (output.Contains("device") && !output.Contains("List of devices"))
            {
                lastCommandOutput += "✓ Device appears to be connected and authorized!";
            }
            else if (output.Contains("offline"))
            {
                lastCommandOutput += "⚠ ISSUE DETECTED: Device is OFFLINE\n\n";
                lastCommandOutput += "Try:\n";
                lastCommandOutput += "1. Unplug and replug the USB cable\n";
                lastCommandOutput += "2. Restart your Quest headset\n";
                lastCommandOutput += "3. Click 'Restart ADB Server' button above";
            }
            else
            {
                lastCommandOutput += "⚠ No devices found.\n\n";
                lastCommandOutput += "Verify:\n";
                lastCommandOutput += "1. Quest is powered on\n";
                lastCommandOutput += "2. USB cable is properly connected\n";
                lastCommandOutput += "3. Developer Mode is enabled (via Meta Quest mobile app)\n";
                lastCommandOutput += "4. Try a different USB cable or USB port";
            }
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private void RestartADBServer()
    {
        isProcessing = true;
        lastCommandOutput = "Restarting ADB server...\n\n";
        
        string killOutput = ExecuteADBCommand("kill-server");
        lastCommandOutput += "Kill Server Output:\n" + (string.IsNullOrEmpty(killOutput) ? "(none)" : killOutput) + "\n\n";
        
        System.Threading.Thread.Sleep(1000);
        
        string startOutput = ExecuteADBCommand("start-server");
        lastCommandOutput += "Start Server Output:\n" + (string.IsNullOrEmpty(startOutput) ? "(none)" : startOutput) + "\n\n";
        
        lastCommandOutput += "ADB server restarted.\n";
        lastCommandOutput += "Now check device status again or reconnect your Quest headset.";
        
        isProcessing = false;
        Repaint();
    }
    
    private void ClearADBKeys()
    {
        isProcessing = true;
        lastCommandOutput = "Clearing ADB authorization keys...\n\n";
        
        string adbKeysPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".android", "adbkey");
        string adbKeysPubPath = adbKeysPath + ".pub";
        
        bool clearedAny = false;
        
        try
        {
            if (File.Exists(adbKeysPath))
            {
                File.Delete(adbKeysPath);
                lastCommandOutput += "✓ Deleted: " + adbKeysPath + "\n";
                clearedAny = true;
            }
            else
            {
                lastCommandOutput += "• Key file not found: " + adbKeysPath + "\n";
            }
            
            if (File.Exists(adbKeysPubPath))
            {
                File.Delete(adbKeysPubPath);
                lastCommandOutput += "✓ Deleted: " + adbKeysPubPath + "\n";
                clearedAny = true;
            }
            else
            {
                lastCommandOutput += "• Public key file not found: " + adbKeysPubPath + "\n";
            }
            
            if (clearedAny)
            {
                lastCommandOutput += "\n✓ ADB keys cleared successfully!\n\n";
                lastCommandOutput += "NEXT STEPS:\n";
                lastCommandOutput += "1. Click 'Restart ADB Server' button\n";
                lastCommandOutput += "2. Reconnect your Quest headset\n";
                lastCommandOutput += "3. Look for authorization prompt in headset\n";
                lastCommandOutput += "4. Click 'Check Device Status' to verify";
            }
            else
            {
                lastCommandOutput += "\nNo ADB keys found to clear.\n";
                lastCommandOutput += "This might indicate ADB has not been used before,\n";
                lastCommandOutput += "or keys are stored in a different location.";
            }
        }
        catch (System.Exception e)
        {
            lastCommandOutput += "\nERROR: " + e.Message;
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private void CheckADBVersion()
    {
        isProcessing = true;
        lastCommandOutput = "Checking ADB version...\n\n";
        
        string output = ExecuteADBCommand("version");
        
        if (string.IsNullOrEmpty(output))
        {
            lastCommandOutput += "ERROR: Could not get ADB version.\n";
            lastCommandOutput += "ADB may not be installed or configured correctly.";
        }
        else
        {
            lastCommandOutput += output;
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private void ListDevices()
    {
        isProcessing = true;
        lastCommandOutput = "Listing all connected devices...\n\n";
        
        string output = ExecuteADBCommand("devices -l");
        
        if (string.IsNullOrEmpty(output))
        {
            lastCommandOutput += "ERROR: Could not list devices.";
        }
        else
        {
            lastCommandOutput += output;
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private void GetDeviceProperties()
    {
        isProcessing = true;
        lastCommandOutput = "Getting device properties...\n\n";
        
        string model = ExecuteADBCommand("shell getprop ro.product.model");
        string manufacturer = ExecuteADBCommand("shell getprop ro.product.manufacturer");
        string androidVersion = ExecuteADBCommand("shell getprop ro.build.version.release");
        string buildNumber = ExecuteADBCommand("shell getprop ro.build.display.id");
        
        if (string.IsNullOrEmpty(model) || model.Contains("error"))
        {
            lastCommandOutput += "ERROR: No authorized device connected.\n";
            lastCommandOutput += "Make sure device is connected and authorized.\n";
            lastCommandOutput += "Use 'Check Device Status' to verify connection.";
        }
        else
        {
            lastCommandOutput += "Device Information:\n";
            lastCommandOutput += "─────────────────────\n";
            lastCommandOutput += $"Model: {model.Trim()}\n";
            lastCommandOutput += $"Manufacturer: {manufacturer.Trim()}\n";
            lastCommandOutput += $"Android Version: {androidVersion.Trim()}\n";
            lastCommandOutput += $"Build: {buildNumber.Trim()}\n";
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private string ExecuteADBCommand(string arguments)
    {
        string adbPath = GetADBPath();
        
        if (string.IsNullOrEmpty(adbPath))
        {
            return "ERROR: ADB path not found. Please configure Android SDK in Unity preferences.";
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
            
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) => 
                {
                    if (e.Data != null) output.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (e.Data != null) error.AppendLine(e.Data);
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                if (!process.WaitForExit(PROCESS_TIMEOUT_MS))
                {
                    process.Kill();
                    return "ERROR: Command timed out.";
                }
                
                string result = output.ToString();
                string errorOutput = error.ToString();
                
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    result += "\nErrors:\n" + errorOutput;
                }
                
                return result;
            }
        }
        catch (System.Exception e)
        {
            return $"ERROR: {e.Message}";
        }
    }
    
    private void CheckMultipleADBInstances()
    {
        isProcessing = true;
        lastCommandOutput = "Checking for multiple ADB instances...\n\n";
        
        try
        {
            System.Diagnostics.Process[] adbProcesses = System.Diagnostics.Process.GetProcessesByName("adb");
            
            if (adbProcesses.Length > 1)
            {
                lastCommandOutput += $"⚠ WARNING: Found {adbProcesses.Length} running ADB processes!\n\n";
                lastCommandOutput += "This is likely why you're getting 4 authorization requests.\n";
                lastCommandOutput += "Each ADB instance triggers a separate authorization.\n\n";
                lastCommandOutput += "Detected processes:\n";
                
                foreach (var proc in adbProcesses)
                {
                    try
                    {
                        lastCommandOutput += $"  • PID {proc.Id}: {proc.MainModule?.FileName ?? "Unknown"}\n";
                    }
                    catch
                    {
                        lastCommandOutput += $"  • PID {proc.Id}\n";
                    }
                }
                
                lastCommandOutput += "\n⚠ RECOMMENDED ACTION:\n";
                lastCommandOutput += "Open 'Meta > ADB Conflict Resolver' window\n";
                lastCommandOutput += "and use 'Quick Fix: Kill All & Restart' button\n\n";
                lastCommandOutput += "This will:\n";
                lastCommandOutput += "1. Kill all ADB servers\n";
                lastCommandOutput += "2. Start only one ADB server\n";
                lastCommandOutput += "3. Reduce authorization requests to just ONE\n";
            }
            else if (adbProcesses.Length == 1)
            {
                lastCommandOutput += "✓ Good: Only ONE ADB process running.\n";
                lastCommandOutput += "You should only get one authorization request.\n";
            }
            else
            {
                lastCommandOutput += "No ADB processes currently running.\n";
                lastCommandOutput += "Start ADB server with 'Restart ADB Server' button.\n";
            }
        }
        catch (System.Exception e)
        {
            lastCommandOutput += $"Error checking processes: {e.Message}\n";
        }
        
        isProcessing = false;
        Repaint();
    }
    
    private string GetADBPath()
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
        
        if (!File.Exists(adbPath))
        {
            return null;
        }
        
        return adbPath;
    }
}
