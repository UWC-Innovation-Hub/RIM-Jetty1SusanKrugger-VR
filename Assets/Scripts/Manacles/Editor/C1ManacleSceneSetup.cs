using System;
using System.Linq;
using RopeToolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityScene = UnityEngine.SceneManagement.Scene;

public static class C1ManacleSceneSetup
{
    private const string ScenePath = "Assets/Scenes/TestScenes/C1_Final_Pass_TEST_Beta.unity";
    private const string ChainMaterialPath = "Assets/Toolkits/Rope/Examples/00_Main/Materials/Chain.mat";
    private const string ChainMeshPath = "Assets/Toolkits/Rope/Examples/00_Main/Models/Chain.fbx";

    private const float Radius = 0.03f;
    private const float CustomMeshRotation = 90.0f;
    private static readonly Vector3 CustomMeshScale = Vector3.one * 0.45f;
    private const bool StretchCustomMesh = false;
    private const float Resolution = 50.0f;
    private const int Substeps = 2;
    private const int SolverIterations = 2;
    private const float SagAmount = 0.05f;

    [MenuItem("Tools/Manacles/Wire C1 Final Pass Manacles")]
    public static void WireC1Manacles()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var material = AssetDatabase.LoadAssetAtPath<Material>(ChainMaterialPath);
        var mesh = LoadChainMesh();

        if (!material)
            throw new InvalidOperationException($"Missing chain material at {ChainMaterialPath}.");

        if (!mesh)
            throw new InvalidOperationException($"Missing chain mesh at {ChainMeshPath}.");

        material.enableInstancing = true;
        EditorUtility.SetDirty(material);

        var cargoPrisoners = RequireTransform(scene, "Prisoners_Batch_CargoScene");
        var cargoChains = RequireGameObject(scene, "CargoScene_Manacles");
        cargoChains.SetActive(true);

        ConfigurePair(scene, cargoPrisoners, "Cargo_P1_P2_Chain", 0, 1, material, mesh);
        ConfigurePair(scene, cargoPrisoners, "Cargo_P3_P4_Chain", 2, 3, material, mesh);
        ConfigurePair(scene, cargoPrisoners, "Cargo_P5_P6_Chain", 4, 5, material, mesh);

        var batchAChains = RequireGameObject(scene, "Batch_A_Chains");
        var batchBChains = RequireGameObject(scene, "Batch_B_Chains");
        var batchCChains = RequireGameObject(scene, "Batch_C_Chains");

        ConfigureBatch(scene, "Prisoners_Batch_A", new[]
        {
            new PairSpec("Batch_A_P1_P2_Chain", 0, 1),
            new PairSpec("Batch_A_P3_P4_Chain", 2, 3),
            new PairSpec("Batch_A_P5_P6_Chain", 4, 5),
        }, material, mesh);

        ConfigureBatch(scene, "Prisoners_Batch_B", new[]
        {
            new PairSpec("Batch_B_P1_P2_Chain", 0, 1),
            new PairSpec("Batch_B_P3_P4_Chain", 2, 3),
        }, material, mesh);

        ConfigureBatch(scene, "Prisoners_Batch_C", new[]
        {
            new PairSpec("Batch_C_P1_P2_Chain", 0, 1),
        }, material, mesh);

        WirePrisonerSortLinkedObjects(scene, batchBChains);

        batchAChains.SetActive(false);
        batchBChains.SetActive(false);
        batchCChains.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("C1 manacle chains wired successfully.");
    }

    private static void ConfigureBatch(UnityScene scene, string prisonerRootName, PairSpec[] pairs, Material material, Mesh mesh)
    {
        var prisonerRoot = RequireTransform(scene, prisonerRootName);

        foreach (var pair in pairs)
            ConfigurePair(scene, prisonerRoot, pair.ChainName, pair.StartIndex, pair.EndIndex, material, mesh);
    }

    private static void ConfigurePair(
        UnityScene scene,
        Transform prisonerRoot,
        string chainObjectName,
        int startPrisonerIndex,
        int endPrisonerIndex,
        Material material,
        Mesh mesh)
    {
        var startPrisoner = RequireChild(prisonerRoot, startPrisonerIndex);
        var endPrisoner = RequireChild(prisonerRoot, endPrisonerIndex);
        var startAnchor = FindOrCreateAnchor(startPrisoner, "Left_Ankle_Anchor", true);
        var endAnchor = FindOrCreateAnchor(endPrisoner, "Right_Ankle_Anchor", false);
        var chainObject = RequireGameObject(scene, chainObjectName);

        var rope = EnsureComponent<Rope>(chainObject);
        rope.shadowMode = ShadowCastingMode.Off;

        var connections = EnsureTwoConnections(chainObject);
        var manacle = EnsureComponent<AnkleManacleRope>(chainObject);
        AssignConnections(manacle, connections[0], connections[1]);
        manacle.ConfigureForAuthoring(
            startAnchor,
            endAnchor,
            material,
            mesh,
            Radius,
            CustomMeshRotation,
            CustomMeshScale,
            StretchCustomMesh,
            Resolution,
            Substeps,
            SolverIterations,
            SagAmount);

        EditorUtility.SetDirty(chainObject);
        EditorUtility.SetDirty(rope);
        EditorUtility.SetDirty(manacle);
        EditorUtility.SetDirty(connections[0]);
        EditorUtility.SetDirty(connections[1]);
    }

    private static RopeConnection[] EnsureTwoConnections(GameObject chainObject)
    {
        var connections = chainObject.GetComponents<RopeConnection>().ToList();

        while (connections.Count < 2)
            connections.Add(chainObject.AddComponent<RopeConnection>());

        for (int i = connections.Count - 1; i >= 2; i--)
            UnityEngine.Object.DestroyImmediate(connections[i]);

        return chainObject.GetComponents<RopeConnection>().Take(2).ToArray();
    }

    private static void AssignConnections(AnkleManacleRope manacle, RopeConnection startConnection, RopeConnection endConnection)
    {
        var serialized = new SerializedObject(manacle);
        serialized.FindProperty("startConnection").objectReferenceValue = startConnection;
        serialized.FindProperty("endConnection").objectReferenceValue = endConnection;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WirePrisonerSortLinkedObjects(UnityScene scene, GameObject retainedBatchChains)
    {
        var interactionRoot = RequireGameObject(scene, "Interaction_PrisonerSort");
        var module = interactionRoot.GetComponent<PrisonerSortModule>();

        if (!module)
            throw new InvalidOperationException("Interaction_PrisonerSort is missing PrisonerSortModule.");

        var serialized = new SerializedObject(module);
        var linkedObjects = serialized.FindProperty("linkedObjects");

        if (linkedObjects == null || !linkedObjects.isArray)
            throw new InvalidOperationException("PrisonerSortModule is missing linkedObjects.");

        linkedObjects.arraySize = 1;
        linkedObjects.GetArrayElementAtIndex(0).objectReferenceValue = retainedBatchChains;

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(module);
    }

    private static Mesh LoadChainMesh()
    {
        return AssetDatabase
            .LoadAllAssetsAtPath(ChainMeshPath)
            .OfType<Mesh>()
            .FirstOrDefault(mesh => mesh.name == "Chain")
            ?? AssetDatabase.LoadAssetAtPath<Mesh>(ChainMeshPath);
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }

    private static GameObject RequireGameObject(UnityScene scene, string objectName)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindDescendant(root.transform, objectName);
            if (found)
                return found.gameObject;
        }

        throw new InvalidOperationException($"Could not find GameObject '{objectName}' in {scene.path}.");
    }

    private static Transform RequireTransform(UnityScene scene, string objectName)
    {
        return RequireGameObject(scene, objectName).transform;
    }

    private static Transform RequireChild(Transform parent, int index)
    {
        if (parent.childCount <= index)
            throw new InvalidOperationException($"{parent.name} does not have child index {index}.");

        return parent.GetChild(index);
    }

    private static Transform FindOrCreateAnchor(Transform root, string anchorName, bool leftSide)
    {
        var found = FindDescendant(root, anchorName);
        if (found)
            return found;

        var bone = FindFallbackAnkleBone(root, leftSide);
        var anchor = new GameObject(anchorName).transform;
        anchor.SetParent(bone, false);
        anchor.localPosition = Vector3.zero;
        anchor.localRotation = Quaternion.identity;
        anchor.localScale = Vector3.one;

        Debug.LogWarning($"Created missing {anchorName} under {bone.name} for {root.name}.", anchor);
        EditorUtility.SetDirty(anchor.gameObject);
        return anchor;
    }

    private static Transform FindFallbackAnkleBone(Transform root, bool leftSide)
    {
        string[] candidates = leftSide
            ? new[] { "DEF-foot.L", "DEF-shin.L", "DEF-shin.L.001" }
            : new[] { "DEF-foot.R", "DEF-shin.R", "DEF-shin.R.001" };

        foreach (string candidate in candidates)
        {
            var found = FindDescendant(root, candidate);
            if (found)
                return found;
        }

        throw new InvalidOperationException($"Could not find a fallback ankle/foot bone under '{root.name}'.");
    }

    private static Transform FindDescendant(Transform root, string childName)
    {
        if (root.name == childName)
            return root;

        foreach (Transform child in root)
        {
            var found = FindDescendant(child, childName);
            if (found)
                return found;
        }

        return null;
    }

    private readonly struct PairSpec
    {
        public readonly string ChainName;
        public readonly int StartIndex;
        public readonly int EndIndex;

        public PairSpec(string chainName, int startIndex, int endIndex)
        {
            ChainName = chainName;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}
