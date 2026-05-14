using UnityEngine;

public class FingerprintLockout : MonoBehaviour
{
    [SerializeField] private FingerprintTrigger[] fingerprints;

    public FingerprintTrigger[] Fingerprints => fingerprints;

    public void SetAllArmed(bool armed)
    {
        if (fingerprints == null) return;
        for (int i = 0; i < fingerprints.Length; i++)
        {
            if (fingerprints[i] != null)
                fingerprints[i].SetArmed(armed);
        }
    }
}
