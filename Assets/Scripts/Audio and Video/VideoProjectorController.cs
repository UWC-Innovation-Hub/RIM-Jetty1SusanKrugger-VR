using UnityEngine;
using UnityEngine.Video;

[ExecuteAlways]
public class VideoProjectorController : MonoBehaviour
{
    [Header("Projector Frustum")]
    [Range(1f, 170f)] public float fieldOfView = 60f;
    public float aspect = 16f / 9f;
    public float nearClip = 0.1f;
    public float farClip = 20f;

    [Header("Material Params")]
    [Range(0f, 2f)] public float intensity = 1f;
    [Range(0f, 1f)] public float minCos = 0.2f;
    public float maxDistance = 15f;

    [Header("Video Source (drag one)")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture;

    void Update()
    {
        // --- Build projection matrix ---
        var proj = Matrix4x4.Perspective(fieldOfView, aspect, nearClip, farClip);
        var view = transform.worldToLocalMatrix;
        var projectorVP = proj * view;

        // --- Push projector globals ---
        Shader.SetGlobalMatrix("_ProjectorVP", projectorVP);
        Shader.SetGlobalVector("_ProjectorPosWS", transform.position);
        Shader.SetGlobalVector("_ProjectorDirWS", transform.forward);
        Shader.SetGlobalFloat("_Intensity", intensity);
        Shader.SetGlobalFloat("_MinCos", minCos);
        Shader.SetGlobalFloat("_MaxDistance", maxDistance);

        // --- Push video texture ---
        RenderTexture rt = videoRenderTexture;
        if (rt == null && videoPlayer != null)
            rt = videoPlayer.targetTexture as RenderTexture;

        if (rt != null)
            Shader.SetGlobalTexture("_VideoTex", rt);
    }
}
