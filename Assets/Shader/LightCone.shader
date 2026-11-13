Shader "Custom/HeadlampBeam"
{
    Properties
    {
        
        _DepthFade ("Depth Fade", Float) = 1
        _Color       ("Color", Color) = (1, 0.95, 0.8, 1)
        _Intensity   ("Intensity", Range(0, 5)) = 0.6
        _BeamLength  ("Beam Length (Object Z)", Float) = 1
        _LengthPower ("Length Fade Power", Range(0.5, 4)) = 1.5
        _EdgePower   ("Edge Softness", Range(0.5, 8)) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend One One   // 가산: 뒤 색에 더하기만 함
            ZWrite Off
            Cull Off        // 원뿔 안팎 양면 다 그림

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _Intensity;
                float  _BeamLength;
                float  _LengthPower;
                float  _EdgePower;
                float _DepthFade;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  along      : TEXCOORD2; // 0 = 머리(꼭짓점), 1 = 빔 끝
                float4 screenPos : TEXCOORD3;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS   = TransformObjectToWorldNormal(v.normalOS);
                o.along      = v.positionOS.x / _BeamLength; // UV 대신 오브젝트 Z축 길이 사용
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // (1) 길이 방향 페이드: 머리 쪽 밝고 끝으로 갈수록 0

                float lengthFade = pow(saturate(1.0 - i.along), _LengthPower);
                lengthFade *= smoothstep(1.0, 0.8, i.along);

                // (2) 가장자리 페이드(Fresnel 반전): 정면을 향한 면은 1, 옆으로 꺾인 외곽은 0
                float3 V = normalize(GetWorldSpaceViewDir(i.positionWS));
                float edgeFade = pow(abs(dot(normalize(i.normalWS), V)), _EdgePower);

                // (3) 곱하기 × 세기
                float a = lengthFade * edgeFade * _Intensity;
                float2 suv    = i.screenPos.xy / i.screenPos.w;
                float sceneZ  = LinearEyeDepth(SampleSceneDepth(suv), _ZBufferParams); // 벽까지 거리
                float beamZ   = i.screenPos.w;                                         // 빔 픽셀까지 거리
                a *= saturate((sceneZ - beamZ) / _DepthFade); // 벽에 가까울수록 0
                
                return half4(_Color.rgb * a, 1); // 가산이라 알파 대신 색 자체에 곱함
                return half4(saturate(i.along).xxx, 1);
            }


            ENDHLSL
        }
    }
}