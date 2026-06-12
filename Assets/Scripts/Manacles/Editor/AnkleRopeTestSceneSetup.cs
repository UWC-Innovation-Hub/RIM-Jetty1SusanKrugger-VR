using System.Linq;
using RopeToolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.Scene;

public static class AnkleRopeTestSceneSetup
{
    private const string ScenePath = "Assets/Scenes/TestScenes/KM_Rope_Test.unity";
    private const string Prisoner1Path = "Assets/Prefabs/Characters V001/RIG_Prisoner_1_v002.prefab";
    private const string Prisoner2Path = "Assets/Prefabs/Characters V001/RIG_Prisoner_2_v002.prefab";
    private const string ChainMaterialPath = "Assets/Toolkits/Rope/Examples/00_Main/Materials/Chain.mat";
    private const string ChainMeshPath = "Assets/Toolkits/Rope/Examples/00_Main/Models/Chain.fbx";

    [MenuItem("Tools/Manacles/Rebuild KM Rope Test Authored Setup")]
    public static void BuildKmRopeTestAuthored()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        DestroyRoot(scene, "Ankle_Rope_Test");
        DestroyRoot(scene, "Authored_Ankle_Rope_Test");

        var root = new GameObject("Authored_Ankle_Rope_Test");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = new Vector3(0.0f, 0.0f, 3.0f);

        var prisoner1 = InstantiatePrefab(Prisoner1Path, root.transform, "P1_Authored_Ankle_Test");
        prisoner1.transform.localPosition = new Vector3(-1.2f, 0.0f, 0.0f);
        prisoner1.transform.localRotation = Quaternion.identity;

        var prisoner2 = InstantiatePrefab(Prisoner2Path, root.transform, "P2_Authored_Ankle_Test");
        prisoner2.transform.localPosition = new Vector3(1.2f, 0.0f, 0.0f);
        prisoner2.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        var startAnchor = FindChildByName(prisoner1.transform, "Left_Ankle_Anchor");
        var endAnchor = FindChildByName(prisoner2.transform, "Right_Ankle_Anchor");

        var ropeObject = new GameObject("P1_P2_Authored_Ankle_Chain");
        ropeObject.transform.SetParent(root.transform, false);
        ropeObject.AddComponent<Rope>();

        var manacleRope = ropeObject.AddComponent<AnkleManacleRope>();
        manacleRope.ConfigureForAuthoring(
            startAnchor,
            endAnchor,
            AssetDatabase.LoadAssetAtPath<Material>(ChainMaterialPath),
            LoadChainMesh(),
            0.03f,
            90.0f,
            Vector3.one * 0.45f,
            false,
            50.0f,
            2,
            2,
            0.05f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject InstantiatePrefab(string assetPath, Transform parent, string instanceName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = instanceName;
        instance.transform.SetParent(parent, false);
        return instance;
    }

    private static Mesh LoadChainMesh()
    {
        return AssetDatabase
            .LoadAllAssetsAtPath(ChainMeshPath)
            .OfType<Mesh>()
            .FirstOrDefault(mesh => mesh.name == "Chain")
            ?? AssetDatabase.LoadAssetAtPath<Mesh>(ChainMeshPath);
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        throw new System.InvalidOperationException($"Could not find child '{childName}' under '{root.name}'.");
    }

    private static void DestroyRoot(UnityScene scene, string rootName)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == rootName)
            {
                Object.DestroyImmediate(root);
                return;
            }
        }
    }
}
