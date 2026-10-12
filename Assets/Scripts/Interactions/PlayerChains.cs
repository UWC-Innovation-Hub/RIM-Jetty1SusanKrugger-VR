using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerChains : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<GameObject> objectOn = new List<GameObject>();
    [SerializeField] private GameObject objectOff;

    [Header("Hand Detection")]
    [SerializeField] private string handTag = "Hand";

    [Header("Behavior")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

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

        if (!other.CompareTag(handTag))
        {
            return;
        }

        hasTriggered = true;

        foreach (GameObject obj in objectOn)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        if (objectOff != null)
        {
            objectOff.SetActive(false);
        }
    }
}
