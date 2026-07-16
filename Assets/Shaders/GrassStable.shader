Shader "Custom/URP/GrassStable"
{
    Properties
    {
        [MainTexture] _MainTex("Grass Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color Tint", Color) = (1, 1, 1, 1)

        _BottomColor("Bottom Color", Color) = (0.08, 0.3, 0.04, 1)
        _TopColor("Top Color", Color) = (0.45, 0.9, 0.15, 1)

        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.35
        _BendPower("Height Bend Power", Range(0.5, 6)) = 2

        [Header(Wind)]
        _WindDirection("Wind Direction XZ", Vector) = (1, 0, 0.3, 0)
        _WindAngle("Wind Bend Angle", Range(0, 30)) = 7
        _WindSpeed("Wind Speed", Range(0, 10)) = 1.5
        _WindScale("Wind Spatial Scale", Range(0, 5)) = 0.5

        [Header(Top View Correction)]
        _TopViewAngle("Top View Bend Angle", Range(0, 80)) = 45
        _TopViewStart("Top View Start", Range(0, 1)) = 0.3
        _TopViewEnd("Top View End", Range(0, 1)) = 0.85
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        Cull Off
        ZWrite On
        ZTest LEqual
        Blend One Zero

        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;

                half4 _BaseColor;
                half4 _BottomColor;
                half4 _TopColor;

                float _Cutoff;
                float _BendPower;

                float4 _WindDirection;
                float _WindAngle;
                float _WindSpeed;
                float _WindScale;

                float _TopViewAngle;
                float _TopViewStart;
                float _TopViewEnd;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half heightMask   : TEXCOORD1;
                half fogFactor    : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 SafeNormalize2(float2 value, float2 fallback)
            {
                float lengthSquared = dot(value, value);

                return lengthSquared > 0.000001
                    ? value * rsqrt(lengthSquared)
                    : fallback;
            }

            float3 RotateAroundGrassBase(
                float3 positionWS,
                float3 pivotWS,
                float2 bendDirection,
                float angle
            )
            {
                float3 relativePosition = positionWS - pivotWS;

                float forwardPosition =
                    dot(relativePosition.xz, bendDirection);

                float verticalPosition =
                    relativePosition.y;

                float sine;
                float cosine;

                sincos(angle, sine, cosine);

                float rotatedForward =
                    forwardPosition * cosine
                    + verticalPosition * sine;

                float rotatedVertical =
                    verticalPosition * cosine
                    - forwardPosition * sine;

                relativePosition.xz +=
                    bendDirection
                    * (rotatedForward - forwardPosition);

                relativePosition.y = rotatedVertical;

                return pivotWS + relativePosition;
            }

            float GetHeightMask(float uvY)
            {
                float height = saturate(uvY);

                return pow(
                    height,
                    max(_BendPower, 0.01)
                );
            }

            float3 DeformGrass(float3 positionOS, float2 uv)
            {
                float3 positionWS =
                    TransformObjectToWorld(positionOS);

                float3 pivotWS =
                    TransformObjectToWorld(float3(0, 0, 0));

                float heightMask =
                    GetHeightMask(uv.y);


                float2 windDirection =
                    SafeNormalize2(
                        _WindDirection.xz,
                        float2(1, 0)
                    );

                float windPhase =
                    dot(pivotWS.xz, windDirection)
                    * _WindScale
                    + _Time.y * _WindSpeed;

                float windAngle =
                    sin(windPhase)
                    * radians(_WindAngle)
                    * heightMask;

                positionWS =
                    RotateAroundGrassBase(
                        positionWS,
                        pivotWS,
                        windDirection,
                        windAngle
                    );

                float3 viewForwardWS =
                    normalize(GetViewForwardDir());

                float lookDown =
                    saturate(-viewForwardWS.y);

                float topViewEnd =
                    max(
                        _TopViewEnd,
                        _TopViewStart + 0.0001
                    );

                float topViewAmount =
                    smoothstep(
                        _TopViewStart,
                        topViewEnd,
                        lookDown
                    );

                float2 oppositeViewDirection =
                    -viewForwardWS.xz;

                float2 viewFallback =
                    SafeNormalize2(
                        oppositeViewDirection,
                        windDirection
                    );

                float2 toCamera =
                    GetCameraPositionWS().xz
                    - pivotWS.xz;

                float2 perspectiveDirection =
                    SafeNormalize2(
                        toCamera,
                        viewFallback
                    );

                float perspective =
                    IsPerspectiveProjection()
                        ? 1.0
                        : 0.0;

                float2 cameraBendDirection =
                    SafeNormalize2(
                        lerp(
                            viewFallback,
                            perspectiveDirection,
                            perspective
                        ),
                        windDirection
                    );

                float cameraBendAngle =
                    radians(-_TopViewAngle)
                    * topViewAmount
                    * heightMask;

                positionWS =
                    RotateAroundGrassBase(
                        positionWS,
                        pivotWS,
                        cameraBendDirection,
                        cameraBendAngle
                    );

                return TransformWorldToObject(positionWS);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 deformedPositionOS =
                    DeformGrass(
                        input.positionOS.xyz,
                        input.uv
                    );

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        deformedPositionOS
                    );

                output.positionCS =
                    positionInputs.positionCS;

                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _MainTex
                    );

                output.heightMask =
                    saturate(input.uv.y);

                output.fogFactor =
                    ComputeFogFactor(
                        output.positionCS.z
                    );

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 textureSample =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                half alpha =
                    textureSample.a
                    * _BaseColor.a;

                clip(alpha - _Cutoff);

                half3 heightColor =
                    lerp(
                        _BottomColor.rgb,
                        _TopColor.rgb,
                        input.heightMask
                    );

                half3 finalColor =
                    textureSample.rgb
                    * heightColor
                    * _BaseColor.rgb;

                finalColor =
                    MixFog(
                        finalColor,
                        input.fogFactor
                    );

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }

    FallBack Off
}