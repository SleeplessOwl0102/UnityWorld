Shader "DrawMeshIndirect/AlphaClip"
{
    Properties
    {
        _BaseTex("_BaseTex", 2D) = "white" { }

        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
        _GroundColor("_GroundColor", Color) = (0.5,0.5,0.5)
        _Scale("Scale", float) = 1

        [Toggle(BILLBOARD)] BillBoard("BillBoard", Float) = 0
        [Toggle(RANDOM_ROTATE)] RandomRotate("Random Rotate", Float) = 0
        [Toggle(RANDOM_SCALE)] RandomScale("Random Scale", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Off //use default culling because this shader is billboard 
            ZTest LEqual
            ZWrite On
            Blend Off
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local BILLBOARD
            #pragma shader_feature_local RANDOM_ROTATE
            #pragma shader_feature_local RANDOM_SCALE

            // -------------------------------------
            // Universal Render Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            // -------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 posOS   : POSITION;
                float4 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 posCS  : SV_POSITION;
                float3 posWS  : TEXCOORD0;
                float2 uv : TEXCOORD1;
                half3 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)

            half3 _BaseColor;
            half3 _GroundColor;
            float _Scale;

            StructuredBuffer<float3> _AllInstancesTransformBuffer;
            StructuredBuffer<uint> _VisibleInstancesIDBuffer;
            CBUFFER_END

            TEXTURE2D_X(_BaseTex);
            SAMPLER(sampler_BaseTex);

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float2 Rotate(float2 pos,float euler)
            {
                //example rotate Y
                //posOS.xz = Rotate(posOS.xz, 90);

                float s, c;
                sincos(euler, s, c);
                pos = mul(float2x2(c, -s, s, c), pos);
                return pos;
            }

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;
                //UNITY_MATRIX_V[0].xyz == world space camera Right unit vector
                float3 cameraRightWS = UNITY_MATRIX_V[0].xyz;
                //UNITY_MATRIX_V[1].xyz == world space camera Up unit vector
                float3 cameraUpWS = UNITY_MATRIX_V[1].xyz;
                //UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector
                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;

                //we pre-transform to posWS in C# now
                float3 pivotPosWS = _AllInstancesTransformBuffer[_VisibleInstancesIDBuffer[instanceID]];

                float3 posOS = IN.posOS;
#if BILLBOARD
                float off = (sin(pivotPosWS.x * 95.4643 + pivotPosWS.z) * .15 + .85);
                posOS = IN.posOS.x * cameraRightWS * off;
                posOS += float3(IN.posOS.y * .3, IN.posOS.y, IN.posOS.y*.3) * cameraUpWS;
#endif

                float rand = random(pivotPosWS.xz);
                posOS *= _Scale;

#if RANDOM_ROTATE
                posOS.xz = Rotate(posOS.xz, rand * 5);
#endif
#if RANDOM_SCALE
                posOS *= max(.5, rand);
#endif

                float3 posWS = posOS + pivotPosWS;


                Light mainLight;
                mainLight = GetMainLight(TransformWorldToShadowCoord(posWS));

                float offsetX = (sin(_Time.y * 2 + posWS.x * .5) * .17) * posOS.y;
                float offsetZ = sin(_Time.y * 3 + posWS.y * .5) * .12 * posOS.y;
                posWS.x += offsetX;
                posWS.z += offsetZ;
                posWS.y -= sin(length(float2(posOS.x + offsetX, posOS.z + offsetZ))) * .5 * posOS.y;

                OUT.posCS = TransformWorldToHClip(posWS);

                half3 albedo = lerp(_GroundColor,_BaseColor, max(1,posOS.y  * 4));
                albedo -= abs(sin(pivotPosWS.x * 2 + pivotPosWS.y * 1)) * .1;


                //albedo *= min(dot(mainLight.direction, -IN.normalOS) * .2 + .8, min(1, mainLight.shadowAttenuation * 2.7)) ;
                float fogFactor = ComputeFogFactor(OUT.posCS.z);
                OUT.color = MixFog(albedo, fogFactor);
                OUT.posWS = posWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D_X(_BaseTex, sampler_BaseTex, IN.uv);
                clip(texColor.a - .95);

                Light mainLight;
                mainLight = GetMainLight(TransformWorldToShadowCoord(IN.posWS));
                IN.color *= min(1, mainLight.shadowAttenuation + .4);
                texColor.rgb *= IN.color;
                return float4(texColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}