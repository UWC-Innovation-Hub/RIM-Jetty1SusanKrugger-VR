using RopeToolkit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rope))]
public class AnkleManacleRope : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Transform startAnchor;
    [SerializeField] private Transform endAnchor;
    [SerializeField] private RopeConnection startConnection;
    [SerializeField] private RopeConnection endConnection;

    [Header("Rope Visuals")]
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private Mesh customChainMesh;
    [SerializeField] private float radius = 0.03f;
    [SerializeField] private float customMeshRotation = 90.0f;
    [SerializeField] private Vector3 customMeshScale = Vector3.one * 0.45f;
    [SerializeField] private bool stretchCustomMesh = false;
    [SerializeField] private ShadowCastingMode shadowMode = ShadowCastingMode.Off;

    [Header("Rope Simulation")]
    [SerializeField] private float resolution = 50.0f;
    [SerializeField] private float massPerMeter = 0.2f;
    [SerializeField, Range(0.01f, 1.0f)] private float stiffness = 1.0f;
    [SerializeField, Range(1, 10)] private int substeps = 2;
    [SerializeField, Range(1, 32)] private int solverIterations = 2;
    [SerializeField] private float sagAmount = 0.05f;

    [Header("Runtime")]
    [SerializeField] private bool resetToSpawnCurveOnStart = true;
    [SerializeField] private bool resetToSpawnCurveOnReEnable = true;

    private Rope rope;
    private bool started;

    private void Awake()
    {
        ResolveRope();
    }

    private void Start()
    {
        started = true;

        if (resetToSpawnCurveOnStart)
        {
            ResetRopeToSpawnCurve();
        }
    }

    private void OnEnable()
    {
        if (started && resetToSpawnCurveOnReEnable)
        {
            ResetRopeToSpawnCurve();
        }
    }

    [ContextMenu("Apply Rope Settings")]
    public void ApplyRopeSettings()
    {
        ResolveRope();

        rope.radius = radius;
        rope.radialVertices = 6;
        rope.material = ropeMaterial;
        rope.shadowMode = shadowMode;
        rope.customMesh.mesh = customChainMesh;
        rope.customMesh.rotation = customMeshRotation;
        rope.customMesh.scale = customMeshScale;
        rope.customMesh.stretch = stretchCustomMesh;
        rope.interpolation = RopeInterpolation.None;

        rope.simulation.enabled = true;
        rope.simulation.resolution = resolution;
        rope.simulation.massPerMeter = massPerMeter;
        rope.simulation.stiffness = stiffness;
        rope.simulation.energyLoss = 0.0025f;
        rope.simulation.lengthMultiplier = 1.0f;
        rope.simulation.gravityMultiplier = 1.0f;
        rope.simulation.useCustomGravity = false;
        rope.simulation.customGravity = Physics.gravity;
        rope.simulation.substeps = substeps;
        rope.simulation.solverIterations = solverIterations;

        rope.collisions.enabled = false;
        rope.collisions.influenceRigidbodies = false;

        ConfigureConnection(ref startConnection, startAnchor, 0.0f);
        ConfigureConnection(ref endConnection, endAnchor, 1.0f);

        WarnAboutAuthoringIssues();
        MarkDirtyForEditor();
    }

    [ContextMenu("Bake Spawn Points From Anchors")]
    public void BakeSpawnPointsFromAnchors()
    {
        ResolveRope();

        if (!startAnchor || !endAnchor)
        {
            Debug.LogError($"{nameof(AnkleManacleRope)} on {name} needs both anchors before spawn points can be baked.", this);
            return;
        }

        var ropeTransform = rope.transform;
        var startWorld = startAnchor.position;
        var endWorld = endAnchor.position;

        rope.spawnPoints.Clear();
        rope.spawnPoints.Add((float3)ropeTransform.InverseTransformPoint(startWorld));

        if (sagAmount > 0.0f)
        {
            var middleWorld = Vector3.Lerp(startWorld, endWorld, 0.5f) + Vector3.down * sagAmount;
            rope.spawnPoints.Add((float3)ropeTransform.InverseTransformPoint(middleWorld));
        }

        rope.spawnPoints.Add((float3)ropeTransform.InverseTransformPoint(endWorld));
        MarkDirtyForEditor();
    }

    [ContextMenu("Reset Rope To Spawn Curve")]
    public void ResetRopeToSpawnCurve()
    {
        ResolveRope();

        if (Application.isPlaying)
        {
            rope.ResetToSpawnCurve();
        }
    }

    public void ConfigureForAuthoring(
        Transform start,
        Transform end,
        Material material,
        Mesh chainMesh,
        float ropeRadius,
        float chainMeshRotation,
        Vector3 chainMeshScale,
        bool chainMeshStretch,
        float ropeResolution,
        int ropeSubsteps,
        int ropeSolverIterations,
        float ropeSagAmount)
    {
        startAnchor = start;
        endAnchor = end;
        ropeMaterial = material;
        customChainMesh = chainMesh;
        radius = ropeRadius;
        customMeshRotation = chainMeshRotation;
        customMeshScale = chainMeshScale;
        stretchCustomMesh = chainMeshStretch;
        resolution = ropeResolution;
        substeps = ropeSubsteps;
        solverIterations = ropeSolverIterations;
        sagAmount = ropeSagAmount;

        ApplyRopeSettings();
        BakeSpawnPointsFromAnchors();
    }

    private void ResolveRope()
    {
        if (!rope)
        {
            rope = GetComponent<Rope>();
        }
    }

    private void ConfigureConnection(ref RopeConnection connection, Transform anchor, float ropeLocation)
    {
        if (!connection)
        {
            connection = gameObject.AddComponent<RopeConnection>();
        }

        connection.type = RopeConnectionType.PinRopeToTransform;
        connection.ropeLocation = ropeLocation;
        connection.autoFindRopeLocation = false;
        connection.transformSettings.transform = anchor;
        connection.localConnectionPoint = float3.zero;
    }

    private void WarnAboutAuthoringIssues()
    {
        if (!startAnchor || !endAnchor)
        {
            Debug.LogWarning($"{nameof(AnkleManacleRope)} on {name} is missing one or both anchors.", this);
        }

        if (!ropeMaterial)
        {
            Debug.LogWarning($"{nameof(AnkleManacleRope)} on {name} has no rope material assigned.", this);
        }
        else if (!ropeMaterial.enableInstancing)
        {
            Debug.LogWarning($"{ropeMaterial.name} should have GPU instancing enabled for Rope Toolkit custom mesh rendering.", ropeMaterial);
        }

        if (!customChainMesh)
        {
            Debug.LogWarning($"{nameof(AnkleManacleRope)} on {name} has no custom chain mesh assigned.", this);
        }
    }

    private void MarkDirtyForEditor()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (rope)
            {
                UnityEditor.EditorUtility.SetDirty(rope);
            }
            if (startConnection)
            {
                UnityEditor.EditorUtility.SetDirty(startConnection);
            }
            if (endConnection)
            {
                UnityEditor.EditorUtility.SetDirty(endConnection);
            }
        }
#endif
    }
}
