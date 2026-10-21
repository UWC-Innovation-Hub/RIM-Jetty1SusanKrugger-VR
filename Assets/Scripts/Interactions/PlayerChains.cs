using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerChains : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<GameObject> objectOn = new List<GameObject>();
    [SerializeField] private GameObject objectOff;
    [SerializeField] private float delayBeforeActive = 0.5f;

    [Header("Hand Detection")]
    [SerializeField] private string handTag = "Hand";

    [Header("Auto Inactive")]
    [SerializeField] private float activeDuration = 3f;

    [Header("Exit Animation")]
    [SerializeField] private float moveDistance = 0.2f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float delayBeforeMove = 0f;
    [SerializeField] private float delayBeforeOff = 0.2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Behavior")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;
    private bool isAnimating;
    private bool isActiveWindowRunning;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (isAnimating || isActiveWindowRunning)
        {
            return;
        }

        if (!other.CompareTag(handTag))
        {
            return;
        }

        hasTriggered = true;

        StartCoroutine(ActiveAfterDelay());

        if (objectOff != null)
        {
            StartCoroutine(MoveForwardThenOff(objectOff));
        }

        foreach (GameObject obj in objectOn)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        //if (activeDuration > 0f)
        //{
        //    StartCoroutine(DeactivateAfterDelay());
        //}

        if (objectOff != null)
        {
            StartCoroutine(MoveForwardThenOff(objectOff));
        }
    }

    //private IEnumerator DeactivateAfterDelay()
    //{
    //    isActiveWindowRunning = true;

    //    yield return new WaitForSeconds(activeDuration);

    //    foreach (GameObject obj in objectOn)
    //    {
    //        if (obj != null) obj.SetActive(false);
    //    }

    //    isActiveWindowRunning = false;
    //}

    private IEnumerator ActiveAfterDelay()
    {
        isActiveWindowRunning = true;

        if (delayBeforeActive > 0f)
        {
            yield return new WaitForSeconds(delayBeforeActive);
        }

        foreach (GameObject obj in objectOn)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        if (activeDuration > 0f)
        {
            yield return new WaitForSeconds(activeDuration);

            foreach (GameObject obj in objectOn)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        isActiveWindowRunning = false;
    }

    private IEnumerator MoveForwardThenOff(GameObject target)
    {
        isAnimating = true;

        if (delayBeforeMove > 0f)
        {
            yield return new WaitForSeconds(delayBeforeMove);
        }

        Transform t = target.transform;
        Vector3 start = t.position;
        Vector3 end = start + t.forward * moveDistance;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = moveCurve.Evaluate(Mathf.Clamp01(elapsed / moveDuration));
            t.position = Vector3.LerpUnclamped(start, end, progress);
            yield return null;
        }

        t.position = end;

        if (delayBeforeOff > 0f)
        {
            yield return new WaitForSeconds(delayBeforeOff);
        }

        target.SetActive(false);
        isAnimating = false;
    }
}
