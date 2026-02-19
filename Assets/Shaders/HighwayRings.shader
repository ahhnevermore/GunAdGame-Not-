Shader "Custom/HighwayRings_FresnelGradient"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.2,0.2,0.2,1)
        _MaxZ("Max Z for Gradient", Float) = 240
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float _MaxZ;

            float4 _RingColors[8];
            float4 _RingCenters[8];
            float _RingRadii[8];
            float _RingWidths[8];
            float _RingCount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 pos = IN.worldPos;

                // --- Z-gradient ---
                float gradient = smoothstep(0, _MaxZ, pos.z);
                // --- Base color ---
                float3 col = _BaseColor.rgb * (0.7 + 0.3 * gradient);

                // --- Rings ---
                for (int i = 0; i < _RingCount; i++)
                {
                    float2 ringPos = float2(pos.x, pos.z);
                    float2 centerPos = float2(_RingCenters[i].x, _RingCenters[i].z);

                    float dist = distance(ringPos, centerPos);

                    float intensity = saturate(1 - abs(dist - _RingRadii[i]) / _RingWidths[i]);

                    // only draw if still inside max radius
                    if (_RingRadii[i] >= 0 && dist <= _RingRadii[i])
                        col += _RingColors[i].rgb * intensity * 3.0;
                }
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
