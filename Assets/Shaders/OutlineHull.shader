Shader "Custom/URP/OutlineHull"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 0.85, 0.3, 1)
        _OutlineWidth("Outline Width (pixels)", Range(0.5, 16)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry+2"
        }

        Pass
        {
            Name "OutlineHull"
            Tags { "LightMode" = "UniversalForward" }

            // Front faces are culled so only the part of the inflated hull that sticks
            // out past the real silhouette survives, and ZWrite stays off so the hull
            // never fights the object it wraps.
            Cull Front
            ZWrite Off

            // ZTest Always, not LEqual. The rim carries the depth of the silhouette it
            // was extruded from, so any surface nearer at that pixel - the ground a
            // plant stands on, above all - swallows the lower half of the outline.
            // Drawing on top is safe here because PlayerInteraction only highlights
            // what its ray actually reached, which makes the object known to be visible.
            ZTest Always

            // Everything OutlineMask marked is the object itself - dropping it leaves
            // the rim alone, with no fill over the surface and no seams between the
            // separate renderers of one prefab.
            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionCS = TransformWorldToHClip(positionWS);

                // Expanded in clip space rather than along the world normal, so the
                // outline keeps the same pixel width whatever the distance.
                float3 normalCS = mul((float3x3)UNITY_MATRIX_VP, normalWS);
                float2 normalPixels = normalCS.xy * _ScreenParams.xy;
                float2 direction = normalPixels / max(length(normalPixels), 1e-5);

                OUT.positionCS.xy += direction * _OutlineWidth * 2.0 / _ScreenParams.xy * OUT.positionCS.w;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
