Shader "Custom/ModelShader"
{
    Properties
    {
        _Color("Color", Color) = (0, 0, 0, 1)
    }

        SubShader
        {
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Fade"}
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            LOD 100

                CGPROGRAM
                #pragma surface surf Standard fullforwardshadows
                #pragma target 3.0

                #include "UnityCG.cginc"

                float3 _MinBounds;
                float3 _MaxBounds;
                float4 _Rotation;
                fixed3 _Color;

                struct Input {
                    float2 uv_MainTex;
                    float3 worldPos;
                    float facing : VFACE;
                };

                void surf(Input i, inout SurfaceOutputStandard o) {
                    float3 newPos = i.worldPos + 2.0 * cross(_Rotation.xyz, cross(_Rotation.xyz, i.worldPos) + _Rotation.w * i.worldPos);

                    if (newPos.x > _MinBounds.x && newPos.y > _MinBounds.y && newPos.z > _MinBounds.z) {
                        if (newPos.x < _MaxBounds.x && newPos.y < _MaxBounds.y && newPos.z < _MaxBounds.z) {
                            clip(-1);
                        }
                    }

                    float facing = i.facing * 0.5 + 0.5;
                    o.Albedo = _Color.rgb * facing;
                }

                ENDCG
        }

        FallBack "Standard"
}