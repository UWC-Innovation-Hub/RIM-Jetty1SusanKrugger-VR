Shader "URP/ProjectorSimpleLit"
{
    Properties
    {
        // Base (lit) inputs
        _BaseMap    ("Base Albedo", 2D) = "white" {}
        _BaseColor  ("Base Color", Color) = (1,1,1,1)

        [Toggle(_NORMALMAP)] _UseNormalMap("Use Normal Map", Float) = 0
        _NormalMap  ("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0,2)) = 1

        // Simple lighting params
        _SpecColor  ("Specular Color", Color) = (0.04,0.04,0.04,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.2

        // Projector inputs
        _VideoTex   ("Video Texture", 2D) = "black" {}
        _Intensity  ("Video Intensity", Range(0,8)) = 1
        _MinCos     ("Edge Softness (angle)", Range(-1,1)) = -0.2
        _MaxDistance("Max Distance", Float) = 20

        _UseAngle   ("Use Angle Mask", Float) = 1
        _UseDistance("Use Distance Mask", Float) = 1
        _UseFacing  ("Use Facing Mask", Float) = 1
        _FacingAbs  ("Facing Uses Abs(dot)", Float) = 1
        _AngleFlip  ("Flip Angle Polarity", Float) = 0

        // How to mix the projector
        _ProjectAsEmissive ("Project as Emissive (1) or into Albedo (0)", Float) = 1
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
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            // XR / single-pass instancing safety (also harmless in Multi-Pass)
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_MULTIVIEW _STEREO_INSTANCING

            // Feature toggles
            #pragma shader_feature_local _NORMALMAP

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures / samplers
            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);
            TEXTURE2D(_VideoTex);   SAMPLER(sampler_VideoTex);

            float4 _BaseMap_ST;   // xy tiling, zw offset
            float4 _BaseColor;
            float4 _SpecColor;

            float  _Smoothness;
            float  _NormalScale;

            // Projector uniforms (from your controller)
            float4x4 _ProjectorVP;
            float3   _ProjectorPosWS;
            float3   _ProjectorDirWS;

            float  _Intensity, _MinCos, _MaxDistance;
            float  _UseAngle, _UseDistance, _UseFacing, _FacingAbs, _AngleFlip;
            float  _ProjectAsEmissive;

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 posWS      : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 tangentWS  : TEXCOORD2; // xyz = tangent, w = sign
                float2 uv         : TEXCOORD3;
                float3 viewDirWS  : TEXCOORD4;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 posWS = TransformObjectToWorld(v.positionOS);
                o.positionCS = TransformWorldToHClip(posWS);
                o.posWS = posWS;

                float3 nWS = TransformObjectToWorldNormal(v.normalOS);
                o.normalWS = nWS;

                // Build world-space tangent frame
                float3 tWS = TransformObjectToWorldDir(v.tangentOS.xyz);
                float  sgn = v.tangentOS.w * GetOddNegativeScale();
                o.tangentWS = float4(normalize(tWS), sgn);

                o.uv = v.uv;
                o.viewDirWS = GetWorldSpaceViewDir(posWS);
                return o;
            }

float3 ApplyNormalMap(float2 uv, float3 normalWS, float4 tangentWS)
{
    #if defined(_NORMALMAP)
    float3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv), _NormalScale);

    float3 t = normalize(tangentWS.xyz);
    float3 n = normalize(normalWS);

    // Handedness from tangent.w * object negative scale
    float3 b = normalize(cross(n, t)) * tangentWS.w;

    // Columns = t, b, n  (URP convention)
    float3x3 TBN = float3x3(t, b, n);

    // IMPORTANT: matrix * vector (column-major multiply)
    return normalize(mul(TBN, nTS));
    #else
    return normalize(normalWS);
    #endif
}


            // // Very small, cheap lighting: ambient (SH) + main directional + simple spec
            // float3 SimpleLighting(float3 albedo, float3 normalWS, float3 viewDirWS, float smoothness)
            // {
            //     normalWS = normalize(normalWS);
            //     viewDirWS = normalize(viewDirWS);

            //     // Ambient from SH
            //     float3 ambient = SampleSH(normalWS) * albedo;

            //     // Main light (no shadows for speed; add if you want)
            //     Light mainLight = GetMainLight(); // URP provides color + direction
            //     float NdotL = saturate(dot(normalWS, -mainLight.direction)); // direction points *from* light to surface in URP
            //     float3 diffuse = albedo * mainLight.color * NdotL;

            //     // Very cheap Blinn-Phong spec
            //     float3 halfDir = normalize(-mainLight.direction + viewDirWS);
            //     float NdotH = saturate(dot(normalWS, halfDir));
            //     float specPow = lerp(8.0, 128.0, smoothness); // rough heuristic
            //     float3 spec = _SpecColor.rgb * pow(NdotH, specPow) * NdotL;

            //     return ambient + diffuse + spec;
            // }

            // --- replace your SimpleLighting(...) with this ---
            float3 SimpleLightingWithAdditional(float3 albedo, float3 normalWS, float3 viewDirWS, float smoothness, float3 posWS)
            {
                normalWS = normalize(normalWS);
                viewDirWS = normalize(viewDirWS);

                // Ambient (SH)
                float3 color = SampleSH(normalWS) * albedo;

                // Main directional light
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));     // <-- no minus
                if (NdotL > 0)
                {
                    float3 halfDir = normalize(mainLight.direction + viewDirWS); // <-- no minus
                    float  NdotH   = saturate(dot(normalWS, halfDir));
                    float  specPow = lerp(8.0, 128.0, smoothness);

                    color += albedo * mainLight.color * NdotL;
                    color += _SpecColor.rgb * pow(NdotH, specPow) * NdotL;
                }

                // Additional point/spot lights
                int addCount = GetAdditionalLightsCount();
                for (int i = 0; i < addCount; i++)
                {
                    Light l = GetAdditionalLight(i, posWS);

                    float NdotL2 = saturate(dot(normalWS, l.direction));        // <-- no minus
                    if (NdotL2 <= 0) continue;

                    float3 halfDir2 = normalize(l.direction + viewDirWS);        // <-- no minus
                    float  NdotH2   = saturate(dot(normalWS, halfDir2));
                    float  specPow2 = lerp(8.0, 128.0, smoothness);

                    float atten = l.distanceAttenuation * l.shadowAttenuation;   // includes spot falloff if any

                    color += albedo * l.color * (NdotL2 * atten);
                    color += _SpecColor.rgb * pow(NdotH2, specPow2) * (NdotL2 * atten);
                }

                return color;
            }



            float2 TransformBaseUV(float2 uv)
            {
                return uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
            }

            // --- Projector sampling & masks (same logic you've debugged) ---
            float3 SampleProjectedVideo(float3 posWS, float3 normalWS, out float maskOut)
            {
                // projective UV
                float4 clip = mul(_ProjectorVP, float4(posWS, 1.0));
                if (clip.w <= 0) { maskOut = 0; return 0; }
                float2 pUV = (clip.xy / clip.w) * 0.5 + 0.5;
                if (pUV.x < 0 || pUV.x > 1 || pUV.y < 0 || pUV.y > 1) { maskOut = 0; return 0; }

                // masks
                float3 fwd = normalize(_ProjectorDirWS);
                if (_AngleFlip > 0.5) fwd = -fwd;

                float3 toPoint = normalize(posWS - _ProjectorPosWS); // projector -> point
                float3 toProj  = -toPoint;

                float dist     = distance(posWS, _ProjectorPosWS);
                float distMask = (_MaxDistance > 1e-4) ? saturate(1.0 - dist / _MaxDistance) : 1.0;

                float cosAng   = dot(fwd, toPoint);
                float angMask  = saturate((cosAng - _MinCos) / (1.0 - _MinCos + 1e-5));

                float facingDot = dot(normalize(normalWS), toProj);
                if (_FacingAbs > 0.5) facingDot = abs(facingDot);
                float facing = saturate(facingDot);

                float mask = 1.0;
                if (_UseDistance > 0.5) mask *= distMask;
                if (_UseAngle    > 0.5) mask *= angMask;
                if (_UseFacing   > 0.5) mask *= facing;

                maskOut = mask;
                float3 vid = SAMPLE_TEXTURE2D(_VideoTex, sampler_VideoTex, pUV).rgb;
                return vid;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 baseUV = TransformBaseUV(i.uv);
                float3 baseAlbedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV).rgb * _BaseColor.rgb;

                // Normal (optional)
                float3 normalWS = ApplyNormalMap(baseUV, i.normalWS, i.tangentWS);

                // Projector sample + mask
                float mask;
                float3 projVid = SampleProjectedVideo(i.posWS, normalWS, mask);

                // Two mixing modes:
                // 1) Emissive add (projector independent of lights)
                // 2) Into albedo (projector modulated by lights)
                float3 albedoForLighting = baseAlbedo;
                float3 emissive = 0;

                if (_ProjectAsEmissive > 0.5)
                {
                    emissive = projVid * (_Intensity * mask);
                }
                else
                {
                    albedoForLighting = saturate(baseAlbedo + projVid * (_Intensity * mask));
                }

                // Simple lighting
                //float3 litColor = SimpleLighting(albedoForLighting, normalWS, i.viewDirWS, _Smoothness);
                float3 litColor = SimpleLightingWithAdditional(albedoForLighting, normalWS, i.viewDirWS, _Smoothness, i.posWS);

                // Add emissive if used
                float3 finalCol = litColor + emissive;

                return float4(finalCol, 1);
            }
            ENDHLSL
        }
    }
}
