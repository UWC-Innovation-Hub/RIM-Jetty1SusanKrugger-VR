using UnityEngine;

public class AnimationAudioSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private int lastLoop = -1;

    void Update()
    {
        if (animator == null || audioSource == null || audioClip == null)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // Get the current loop number
        int currentLoop = Mathf.FloorToInt(state.normalizedTime);

        // Detect animation starting or looping
        if (currentLoop != lastLoop)
        {
            lastLoop = currentLoop;

            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}