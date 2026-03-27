using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Name of the scene to load (must match exactly with scene name in Build Settings)")]
    public string sceneToLoad;

    [Tooltip("Build index of the scene to load")]
    public int sceneIndexToLoad = -1;
    
    [Header("Optional Settings")]
    [Tooltip("Delay before loading scene (in seconds)")]
    public float loadDelay = 0f;
    
    [Tooltip("Enable this to load scene additively")]
    public bool loadAdditively = false;

    [Header("Scene Fade")]
    [Tooltip("Optional FadeController reference. If left empty, SceneManager will look for one in the scene.")]
    [SerializeField] private FadeController fadeController;
    [Tooltip("Duration used for fade-out before loading. Use -1 to use FadeController's default duration.")]
    [SerializeField] private float fadeOutDuration = -1f;

    private string pendingSceneName;
    private int pendingSceneIndex = -1;
    private bool useSceneIndex;
    private Coroutine loadRoutine;

    private void Start()
    {
        // Validate that at least one scene target has been provided.
        if (string.IsNullOrEmpty(sceneToLoad) && sceneIndexToLoad < 0)
        {
            Debug.LogWarning($"No scene target is set on {gameObject.name}. Please specify a scene name or build index.");
        }
    }

    /// <summary>
    /// Load the specified scene. Call this method from buttons or other UI elements.
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("No scene specified to load!");
            return;
        }

        BeginSceneLoad(sceneToLoad);
    }

    /// <summary>
    /// Load scene with specified name (can be called from UnityEvents)
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty!");
            return;
        }

        sceneToLoad = sceneName;
        BeginSceneLoad(sceneName);
    }

    /// <summary>
    /// Load scene using the inspector-assigned build index.
    /// </summary>
    public void LoadSceneByIndex()
    {
        if (sceneIndexToLoad < 0)
        {
            Debug.LogError("Scene index is invalid!");
            return;
        }

        BeginSceneLoad(sceneIndexToLoad);
    }

    private void LoadSceneDelayed()
    {
        try
        {
            if (useSceneIndex)
            {
                if (loadAdditively)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(pendingSceneIndex, LoadSceneMode.Additive);
                    Debug.Log($"Loading scene at index {pendingSceneIndex} additively...");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(pendingSceneIndex);
                    Debug.Log($"Loading scene at index {pendingSceneIndex}...");
                }
            }
            else
            {
                if (loadAdditively)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(pendingSceneName, LoadSceneMode.Additive);
                    Debug.Log($"Loading scene '{pendingSceneName}' additively...");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(pendingSceneName);
                    Debug.Log($"Loading scene '{pendingSceneName}'...");
                }
            }
        }
        catch (System.Exception e)
        {
            if (useSceneIndex)
            {
                Debug.LogError($"Failed to load scene at index {pendingSceneIndex}: {e.Message}");
            }
            else
            {
                Debug.LogError($"Failed to load scene '{pendingSceneName}': {e.Message}");
            }
        }
    }

    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
        Debug.Log($"Reloading current scene: {currentScene.name}");
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Load scene by build index
    /// </summary>
    /// <param name="sceneIndex">Build index of the scene</param>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < 0)
        {
            Debug.LogError("Scene index is invalid!");
            return;
        }

        sceneIndexToLoad = sceneIndex;
        BeginSceneLoad(sceneIndex);
    }

    private void BeginSceneLoad(string sceneName)
    {
        useSceneIndex = false;
        pendingSceneName = sceneName;
        BeginSceneLoadRoutine();
    }

    private void BeginSceneLoad(int sceneIndex)
    {
        useSceneIndex = true;
        pendingSceneIndex = sceneIndex;
        BeginSceneLoadRoutine();
    }

    private void BeginSceneLoadRoutine()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
        }

        loadRoutine = StartCoroutine(LoadSceneWithFadeRoutine());
    }

    private IEnumerator LoadSceneWithFadeRoutine()
    {
        if (loadDelay > 0f)
        {
            yield return new WaitForSeconds(loadDelay);
        }

        FadeController controller = GetFadeController();
        if (controller != null)
        {
            yield return controller.FadeOutRoutine(fadeOutDuration);
        }

        LoadSceneDelayed();
        loadRoutine = null;
    }

    private FadeController GetFadeController()
    {
        if (fadeController != null)
        {
            return fadeController;
        }

        fadeController = FindFirstObjectByType<FadeController>();
        return fadeController;
    }

    // Click detection for 3D objects
    private void OnMouseDown()
    {
        LoadScene();
    }
}
