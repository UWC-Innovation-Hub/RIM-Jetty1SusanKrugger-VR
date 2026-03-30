Shader "RIM/BreathingVignetteQuad"
{
    Properties
    {
        _Color("Vignette Color", Color) = (0, 0, 0, 1)
        _InnerRadius("Inner Radius", Range(0, 1)) = 0.45
        _Softness("Softness", Range(0.001, 1)) = 0.2
        _Opacity("Opacity", Range(0, 1)) = 1
        _AspectRatio("Aspect Ratio", Float) = 1
        _Center("Center", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _InnerRadius;
                float _Softness;
                float _Opacity;
                float _AspectRatio;
                float4 _Center;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 centeredUV = (input.uv - _Center.xy) * 2.0;
                centeredUV.x *= max(0.0001, _AspectRatio);

                float dist = length(centeredUV);
                float feather = max(0.001, _Softness);
                float alpha = smoothstep(_InnerRadius, _InnerRadius + feather, dist);

                half4 color = _Color;
                color.a *= alpha * saturate(_Opacity);
                return color;
            }
            ENDHLSL
        }
    }
}
