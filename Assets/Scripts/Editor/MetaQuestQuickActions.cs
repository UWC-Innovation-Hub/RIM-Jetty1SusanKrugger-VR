using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class MetaQuestQuickActions : EditorWindow
{
    private const string WINDOW_TITLE = "Meta Quest Quick Actions";
    
    [MenuItem("Meta/Quick Actions")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaQuestQuickActions>(WINDOW_TITLE);
        window.minSize = new Vector2(350, 450);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Meta Quest Development Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Quick access to common Meta Quest development actions and resources.", MessageType.Info);
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Unity Settings", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Open XR Plug-in Management", GUILayout.Height(30)))
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
        
        if (GUILayout.Button("Open Player Settings", GUILayout.Height(30)))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        
        if (GUILayout.Button("Open Build Settings", GUILayout.Height(30)))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("External Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Open Android SDK Location", GUILayout.Height(30)))
        {
            SettingsService.OpenUserPreferences("Preferences/External Tools");
        }
        
        if (GUILayout.Button("Open Meta Quest Developer Hub", GUILayout.Height(30)))
        {
            OpenMetaQuestDeveloperHub();
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Documentation & Resources", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Meta XR SDK Documentation", GUILayout.Height(30)))
        {
            Application.OpenURL("https://developer.oculus.com/documentation/unity/");
        }
        
        if (GUILayout.Button("Unity XR Documentation", GUILayout.Height(30)))
        {
            Application.OpenURL("https://docs.unity3d.com/6000.2/Documentation/Manual/XR.html");
        }
        
        if (GUILayout.Button("Quest Device Setup Guide", GUILayout.Height(30)))
        {
            Application.OpenURL("https://developer.oculus.com/documentation/unity/unity-env-device-setup/");
        }
        
        if (GUILayout.Button("ADB Troubleshooting Guide", GUILayout.Height(30)))
        {
            Application.OpenURL("https://developer.android.com/tools/adb");
        }
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Project Information", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Unity Version:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(Application.unityVersion, EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Build Target:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(EditorUserBuildSettings.activeBuildTarget.ToString(), EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.Space(5);
        
        string androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        if (string.IsNullOrEmpty(androidSdkPath))
        {
            androidSdkPath = "Not configured";
        }
        
        EditorGUILayout.LabelField("Android SDK Path:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(androidSdkPath, EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void OpenMetaQuestDeveloperHub()
    {
        string mqdhPath = "";
        
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            mqdhPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "Programs", "meta-quest-developer-hub", "Meta Quest Developer Hub.exe"
            );
            
            if (!System.IO.File.Exists(mqdhPath))
            {
                mqdhPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
                    "Meta Quest Developer Hub", "Meta Quest Developer Hub.exe"
                );
            }
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            mqdhPath = "/Applications/Meta Quest Developer Hub.app";
        }
        
        if (!string.IsNullOrEmpty(mqdhPath) && (System.IO.File.Exists(mqdhPath) || System.IO.Directory.Exists(mqdhPath)))
        {
            try
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start(mqdhPath);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start("open", $"\"{mqdhPath}\"");
                }
                
                UnityEngine.Debug.Log("Opening Meta Quest Developer Hub...");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to open Meta Quest Developer Hub:\n{e.Message}", 
                    "OK");
            }
        }
        else
        {
            bool download = EditorUtility.DisplayDialog("Meta Quest Developer Hub Not Found", 
                "Meta Quest Developer Hub is not installed on your system.\n\n" +
                "Would you like to download it from the Meta Developer website?", 
                "Yes", "No");
            
            if (download)
            {
                Application.OpenURL("https://developer.oculus.com/meta-quest-developer-hub/");
            }
        }
    }
}
