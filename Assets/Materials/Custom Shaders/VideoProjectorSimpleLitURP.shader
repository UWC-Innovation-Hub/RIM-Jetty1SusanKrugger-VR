
Shader "URP/ProjectorSimpleLit"
{
    Properties
    {
        _BaseMap ("Base Albedo", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        [Toggle(_NORMALMAP)] _UseNormalMap ("Use Normal Map", Float) = 0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0,2)) = 1
        _RealtimeNormalInfluence ("Realtime Normal Map Influence", Range(0,1)) = 0.25

        _SpecColor ("Specular Color", Color) = (0.04,0.04,0.04,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.2

        _VideoTex ("Video Texture", 2D) = "black" {}
        _Intensity ("Video Intensity", Range(0,8)) = 1
        _MinCos ("Edge Softness (angle)", Range(-1,1)) = -0.2
        _MaxDistance ("Max Distance", Float) = 20

        _UseAngle ("Use Angle Mask", Float) = 1
        _UseDistance ("Use Distance Mask", Float) = 1
        _UseFacing ("Use Facing Mask", Float) = 1
        _FacingAbs ("Facing Uses Abs(dot)", Float) = 1
        _AngleFlip ("Flip Angle Polarity", Float) = 0

        _ProjectAsEmissive ("Project as Emissive (1) or into Albedo (0)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON

            // Realtime per-pixel additional lights (point and spot)
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_MULTIVIEW _STEREO_INSTANCING

            #pragma shader_feature_local _NORMALMAP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            TEXTURE2D(_VideoTex);
            SAMPLER(sampler_VideoTex);

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _SpecColor;

            float _Smoothness;
            float _NormalScale;
            float _RealtimeNormalInfluence;

            float4x4 _ProjectorVP;
            float3 _ProjectorPosWS;
            float3 _ProjectorDirWS;

            float _Intensity;
            float _MinCos;
            float _MaxDistance;

            float _UseAngle;
            float _UseDistance;
            float _UseFacing;
            float _FacingAbs;
            float _AngleFlip;

            float _ProjectAsEmissive;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;

                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);

                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 posWS = TransformObjectToWorld(v.positionOS);

                o.positionCS = TransformWorldToHClip(posWS);
                o.posWS = posWS;

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);

                float3 tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
                float tangentSign = v.tangentOS.w * GetOddNegativeScale();

                o.tangentWS = float4(normalize(tangentWS), tangentSign);
                o.uv = v.uv;
                o.viewDirWS = GetWorldSpaceViewDir(posWS);

                OUTPUT_LIGHTMAP_UV(
                    v.lightmapUV,
                    unity_LightmapST,
                    o.lightmapUV
                );

                OUTPUT_SH(o.normalWS, o.vertexSH);

                return o;
            }

            float3 ApplyNormalMap(
                float2 uv,
                float3 normalWS,
                float4 tangentWS)
            {
                #if defined(_NORMALMAP)

                    float3 normalTS = UnpackNormalScale(
                        SAMPLE_TEXTURE2D(
                            _NormalMap,
                            sampler_NormalMap,
                            uv
                        ),
                        _NormalScale
                    );

                    float3 tangent = normalize(tangentWS.xyz);
                    float3 normal = normalize(normalWS);

                    float3 bitangent =
                        normalize(cross(normal, tangent)) *
                        tangentWS.w;

                    float3x3 TBN = float3x3(
                        tangent,
                        bitangent,
                        normal
                    );

                    return normalize(mul(TBN, normalTS));

                #else

                    return normalize(normalWS);

                #endif
            }

            float2 TransformBaseUV(float2 uv)
            {
                return uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
            }

            // Simple realtime diffuse lighting using URP's light data.
            // Deliberately avoids realtime shadow calculations while diagnosing artifacts.
            float3 CalculateRealtimeLighting(
                float3 albedo,
                float3 normalWS,
                float3 posWS)
            {
                normalWS = normalize(normalWS);
                float3 color = 0;

                // Main directional light.
                Light mainLight = GetMainLight();
                float mainNdotL = saturate(dot(normalWS, mainLight.direction));
                color += albedo * mainLight.color * mainNdotL;

                // Additional point and spot lights, using URP attenuation.
                #if defined(_ADDITIONAL_LIGHTS)
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, posWS);
                        float NdotL = saturate(dot(normalWS, light.direction));
                        color += albedo * light.color * NdotL *
                                 light.distanceAttenuation * light.shadowAttenuation;
                    }
                #endif

                return color;
            }

            float3 SampleProjectedVideo(
                float3 posWS,
                float3 normalWS,
                out float maskOut)
            {
                float4 clip = mul(
                    _ProjectorVP,
                    float4(posWS, 1.0)
                );

                if (clip.w <= 0)
                {
                    maskOut = 0;
                    return 0;
                }

                float2 pUV = (clip.xy / clip.w) * 0.5 + 0.5;

                if (
                    pUV.x < 0 || pUV.x > 1 ||
                    pUV.y < 0 || pUV.y > 1
                )
                {
                    maskOut = 0;
                    return 0;
                }

                float3 forward = normalize(_ProjectorDirWS);

                if (_AngleFlip > 0.5)
                    forward = -forward;

                float3 toPoint = normalize(
                    posWS - _ProjectorPosWS
                );

                float3 toProjector = -toPoint;

                float distanceToProjector = distance(
                    posWS,
                    _ProjectorPosWS
                );

                float distanceMask =
                    (_MaxDistance > 1e-4)
                    ? saturate(
                        1.0 - distanceToProjector / _MaxDistance
                    )
                    : 1.0;

                float cosineAngle = dot(forward, toPoint);

                float angleMask = saturate(
                    (cosineAngle - _MinCos) /
                    (1.0 - _MinCos + 1e-5)
                );

                float facingDot = dot(
                    normalize(normalWS),
                    toProjector
                );

                if (_FacingAbs > 0.5)
                    facingDot = abs(facingDot);

                float facingMask = saturate(facingDot);

                float mask = 1.0;

                if (_UseDistance > 0.5)
                    mask *= distanceMask;

                if (_UseAngle > 0.5)
                    mask *= angleMask;

                if (_UseFacing > 0.5)
                    mask *= facingMask;

                maskOut = mask;

                return SAMPLE_TEXTURE2D(
                    _VideoTex,
                    sampler_VideoTex,
                    pUV
                ).rgb;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 baseUV = TransformBaseUV(i.uv);

                float3 baseAlbedo =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        baseUV
                    ).rgb * _BaseColor.rgb;

                float3 normalWS = ApplyNormalMap(
                    baseUV,
                    i.normalWS,
                    i.tangentWS
                );

                float mask;

                float3 projectedVideo = SampleProjectedVideo(
                    i.posWS,
                    normalWS,
                    mask
                );

                float3 albedoForLighting = baseAlbedo;
                float3 emissive = 0;

                if (_ProjectAsEmissive > 0.5)
                {
                    emissive =
                        projectedVideo * (_Intensity * mask);
                }
                else
                {
                    albedoForLighting = saturate(
                        baseAlbedo +
                        projectedVideo * (_Intensity * mask)
                    );
                }

                // Sample the baked lightmap or ambient SH fallback
                float3 bakedGI = SAMPLE_GI(
                    i.lightmapUV,
                    i.vertexSH,
                    normalWS
                );

                // Preserve the working baked-GI calculation.
                float3 bakedColor = albedoForLighting * bakedGI;

                // Blend a small amount of normal-map detail into the geometric
                // normal for realtime lighting. A low default influence helps
                // avoid the artifacts seen with the full normal-mapped normal.
                float3 geometricNormalWS = normalize(i.normalWS);
                float3 realtimeNormalWS = normalize(lerp(
                    geometricNormalWS,
                    normalWS,
                    saturate(_RealtimeNormalInfluence)
                ));

                float3 realtimeColor = CalculateRealtimeLighting(
                    albedoForLighting,
                    realtimeNormalWS,
                    i.posWS
                );

                // Emissive video remains unaffected by scene lighting.
                float3 finalColor = bakedColor + realtimeColor + emissive;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}
