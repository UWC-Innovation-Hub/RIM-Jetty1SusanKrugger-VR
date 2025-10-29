Shader "Unlit/VideoProjectorAdditiveURP_Debug"
{
    Properties
    {
        _BaseMap ("Base Albedo", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _VideoTex ("Video Texture", 2D) = "black" {}

        _Intensity ("Video Intensity", Range(0,8)) = 1
        _MinCos ("Edge Softness (angle)", Range(-1,1)) = -0.2
        _MaxDistance ("Max Distance", Float) = 20.0

        _UseAngle ("Use Angle Mask", Float) = 1
        _UseDistance ("Use Distance Mask", Float) = 1
        _UseFacing ("Use Facing Mask", Float) = 1
        _FacingAbs ("Facing Uses Abs(dot)", Float) = 1
        _AngleFlip ("Flip Angle Polarity", Float) = 0

        _BypassMasks ("Bypass All Masks", Float) = 0   // 1 = ignore masks
        _DebugMode ("Debug Mode (0=Normal,1=VideoOnly,2=UV,3=Masks)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_VideoTex); SAMPLER(sampler_VideoTex);

            float4 _BaseMap_ST; // xy = tiling, zw = offset


            float4x4 _ProjectorVP;
            float3   _ProjectorPosWS;
            float3   _ProjectorDirWS;

            half _Intensity, _MinCos, _MaxDistance;
            half _UseAngle, _UseDistance, _UseFacing, _FacingAbs, _AngleFlip;
            half _BypassMasks, _DebugMode;
            float4 _BaseColor;

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 posWS      : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS);
                o.positionCS = TransformWorldToHClip(posWS);
                o.posWS = posWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // --- Base wall ---
                //half3 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;

                float2 baseUV = i.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                half3 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV).rgb * _BaseColor.rgb;

                // --- Projective UV ---
                float4 clip = mul(_ProjectorVP, float4(i.posWS, 1.0));
                if (clip.w <= 0) {
                    // Debug: visualize "behind projector" as red
                    if (_DebugMode > 1.5h) return half4(baseCol * 0 + half3(1,0,0), 1);
                    return half4(baseCol, 1);
                }

                float2 pUV = (clip.xy / clip.w) * 0.5 + 0.5;
                if (pUV.x < 0 || pUV.x > 1 || pUV.y < 0 || pUV.y > 1) {
                    // Debug: visualize OOB as blue tint
                    if (_DebugMode > 1.5h) return half4(baseCol * 0 + half3(0,0,1), 1);
                    return half4(baseCol, 1);
                }

                // --- Video sample ---
                half3 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, pUV).rgb;

                // Debug: show raw video only (no base, no masks)
                if (_DebugMode > 0.5h && _DebugMode < 1.5h) {
                    return half4(vid, 1);
                }

                // Debug: show UV as color (green=V, red=U)
                if (_DebugMode > 1.5h && _DebugMode < 2.5h) {
                    return half4(pUV.x, pUV.y, 0, 1);
                }

                // --- Masks ---
                float3 fwd = normalize(_ProjectorDirWS);
                if (_AngleFlip > 0.5h) fwd = -fwd;

                float3 toPoint = normalize(i.posWS - _ProjectorPosWS); // projector -> point
                float3 toProj  = -toPoint;
                float3 nrm     = normalize(i.normalWS);

                half dist     = distance(i.posWS, _ProjectorPosWS);
                half distMask = (_MaxDistance > 1e-4h) ? saturate(1.0h - dist / _MaxDistance) : 1.0h;

                half cosAng   = dot(fwd, toPoint); // [-1..1]
                half angMask  = saturate((cosAng - _MinCos) / (1.0h - _MinCos + 1e-5h));

                half facingDot = dot(nrm, toProj);
                if (_FacingAbs > 0.5h) facingDot = abs(facingDot);
                half facing = saturate(facingDot);

                // Debug: visualize masks as RGB = (angle, distance, facing)
                if (_DebugMode > 2.5h) {
                    return half4(angMask, distMask, facing, 1);
                }

                half mask = 1.0h;
                if (_BypassMasks < 0.5h) {
                    if (_UseDistance > 0.5h) mask *= distMask;
                    if (_UseAngle    > 0.5h) mask *= angMask;
                    if (_UseFacing   > 0.5h) mask *= facing;
                }

                // --- Additive over base ---
                half3 outCol = baseCol + vid * (_Intensity * mask);
                return half4(outCol, 1);
            }
            ENDHLSL
        }
    }
}




////Shader when using default Base_Tex with no falloff angle/distance properties enabled, etc.
// Shader "Unlit/VideoProjectorAdditiveURP"
// {
//     Properties
//     {
//         _BaseMap ("Base Albedo", 2D) = "white" {}
//         _BaseColor ("Base Color", Color) = (1,1,1,1)
//         _VideoTex ("Video Texture", 2D) = "black" {}

//         _Intensity ("Video Intensity", Range(0,4)) = 1
//         _MinCos ("Edge Softness (angle)", Range(-1,1)) = -0.2
//         _MaxDistance ("Max Distance", Float) = 20.0

//         _UseAngle ("Use Angle Mask", Float) = 1
//         _UseDistance ("Use Distance Mask", Float) = 1
//         _UseFacing ("Use Facing Mask", Float) = 1
//         _FacingAbs ("Facing Uses Abs(dot)", Float) = 1
//         _AngleFlip ("Flip Angle Polarity", Float) = 0
//     }

//     SubShader
//     {
//         Tags { "RenderType"="Opaque" "Queue"="Geometry" }
//         Pass
//         {
//             Tags { "LightMode"="UniversalForward" }
//             Cull Back
//             ZWrite On
//             ZTest LEqual

//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//             TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
//             TEXTURE2D(_VideoTex); SAMPLER(sampler_VideoTex);

//             float4x4 _ProjectorVP;
//             float3   _ProjectorPosWS;
//             float3   _ProjectorDirWS;

//             half _Intensity, _MinCos, _MaxDistance;
//             half _UseAngle, _UseDistance, _UseFacing, _FacingAbs, _AngleFlip;
//             float4 _BaseColor;

//             struct Attributes {
//                 float3 positionOS : POSITION;
//                 float3 normalOS   : NORMAL;
//                 float2 uv         : TEXCOORD0;
//             };
//             struct Varyings {
//                 float4 positionCS : SV_POSITION;
//                 float3 posWS      : TEXCOORD0;
//                 float3 normalWS   : TEXCOORD1;
//                 float2 uv         : TEXCOORD2;
//             };

//             Varyings vert (Attributes v)
//             {
//                 Varyings o;
//                 float3 posWS = TransformObjectToWorld(v.positionOS);
//                 o.positionCS = TransformWorldToHClip(posWS);
//                 o.posWS = posWS;
//                 o.normalWS = TransformObjectToWorldNormal(v.normalOS);
//                 o.uv = v.uv;
//                 return o;
//             }

//             half4 frag (Varyings i) : SV_Target
//             {
//                 // Base wall
//                 half3 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;

//                 // Projective UV (still needed to fetch the video at the right place)
//                 float4 clip = mul(_ProjectorVP, float4(i.posWS, 1.0));
//                 if (clip.w <= 0) return half4(baseCol,1);
//                 float2 uv = (clip.xy / clip.w) * 0.5 + 0.5;
//                 if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return half4(baseCol,1);

//                 // Sample video (NO MASKS)
//                 half3 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, uv).rgb;

//                 // Add over base
//                 return half4(baseCol + vid * _Intensity, 1);
//             }

//             ENDHLSL
//         }
//     }
// }


// //Original VideoProjectorURP Shader Code
// Shader "Unlit/VideoProjectorURP"
// {
//     Properties
//     {
//         _VideoTex ("Video Texture", 2D) = "black" {}
//         _Intensity ("Intensity", Range(0, 2)) = 1
//         _MinCos ("Edge Softness (angle)", Range(-1,1)) = -0.2   // <-- was 0..1
//         _MaxDistance ("Max Distance", Float) = 15.0
//         _UseAngle ("Use Angle Mask", Float) = 1
//         _UseDistance ("Use Distance Mask", Float) = 1
//         _UseFacing ("Use Facing Mask", Float) = 1
//         _FacingAbs ("Facing Uses Abs(dot)", Float) = 0          // <-- new toggle
//     }

//     SubShader
//     {
//         Tags{ "RenderType"="Opaque" "Queue"="Geometry" }
//         Pass
//         {
//             Tags{"LightMode"="UniversalForward"}
//             Cull Back
//             ZWrite On
//             ZTest LEqual

//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//             TEXTURE2D(_VideoTex); SAMPLER(sampler_VideoTex);

//             float4x4 _ProjectorVP;       // proj * view of projector
//             float3   _ProjectorPosWS;    // projector world position
//             float3   _ProjectorDirWS;    // projector forward (world)
//             half     _Intensity;
//             half     _MinCos;
//             half     _MaxDistance;

//             struct Attributes {
//                 float3 positionOS : POSITION;   // <-- float3, not float4
//                 float3 normalOS   : NORMAL;
//             };
//             struct Varyings {
//                 float4 positionCS : SV_POSITION;
//                 float3 posWS      : TEXCOORD0;
//                 float3 normalWS   : TEXCOORD1;
//             };

//             Varyings vert (Attributes v)
//             {
//                 Varyings o;
//                 float3 posWS = TransformObjectToWorld(v.positionOS);   // <-- returns float3
//                 o.positionCS = TransformWorldToHClip(posWS);
//                 o.posWS = posWS;
//                 o.normalWS = TransformObjectToWorldNormal(v.normalOS);
//                 return o;
//             }



//             half _UseAngle, _UseDistance, _UseFacing, _FacingAbs;

//             half4 frag (Varyings i) : SV_Target
//             {
//                 // Projective UV
//                 float4 clip = mul(_ProjectorVP, float4(i.posWS, 1.0));
//                 if (clip.w <= 0) discard;

//                 float2 uv = (clip.xy / clip.w) * 0.5 + 0.5;
//                 if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) discard;

//                 // Sample video
//                 half4 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, uv);

//                 // --- Masks (robust) ---
//                 float3 fwd     = normalize(_ProjectorDirWS);
//                 float3 toPoint = normalize(i.posWS - _ProjectorPosWS); // projector -> point
//                 float3 toProj  = -toPoint;                             // point -> projector
//                 float3 nrm     = normalize(i.normalWS);

//                 // Distance
//                 half dist     = distance(i.posWS, _ProjectorPosWS);
//                 half distMask = (_MaxDistance > 1e-4h) ? saturate(1.0h - dist / _MaxDistance) : 1.0h;

//                 // Angle (gentle linear remap from _MinCos..1)
//                 half cosAng  = dot(-fwd, toPoint); // now points match your frustum
//                 half angMask = saturate((cosAng - _MinCos) / (1.0h - _MinCos + 1e-5h));

//                 // Facing: does the surface face toward projector *position*?
//                 half facingDot = dot(nrm, toProj);
//                 if (_FacingAbs > 0.5h) facingDot = abs(facingDot);   // tolerate flipped normals
//                 half facing = saturate(facingDot);

//                 // Apply toggles
//                 half mask = 1.0h;
//                 if (_UseDistance > 0.5h) mask *= distMask;
//                 if (_UseAngle    > 0.5h) mask *= angMask;
//                 if (_UseFacing   > 0.5h) mask *= facing;

//                 return vid * (_Intensity * mask);
//             }




//             //Original Fragment
//             // half4 frag (Varyings i) : SV_Target
//             // {
//             //     // Project world pos into projector clip space
//             //     float4 clip = mul(_ProjectorVP, float4(i.posWS, 1.0));
//             //     float w = clip.w;
//             //     if (w <= 0) discard;

//             //     float2 uv = (clip.xy / w) * 0.5 + 0.5;
//             //     if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) discard;

//             //     // Angle falloff
//             //     float3 toPoint = normalize(i.posWS - _ProjectorPosWS);
//             //     half cosAng = dot(normalize(_ProjectorDirWS), toPoint);
//             //     half angMask = smoothstep(_MinCos, 1.0, cosAng);

//             //     // Distance falloff
//             //     half dist = distance(i.posWS, _ProjectorPosWS);
//             //     half distMask = saturate(1.0 - saturate(dist / _MaxDistance));

//             //     // Optional normal-facing fade
//             //     half facing = saturate(dot(normalize(i.normalWS), -normalize(_ProjectorDirWS)));

//             //     half mask = angMask * distMask * facing;

//             //     half4 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, uv);

//             //     return vid * (_Intensity * mask);
//             // }


//             // //Test (debug) fragment for projection
//             // half4 frag (Varyings i) : SV_Target
//             // {
//             //     float4 clip = mul(_ProjectorVP, float4(i.posWS, 1.0));
//             //     if (clip.w <= 0) return half4(0,0,0,1);

//             //     float2 uv = (clip.xy / clip.w) * 0.5 + 0.5;

//             //     // visualize bounds
//             //     if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
//             //     return half4(0,0,0,1); // outside = black

//             //     // no angle/distance/normal masks for now
//             //     half4 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, uv);
//             //     return vid; // just show the video
//             // }


//             ENDHLSL
//         }
//     }
// }
