Shader "Custom/URP/OutlineMask"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry+1"
        }

        Pass
        {
            Name "OutlineMask"
            Tags { "LightMode" = "UniversalForward" }

            // Writes nothing but the stencil: this pass only marks which pixels the
            // object itself covers, so OutlineHull can throw away everything that is
            // not past the silhouette. The queue matters - every mask has to be drawn
            // before any hull, otherwise the parts of one prefab outline each other.
            Cull Back
            ZWrite Off
            ColorMask 0

            // Always, to match OutlineHull: the mask has to cover the object's whole
            // footprint, or the hull would fill in wherever the mask was depth-rejected.
            ZTest Always

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
