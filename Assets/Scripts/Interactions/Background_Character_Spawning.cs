using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdPathPool : MonoBehaviour
{
    [Header("Character Prefabs")]
    public List<GameObject> characterPrefabs = new List<GameObject>();

    [Header("Pool Settings")]
    public int characterDensity = 20;

    [Header("Speed")]
    public float minSpeed = 1f;
    public float maxSpeed = 2f;

    [Header("Animation")]
    public float animationSpeedMultiplier = 1f;

    [Header("Scale")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    [Header("Rotation")]
    public float rotationSpeed = 6f;

    [Header("Path Points")]
    public List<Transform> pathPoints = new List<Transform>();

    [Header("Path Offset")]
    public float pathOffsetRadius = 0.5f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2f;

    [Header("Prewarm")]
    public bool prewarm = true;

    class PooledCharacter
    {
        public GameObject obj;
        public float speed;
        public int targetIndex;
        public bool active;
        public Vector3 offset;
        public Animator animator;
    }

    List<PooledCharacter> pool = new List<PooledCharacter>();
    Queue<PooledCharacter> available = new Queue<PooledCharacter>();

    void Start()
    {
        CreatePool();

        if (prewarm)
            PrewarmCharacters();

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        UpdateCharacters();
    }

    void CreatePool()
    {
        for (int i = 0; i < characterDensity; i++)
        {
            GameObject prefab = characterPrefabs[Random.Range(0, characterPrefabs.Count)];
            GameObject obj = Instantiate(prefab, transform);

            obj.SetActive(false);

            PooledCharacter pc = new PooledCharacter();
            pc.obj = obj;
            pc.animator = obj.GetComponentInChildren<Animator>();
            pc.active = false;

            pool.Add(pc);
            available.Enqueue(pc);
        }
    }

    void PrewarmCharacters()
    {
        int spawnCount = available.Count;

        for (int i = 0; i < spawnCount; i++)
        {
            if (available.Count == 0)
                break;

            PooledCharacter pc = available.Dequeue();

            pc.speed = Random.Range(minSpeed, maxSpeed);

            Vector2 offset2D = Random.insideUnitCircle * pathOffsetRadius;
            pc.offset = new Vector3(offset2D.x, 0, offset2D.y);

            int segment = Random.Range(0, pathPoints.Count - 1);

            Vector3 start = pathPoints[segment].position;
            Vector3 end = pathPoints[segment + 1].position;

            float t = Random.value;

            Vector3 pos = Vector3.Lerp(start, end, t);

            Transform tr = pc.obj.transform;
            tr.position = pos + pc.offset;

            float scale = Random.Range(minScale, maxScale);
            tr.localScale = Vector3.one * scale;

            if (pc.animator != null)
                pc.animator.speed = pc.speed * animationSpeedMultiplier;

            pc.targetIndex = segment + 1;
            pc.active = true;

            pc.obj.SetActive(true);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnCharacter();

            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    void SpawnCharacter()
    {
        if (available.Count == 0 || pathPoints.Count < 2)
            return;

        PooledCharacter pc = available.Dequeue();

        pc.speed = Random.Range(minSpeed, maxSpeed);

        Vector2 offset2D = Random.insideUnitCircle * pathOffsetRadius;
        pc.offset = new Vector3(offset2D.x, 0, offset2D.y);

        pc.targetIndex = 1;
        pc.active = true;

        Transform t = pc.obj.transform;

        t.position = pathPoints[0].position + pc.offset;

        float scale = Random.Range(minScale, maxScale);
        t.localScale = Vector3.one * scale;

        if (pc.animator != null)
            pc.animator.speed = pc.speed * animationSpeedMultiplier;

        pc.obj.SetActive(true);
    }

    void UpdateCharacters()
    {
        foreach (var pc in pool)
        {
            if (!pc.active)
                continue;

            Transform t = pc.obj.transform;

            Vector3 target = pathPoints[pc.targetIndex].position + pc.offset;
            Vector3 dir = target - t.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);

                t.rotation = Quaternion.Slerp(
                    t.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }

            Vector3 move = dir.normalized * pc.speed * Time.deltaTime;

            if (move.magnitude >= dir.magnitude)
            {
                t.position = target;
                pc.targetIndex++;

                if (pc.targetIndex >= pathPoints.Count)
                {
                    Despawn(pc);
                    continue;
                }
            }
            else
            {
                t.position += move;
            }
        }
    }

    void Despawn(PooledCharacter pc)
    {
        pc.active = false;
        pc.obj.SetActive(false);
        available.Enqueue(pc);
    }
}