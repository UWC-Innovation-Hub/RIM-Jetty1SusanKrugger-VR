using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using System.Linq;
using System.Reflection;

public class MetaSDKDiagnostics : EditorWindow
{
    private const string WINDOW_TITLE = "Meta SDK Diagnostics";
    
    private Vector2 scrollPosition;
    private string diagnosticReport = "";
    
    [MenuItem("Meta/SDK Diagnostics")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaSDKDiagnostics>(WINDOW_TITLE);
        window.minSize = new Vector2(600, 700);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Meta SDK vs Unity XR Diagnostics", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool helps diagnose why Meta SDK behaves differently than Unity XR.\n" +
            "It checks for Meta-specific configurations that might cause connection issues.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Run Full Meta SDK Diagnostic", GUILayout.Height(40)))
        {
            RunDiagnostics();
        }
        
        EditorGUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(diagnosticReport))
        {
            EditorGUILayout.LabelField("Diagnostic Report:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(diagnosticReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Common Meta SDK-specific issues:\n" +
            "• OVR Manager settings conflicts\n" +
            "• Multiple XR loaders enabled simultaneously\n" +
            "• Oculus Platform entitlement checks\n" +
            "• Meta-specific Android manifest entries\n" +
            "• Build hooks triggering additional ADB processes",
            MessageType.None);
    }
    
    private void RunDiagnostics()
    {
        diagnosticReport = "=== META SDK DIAGNOSTICS REPORT ===\n";
        diagnosticReport += $"Generated: {System.DateTime.Now}\n\n";
        
        CheckXRLoaders();
        CheckOVRManager();
        CheckOculusPlatformSettings();
        CheckAndroidManifest();
        CheckMetaXRPackages();
        CheckBuildSettings();
        CheckProjectValidation();
        
        diagnosticReport += "\n=== END OF REPORT ===\n";
        
        Repaint();
    }
    
    private void CheckXRLoaders()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "1. XR LOADER CONFIGURATION\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        
        if (settings == null || settings.Manager == null)
        {
            diagnosticReport += "⚠ ERROR: XR Plugin Management not configured!\n";
            diagnosticReport += "   Go to: Edit > Project Settings > XR Plug-in Management\n\n";
            return;
        }
        
        var loaders = settings.Manager.activeLoaders;
        diagnosticReport += $"Active Loaders: {loaders.Count}\n\n";
        
        bool hasOculus = false;
        bool hasOpenXR = false;
        bool hasMetaOpenXR = false;
        
        foreach (var loader in loaders)
        {
            string loaderName = loader.GetType().FullName;
            diagnosticReport += $"  • {loaderName}\n";
            
            if (loaderName.Contains("Oculus"))
            {
                hasOculus = true;
                diagnosticReport += "    ↳ Legacy Oculus loader detected\n";
            }
            if (loaderName.Contains("OpenXR") && !loaderName.Contains("Meta"))
            {
                hasOpenXR = true;
                diagnosticReport += "    ↳ Standard OpenXR loader\n";
            }
            if (loaderName.Contains("Meta") && loaderName.Contains("OpenXR"))
            {
                hasMetaOpenXR = true;
                diagnosticReport += "    ↳ Meta OpenXR loader (recommended)\n";
            }
        }
        
        diagnosticReport += "\n";
        
        if (loaders.Count > 1)
        {
            diagnosticReport += "⚠ WARNING: Multiple XR loaders enabled!\n";
            diagnosticReport += "   This can cause conflicts. Recommended: Use only Meta OpenXR.\n\n";
        }
        
        if (hasOculus && hasOpenXR)
        {
            diagnosticReport += "⚠ CONFLICT: Both Oculus and OpenXR loaders active!\n";
            diagnosticReport += "   Remove the legacy Oculus loader.\n\n";
        }
        
        if (!hasMetaOpenXR && !hasOculus && !hasOpenXR)
        {
            diagnosticReport += "✗ ERROR: No Meta/Oculus XR loader found!\n\n";
        }
        else if (hasMetaOpenXR)
        {
            diagnosticReport += "✓ Meta OpenXR loader is active (correct for Meta SDK)\n\n";
        }
    }
    
    private void CheckOVRManager()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "2. OVR MANAGER CONFIGURATION\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        var ovrManagerType = System.Type.GetType("OVRManager, Assembly-CSharp");
        if (ovrManagerType == null)
        {
            ovrManagerType = System.Type.GetType("OVRManager, Oculus.VR");
        }
        
        if (ovrManagerType == null)
        {
            diagnosticReport += "✗ OVRManager class not found\n";
            diagnosticReport += "  Meta XR SDK may not be properly installed\n\n";
            return;
        }
        
        diagnosticReport += "✓ OVRManager class found\n";
        
        var ovrManager = FindObjectOfType(ovrManagerType);
        if (ovrManager == null)
        {
            diagnosticReport += "⚠ No OVRManager in current scene\n";
            diagnosticReport += "  Add OVRCameraRig to your scene for Meta XR features\n\n";
        }
        else
        {
            diagnosticReport += "✓ OVRManager instance found in scene\n";
            
            var targetDeviceField = ovrManagerType.GetField("_targetDevice", BindingFlags.NonPublic | BindingFlags.Instance);
            if (targetDeviceField != null)
            {
                var targetDevice = targetDeviceField.GetValue(ovrManager);
                diagnosticReport += $"  Target Device: {targetDevice}\n";
            }
            
            diagnosticReport += "\n";
        }
    }
    
    private void CheckOculusPlatformSettings()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "3. OCULUS PLATFORM SETTINGS\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        var platformSettings = Resources.Load("OculusPlatformSettings");
        
        if (platformSettings == null)
        {
            diagnosticReport += "• OculusPlatformSettings not found\n";
            diagnosticReport += "  (Optional - only needed for platform features)\n\n";
            return;
        }
        
        var settingsType = platformSettings.GetType();
        
        var appIdField = settingsType.GetField("AppID", BindingFlags.Public | BindingFlags.Instance);
        if (appIdField != null)
        {
            var appId = appIdField.GetValue(platformSettings) as string;
            
            if (string.IsNullOrEmpty(appId))
            {
                diagnosticReport += "⚠ Application ID is empty\n";
                diagnosticReport += "  This can cause entitlement check failures\n";
                diagnosticReport += "  Set in: Oculus > Platform > Edit Settings\n\n";
            }
            else
            {
                diagnosticReport += $"✓ Application ID configured: {appId.Substring(0, System.Math.Min(10, appId.Length))}...\n\n";
            }
        }
        
        var skipEntitlementCheckField = settingsType.GetField("SkipEntitlementCheck", BindingFlags.Public | BindingFlags.Instance);
        if (skipEntitlementCheckField != null)
        {
            var skipCheck = (bool)skipEntitlementCheckField.GetValue(platformSettings);
            
            if (!skipCheck)
            {
                diagnosticReport += "⚠ Entitlement check is ENABLED\n";
                diagnosticReport += "  This requires valid App ID and can prevent launching\n";
                diagnosticReport += "  For development, consider enabling 'Skip Entitlement Check'\n\n";
            }
            else
            {
                diagnosticReport += "✓ Entitlement check is DISABLED (good for development)\n\n";
            }
        }
    }
    
    private void CheckAndroidManifest()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "4. ANDROID MANIFEST\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
        
        if (!System.IO.File.Exists(manifestPath))
        {
            diagnosticReport += "• No custom AndroidManifest.xml found\n";
            diagnosticReport += "  Using Unity's default manifest\n\n";
        }
        else
        {
            diagnosticReport += "✓ Custom AndroidManifest.xml found\n";
            
            string manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            if (manifestContent.Contains("com.oculus.intent.category.VR"))
            {
                diagnosticReport += "  ✓ Contains Oculus VR category intent\n";
            }
            else
            {
                diagnosticReport += "  ⚠ Missing Oculus VR category intent\n";
            }
            
            if (manifestContent.Contains("android.permission.RECORD_AUDIO"))
            {
                diagnosticReport += "  • RECORD_AUDIO permission found\n";
            }
            
            if (manifestContent.Contains("com.oculus.supportedDevices"))
            {
                diagnosticReport += "  ✓ Supported devices metadata present\n";
            }
            
            diagnosticReport += "\n";
        }
    }
    
    private void CheckMetaXRPackages()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "5. META XR PACKAGES\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        var packageList = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
        
        var metaPackages = packageList.Where(p => 
            p.name.Contains("meta") || 
            p.name.Contains("oculus") || 
            p.name.Contains("xr")).ToArray();
        
        if (metaPackages.Length == 0)
        {
            diagnosticReport += "✗ No Meta XR packages found!\n\n";
            return;
        }
        
        diagnosticReport += $"Found {metaPackages.Length} XR-related packages:\n\n";
        
        foreach (var package in metaPackages)
        {
            diagnosticReport += $"  • {package.name} @ {package.version}\n";
            
            if (package.name == "com.meta.xr.sdk.all")
            {
                diagnosticReport += "    ↳ Meta XR All-in-One SDK (primary package)\n";
            }
        }
        
        diagnosticReport += "\n";
        
        bool hasMetaSDK = metaPackages.Any(p => p.name == "com.meta.xr.sdk.all");
        
        if (hasMetaSDK)
        {
            diagnosticReport += "✓ Meta XR All-in-One SDK is installed\n\n";
        }
        else
        {
            diagnosticReport += "⚠ Meta XR All-in-One SDK not found\n";
            diagnosticReport += "  Install via: Window > Package Manager > My Registries\n\n";
        }
    }
    
    private void CheckBuildSettings()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "6. BUILD CONFIGURATION\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        diagnosticReport += $"Build Target: {EditorUserBuildSettings.activeBuildTarget}\n";
        
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            diagnosticReport += "✗ ERROR: Build target is not Android!\n";
            diagnosticReport += "  Switch to Android in File > Build Settings\n\n";
            return;
        }
        else
        {
            diagnosticReport += "✓ Build target is Android\n";
        }
        
        diagnosticReport += $"Scripting Backend: {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}\n";
        
        if (!PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString().Contains("IL2CPP"))
        {
            diagnosticReport += "⚠ WARNING: Should use IL2CPP for Quest\n";
        }
        else
        {
            diagnosticReport += "✓ Using IL2CPP\n";
        }
        
        diagnosticReport += $"Target Architecture: {PlayerSettings.Android.targetArchitectures}\n";
        
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
        {
            diagnosticReport += "⚠ WARNING: Should target ARM64 for Quest\n";
        }
        else
        {
            diagnosticReport += "✓ Targeting ARM64\n";
        }
        
        diagnosticReport += $"Minimum API Level: {PlayerSettings.Android.minSdkVersion}\n\n";
    }
    
    private void CheckProjectValidation()
    {
        diagnosticReport += "──────────────────────────────────\n";
        diagnosticReport += "7. RECOMMENDATIONS\n";
        diagnosticReport += "──────────────────────────────────\n";
        
        diagnosticReport += "Based on this diagnostic:\n\n";
        
        diagnosticReport += "1. Check 'Meta > ADB Conflict Resolver' for ADB issues\n";
        diagnosticReport += "2. Ensure only Meta OpenXR loader is enabled\n";
        diagnosticReport += "3. Use Meta's Project Setup Tool:\n";
        diagnosticReport += "   Meta > Tools > Project Setup Tool\n";
        diagnosticReport += "4. Verify OVRCameraRig is in your scene\n";
        diagnosticReport += "5. For development, disable entitlement checks\n\n";
        
        diagnosticReport += "If Unity XR works but Meta SDK doesn't:\n";
        diagnosticReport += "• The issue is likely Meta-specific configuration\n";
        diagnosticReport += "• Or multiple ADB servers (use ADB Conflict Resolver)\n";
        diagnosticReport += "• Or Meta Quest Developer Hub running in background\n\n";
    }
}
