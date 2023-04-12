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

                float3 _ScreenCorner1;
                float3 _ScreenCorner2;
                float3 _ScreenCorner3;
                float3 _ScreenCorner4;
                float3 _ScreenNormal;

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

                    float3 screenCenter = (_ScreenCorner1 + _ScreenCorner2 + _ScreenCorner3 + _ScreenCorner4) / 4;
                    _ScreenNormal = _ScreenNormal - screenCenter;
                    float3 negScreenNormal = -_ScreenNormal;
                    float3 pointPos = i.worldPos - screenCenter;

                    float3 screenEdge12 = _ScreenCorner2 - _ScreenCorner1;
                    float3 screenEdge23 = _ScreenCorner3 - _ScreenCorner2;
                    float3 screenEdge34 = _ScreenCorner4 - _ScreenCorner3;
                    float3 screenEdge41 = _ScreenCorner1 - _ScreenCorner4;

                    float3 planeCross = cross(screenEdge12, screenEdge23);
                    float d0 = dot(planeCross, pointPos) * dot(planeCross, _ScreenNormal);
                    
                    float3 planeTopEnd = cross(screenEdge12, negScreenNormal);
                    float d1 = dot(planeTopEnd, pointPos) * dot(planeTopEnd, screenEdge41);

                    float3 planeRightEnd = cross(screenEdge23, negScreenNormal);
                    float d2 = dot(planeRightEnd, pointPos) * dot(planeTopEnd, screenEdge12);

                    float3 planeBottomEnd = cross(screenEdge34, negScreenNormal);
                    float d3 = dot(planeBottomEnd, pointPos) * dot(planeTopEnd, screenEdge23);

                    float3 planeLeftEnd = cross(screenEdge41, negScreenNormal);
                    float d4 = dot(planeLeftEnd, pointPos) * dot(planeTopEnd, screenEdge34);

                    if (d0 < 0 && d1 < 0 && d2 < 0 && d3 < 0 && d4 < 0) {
                        clip(-1);
                    }

                    float facing = i.facing * 0.5 + 0.5;
                    o.Albedo = _Color.rgb * facing;
                }

                ENDCG
        }

        FallBack "Standard"
}