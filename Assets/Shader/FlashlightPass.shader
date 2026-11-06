Shader "Custom/FlashlightPass"
{
    Properties {}

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "FlashlightPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _FlashlightPos;        // 플레이어 뷰포트 좌표 (0~1)
            float2 _FlashlightDir;        // 조준 방향 2D 단위벡터
            float  _FlashlightRange;      // 사거리 (뷰포트 스케일)
            float  _FlashlightOuterAngle; // 빔 외각 반각(도)
            float  _FlashlightInnerAngle; // 빔 내각 반각(도)
            float  _FlashlightIntensity;
            float4 _FlashlightColor;
            float  _FlashlightEnabled;
            // x=바닥 뷰포트Y, y=천장 뷰포트Y, z=하이라이트 blur 두께
            float4 _FloorCeilingY;

            // 포인트 라이트 (모닥불 등) — xy=뷰포트 위치, z=범위, w=강도
            float  _PointLightCount;
            float4 _PointLightData[4];
            float4 _PointLightColor[4];

            static const float EDGE_BLUR   = 0.012; // 내부 하드 엣지 폭
            static const float ARC_FADE    = 0.08;  // 호 끝에서 dark까지 그라데 폭
            static const float ANG_FADE    = 0.45;  // 각도 경계 바깥쪽 그라데 비율 (outerTan 기준)
            static const float BOUND_FADE  = 0.05;  // 바닥/천장 경계 그라데 폭
            static const float DARK_FACTOR = 0.09;
            static const float BEAM_DIM    = 0.05; // 빔 안 원본 색상 밝기 배수 (낮출수록 어두움)

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv  = input.texcoord;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float floorY = _FloorCeilingY.x;
                float ceilY  = _FloorCeilingY.y;

                // ── 플레이 영역 경계 그라데 마스크 ──────────────────────
                float floorFade = smoothstep(floorY - BOUND_FADE, floorY + BOUND_FADE, uv.y);
                float ceilFade  = smoothstep(ceilY  + BOUND_FADE, ceilY  - BOUND_FADE, uv.y);
                float playMask  = floorFade * ceilFade;

                if (playMask < 0.001)
                    return col * DARK_FACTOR;

                float  aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uvA    = float2(uv.x * aspect, uv.y);

                // ── 포인트 라이트 (모닥불 등) ─────────────────────────────
                float3 pointContrib = float3(0, 0, 0);
                float  pointMask    = 0;

                UNITY_LOOP
                for (int li = 0; li < (int)_PointLightCount; li++)
                {
                    float2 lPosA  = float2(_PointLightData[li].x * aspect, _PointLightData[li].y);
                    float  lRange = _PointLightData[li].z;
                    float  lIntens = _PointLightData[li].w;
                    float  dist   = length(uvA - lPosA);
                    float  fall   = 1.0 - saturate(dist / max(lRange, 0.001));
                    fall = fall * fall;
                    float  contrib = fall * lIntens;
                    pointContrib += _PointLightColor[li].rgb * contrib;
                    pointMask     = max(pointMask, saturate(contrib));
                }
                pointContrib *= 0.3;
                pointMask    *= 0.3 * playMask;

                // ── 손전등 ───────────────────────────────────────────────
                float  flashMask    = 0;
                float3 flashContrib = float3(0, 0, 0);

                if (_FlashlightEnabled >= 0.5)
                {
                    float2 originA = float2(_FlashlightPos.x * aspect, _FlashlightPos.y);
                    float2 fwd  = normalize(_FlashlightDir + 1e-5);
                    float2 side = float2(-fwd.y, fwd.x);
                    float2 delta = uvA - originA;
                    float  along = dot(delta, fwd);
                    float  perp  = abs(dot(delta, side));

                    float actualDist = length(delta);
                    float fwdMask = step(0.0, along)
                                  * smoothstep(_FlashlightRange + ARC_FADE, _FlashlightRange - EDGE_BLUR, actualDist);

                    float pixelTan  = perp / max(along, 1e-5);
                    float outerTan  = tan(radians(max(_FlashlightOuterAngle, 1.0) * 0.5));
                    float innerTan  = tan(radians(clamp(_FlashlightInnerAngle, 0.5, _FlashlightOuterAngle - 0.5) * 0.5));
                    float fadeOuter = outerTan * (1.0 + ANG_FADE);
                    float sideMask  = smoothstep(fadeOuter, innerTan, pixelTan);

                    float falloff = 1.0 - saturate(actualDist / _FlashlightRange);
                    falloff *= falloff;

                    float mask = fwdMask * sideMask;

                    float blur     = max(_FloorCeilingY.z, 0.005);
                    float inCone   = step(pixelTan, fadeOuter) * fwdMask;
                    float floorHL  = smoothstep(blur, 0.0, abs(uv.y - floorY)) * inCone * falloff;
                    float ceilHL   = smoothstep(blur, 0.0, abs(uv.y - ceilY))  * inCone * falloff;
                    float surfaceBoost = max(floorHL, ceilHL);

                    float3 mainLight    = _FlashlightColor.rgb * _FlashlightIntensity * falloff * mask;
                    float3 surfaceLight = _FlashlightColor.rgb * _FlashlightIntensity * surfaceBoost * 1.5;
                    flashContrib = mainLight + surfaceLight;
                    flashMask    = saturate(mask + surfaceBoost) * playMask;
                }

                // ── 최종 합성 ────────────────────────────────────────────
                float  totalMask = saturate(flashMask + pointMask);
                float3 litColor  = col.rgb * BEAM_DIM + flashContrib + pointContrib;

                return lerp(col * DARK_FACTOR, float4(litColor, col.a), totalMask);
            }
            ENDHLSL
        }
    }
}
