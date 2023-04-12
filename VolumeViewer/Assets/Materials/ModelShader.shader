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

                fixed3 _Color;

                float3 _MinBounds;
                float3 _MaxBounds;
                float4 _Rotation;

                float3 screenCorner1;
                float3 screenCorner2;
                float3 screenCorner3;
                float3 screenCorner4;
                float3 screenNormal;

                struct Input {
                    float2 uv_MainTex;
                    float3 worldPos;
                    float facing : VFACE;
                };

                void surf(Input i, inout SurfaceOutputStandard o) {
                    float3 newPos = i.worldPos + 2.0 * cross(_Rotation.xyz, cross(_Rotation.xyz, i.worldPos) + _Rotation.w * i.worldPos);

                    if (newPos.x > _MinBounds.x && newPos.y > _MinBounds.y && newPos.z > _MinBounds.z) {
                        if (newPos.x < _MaxBounds.x && newPos.y < _MaxBounds.y && newPos.z < _MaxBounds.z) {
                            //clip(-1);
                        }
                    }

                    float3 screenCenter = (screenCorner1 + screenCorner2 + screenCorner3 + screenCorner4) / 4;
                    screenNormal = screenNormal - screenCenter;
                    //float3 negScreenNormal = -screenNormal;
                    float3 pointPos = i.worldPos - screenCenter;

                    float3 screenEdge12 = screenCorner2 - screenCorner1;
                    float3 screenEdge23 = screenCorner3 - screenCorner2;
                    //float3 screenEdge34 = screenCorner4 - screenCorner3;
                    //float3 screenEdge41 = screenCorner1 - screenCorner4;

                    float3 planeCross = cross(screenEdge12, screenEdge23);
                    float d0 = dot(planeCross, pointPos) * dot(planeCross, screenNormal);

                    float cr1 = dot(planeCross, pointPos);
                    float cr2 = dot(planeCross, screenNormal);

                    if (d0 < 0) {
                        clip(-1);
                    }

                    float facing = i.facing * 0.5 + 0.5;
                    o.Albedo = _Color.rgb * facing;
                }

                ENDCG
        }

        FallBack "Standard"
}