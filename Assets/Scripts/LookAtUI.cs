using UnityEngine;
using UnityEngine.UI;

public class LookAtUI : MonoBehaviour
{
    [Header("Gaze Settings")]
    public float gazeDistance = 10f;
    public LayerMask gazeLayer;
    public float dwellTime = 1f;

    private Camera vrCamera;
    private GazeTarget currentTarget;
    private float gazeTimer;

    void Start()
    {
        vrCamera = Camera.main;
    }

    void Update()
    {
        CastGazeRay();
    }

    void CastGazeRay()
    {
        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, gazeDistance, gazeLayer))
        {
            GazeTarget target = hit.collider.GetComponent<GazeTarget>();
            
            if (target != null)
            {
                if (target != currentTarget)
                {
                    ClearCurrentTarget();
                    currentTarget = target;
                    currentTarget.OnGazeEnter();
                    gazeTimer = 0f;
                }

                gazeTimer += Time.deltaTime;
                if (gazeTimer >= dwellTime)
                {
                    currentTarget.OnGazeDwell();
                }
            }
            else
            {
                ClearCurrentTarget();
            }
        }
        else
        {
            ClearCurrentTarget();
        }
    }

    void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnGazeExit();
            currentTarget = null;
            gazeTimer = 0f;
        }
    }
}
