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

                float3 _MinBounds;
                float3 _MaxBounds;
                fixed3 _Color;

                struct Input {
                    float2 uv_MainTex;
                    float3 worldPos;
                    float facing : VFACE;
                };

                void surf(Input i, inout SurfaceOutputStandard o) {
                    if (i.worldPos.x > _MinBounds.x && i.worldPos.y > _MinBounds.y && i.worldPos.z > _MinBounds.z) {
                        if (i.worldPos.x < _MaxBounds.x && i.worldPos.y < _MaxBounds.y && i.worldPos.z < _MaxBounds.z) {
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