using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using System.Collections.Generic;
using System.Linq;

public class MetaXRSettingsValidator : EditorWindow
{
    private const string WINDOW_TITLE = "Meta XR Settings Validator";
    
    private Vector2 scrollPosition;
    private List<ValidationResult> validationResults = new List<ValidationResult>();
    
    private class ValidationResult
    {
        public string category;
        public string message;
        public MessageType type;
        public System.Action fixAction;
        
        public ValidationResult(string category, string message, MessageType type, System.Action fixAction = null)
        {
            this.category = category;
            this.message = message;
            this.type = type;
            this.fixAction = fixAction;
        }
    }
    
    [MenuItem("Meta/XR Settings Validator")]
    public static void ShowWindow()
    {
        var window = GetWindow<MetaXRSettingsValidator>(WINDOW_TITLE);
        window.minSize = new Vector2(500, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Meta XR Settings Validation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool validates your Meta XR SDK configuration and helps fix common setup issues.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Run Full Validation", GUILayout.Height(35)))
        {
            RunValidation();
        }
        
        EditorGUILayout.Space(10);
        
        if (validationResults.Count > 0)
        {
            EditorGUILayout.LabelField("Validation Results:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            
            string currentCategory = "";
            foreach (var result in validationResults)
            {
                if (result.category != currentCategory)
                {
                    currentCategory = result.category;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(currentCategory, EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(result.message, result.type);
                
                if (result.fixAction != null)
                {
                    if (GUILayout.Button("Fix", GUILayout.Width(60), GUILayout.Height(38)))
                    {
                        result.fixAction.Invoke();
                        RunValidation();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Click 'Run Full Validation' to check your Meta XR configuration.", MessageType.None);
        }
    }
    
    private void RunValidation()
    {
        validationResults.Clear();
        
        ValidateBuildTarget();
        ValidateTextureCompression();
        ValidateColorSpace();
        ValidateGraphicsAPI();
        ValidateXRPluginManagement();
        ValidatePlayerSettings();
        ValidateQualitySettings();
        ValidateOculusPlatformSettings();
        
        Repaint();
    }
    
    private void ValidateBuildTarget()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            validationResults.Add(new ValidationResult(
                "Build Target",
                "Build target must be set to Android for Meta Quest development.",
                MessageType.Error,
                () => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android)
            ));
        }
        else
        {
            validationResults.Add(new ValidationResult(
                "Build Target",
                "✓ Build target is correctly set to Android.",
                MessageType.Info
            ));
        }
    }
    
    private void ValidateTextureCompression()
    {
        var textureCompression = EditorUserBuildSettings.androidBuildSubtarget;
        
        if (textureCompression != MobileTextureSubtarget.ASTC)
        {
            validationResults.Add(new ValidationResult(
                "Texture Compression",
                "Texture compression should be set to ASTC for optimal Quest performance.",
                MessageType.Warning,
                () => EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC
            ));
        }
        else
        {
            validationResults.Add(new ValidationResult(
                "Texture Compression",
                "✓ Texture compression is set to ASTC.",
                MessageType.Info
            ));
        }
    }
    
    private void ValidateColorSpace()
    {
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            validationResults.Add(new ValidationResult(
                "Color Space",
                "Linear color space is recommended for better visual quality in VR.",
                MessageType.Warning,
                () => PlayerSettings.colorSpace = ColorSpace.Linear
            ));
        }
        else
        {
            validationResults.Add(new ValidationResult(
                "Color Space",
                "✓ Color space is set to Linear.",
                MessageType.Info
            ));
        }
    }
    
    private void ValidateGraphicsAPI()
    {
        var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        
        if (graphicsAPIs.Length == 0 || graphicsAPIs[0] != UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
        {
            validationResults.Add(new ValidationResult(
                "Graphics API",
                "Vulkan should be the primary graphics API for Meta Quest.",
                MessageType.Warning,
                () => 
                {
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan });
                }
            ));
        }
        else
        {
            validationResults.Add(new ValidationResult(
                "Graphics API",
                "✓ Vulkan is set as the graphics API.",
                MessageType.Info
            ));
        }
    }
    
    private void ValidateXRPluginManagement()
    {
        var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        
        if (settings == null || settings.Manager == null || settings.Manager.activeLoaders.Count == 0)
        {
            validationResults.Add(new ValidationResult(
                "XR Plugin Management",
                "No XR loaders are active. OpenXR or Oculus loader must be enabled.",
                MessageType.Error
            ));
        }
        else
        {
            bool hasMetaLoader = settings.Manager.activeLoaders.Any(loader => 
                loader.GetType().Name.Contains("Oculus") || 
                loader.GetType().Name.Contains("OpenXR") ||
                loader.GetType().Name.Contains("Meta"));
            
            if (hasMetaLoader)
            {
                validationResults.Add(new ValidationResult(
                    "XR Plugin Management",
                    "✓ Meta/OpenXR loader is active.",
                    MessageType.Info
                ));
            }
            else
            {
                validationResults.Add(new ValidationResult(
                    "XR Plugin Management",
                    "No Meta or OpenXR loader found. Please enable it in Project Settings > XR Plug-in Management.",
                    MessageType.Error
                ));
            }
        }
    }
    
    private void ValidatePlayerSettings()
    {
        List<string> issues = new List<string>();
        List<System.Action> fixes = new List<System.Action>();
        
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
        {
            issues.Add("Minimum API Level should be 29 or higher for Quest support");
            fixes.Add(() => PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29);
        }
        
        if (!PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString().Contains("IL2CPP"))
        {
            issues.Add("IL2CPP scripting backend is required for Quest builds");
            fixes.Add(() => PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP));
        }
        
        var targetArchitectures = PlayerSettings.Android.targetArchitectures;
        if (targetArchitectures != AndroidArchitecture.ARM64)
        {
            issues.Add("Target architecture should be ARM64");
            fixes.Add(() => PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64);
        }
        
        if (issues.Count > 0)
        {
            validationResults.Add(new ValidationResult(
                "Player Settings",
                "Issues found:\n• " + string.Join("\n• ", issues),
                MessageType.Warning,
                () => 
                {
                    foreach (var fix in fixes)
                    {
                        fix.Invoke();
                    }
                }
            ));
        }
        else
        {
            validationResults.Add(new ValidationResult(
                "Player Settings",
                "✓ Player settings are correctly configured for Quest.",
                MessageType.Info
            ));
        }
    }
    
    private void ValidateQualitySettings()
    {
        string[] qualityLevelNames = QualitySettings.names;
        int currentQuality = QualitySettings.GetQualityLevel();
        
        validationResults.Add(new ValidationResult(
            "Quality Settings",
            $"Current quality level: {qualityLevelNames[currentQuality]}\n" +
            "Ensure this is optimized for mobile VR performance.",
            MessageType.Info
        ));
        
        if (QualitySettings.antiAliasing > 2)
        {
            validationResults.Add(new ValidationResult(
                "Quality Settings - Anti-Aliasing",
                "Anti-aliasing is set higher than 2x. This may impact performance on Quest.",
                MessageType.Warning,
                () => QualitySettings.antiAliasing = 2
            ));
        }
    }
    
    private void ValidateOculusPlatformSettings()
    {
        var oculusPlatformSettings = Resources.Load("OculusPlatformSettings");
        
        if (oculusPlatformSettings == null)
        {
            validationResults.Add(new ValidationResult(
                "Oculus Platform Settings",
                "OculusPlatformSettings not found. This is optional unless you're using platform features.",
                MessageType.Info
            ));
            return;
        }
        
        var settingsType = oculusPlatformSettings.GetType();
        var appIdField = settingsType.GetField("AppID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (appIdField != null)
        {
            var appId = appIdField.GetValue(oculusPlatformSettings) as string;
            
            if (string.IsNullOrEmpty(appId))
            {
                validationResults.Add(new ValidationResult(
                    "Oculus Platform Settings",
                    "Application ID is not set in Oculus Platform Settings.\n" +
                    "Set this in: Oculus > Platform > Edit Settings (if using platform features).",
                    MessageType.Warning
                ));
            }
            else
            {
                validationResults.Add(new ValidationResult(
                    "Oculus Platform Settings",
                    $"✓ Application ID is configured.",
                    MessageType.Info
                ));
            }
        }
    }
}
