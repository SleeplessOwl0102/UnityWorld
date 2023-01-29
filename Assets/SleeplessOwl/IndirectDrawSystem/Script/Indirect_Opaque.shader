Shader "DrawMeshIndirect/Opaque"
{
    Properties
    {
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)

        _BaseColorTexture("_BaseColorTexture", 2D) = "white" {}

        _GroundColor("_GroundColor", Color) = (0.5,0.5,0.5)
        _Scale("Scale", float) = 1
        _Offset("_Offset", vector) = (0,0,0,0)

        [Toggle(BILLBOARD)] BillBoard("BillBoard", Float) = 0
        [Toggle(RANDOM_ROTATE)] RandomRotate("Random Rotate", Float) = 0
        [Toggle(RANDOM_SCALE)] RandomScale("Random Scale", Float) = 0

        [Toggle(ENALBLE_SWING)] EnableSwing("Enable Swing", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Off 
            ZTest LEqual
            ZWrite On

            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local BILLBOARD
            #pragma shader_feature_local RANDOM_ROTATE
            #pragma shader_feature_local RANDOM_SCALE
            #pragma shader_feature_local ENALBLE_SWING
        
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

            struct a2v
            {
                float4 posOS   : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 posCS  : SV_POSITION;
                float3 posWS  : TEXCOORD0;
                float3 color : COLOR;
                float3 normal :TEXCOORD1;
                float3 ambient : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)

            half3 _BaseColor;
            half3 _GroundColor;
            float _Scale;
            float3 _Offset;

            StructuredBuffer<float3> _AllInstancesTransformBuffer;
            StructuredBuffer<uint> _VisibleInstancesIDBuffer;

            float4 _BaseColorTexture_ST;
            CBUFFER_END

            sampler2D _BaseColorTexture;

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

            v2f vert(a2v vi, uint instanceID : SV_InstanceID)
            {
                v2f vo;
                
                float3 pivotPosWS = _AllInstancesTransformBuffer[_VisibleInstancesIDBuffer[instanceID]];
                float3 posOS = vi.posOS.xyz;

#if BILLBOARD
                float3 cameraRightWS = UNITY_MATRIX_V[0].xyz;  //world space camera Right unit vector
                float3 cameraUpWS = UNITY_MATRIX_V[1].xyz;  //world space camera Up unit vector
                //UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector
                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;

                float off = (sin(pivotPosWS.x * 95.4643 + pivotPosWS.z) * .15 + .85);
                posOS = vi.posOS.x * cameraRightWS * off;
                posOS += float3(vi.posOS.y * .3, vi.posOS.y, vi.posOS.y*.3) * cameraUpWS;
#endif

                float rand = random(pivotPosWS.xz);
                posOS *= _Scale;

#if RANDOM_ROTATE
                posOS.xz = Rotate(posOS.xz, rand * 180);
#endif
#if RANDOM_SCALE
                posOS *= max(.5, rand);
#endif

                float3 posWS = posOS + pivotPosWS + _Offset;
                half3 baseColor = tex2Dlod(_BaseColorTexture, float4(TRANSFORM_TEX(posWS.xz, _BaseColorTexture), 0, 0));//sample mip 0 only

#if ENALBLE_SWING
                float offsetX = (sin(_Time.y * 2 + posWS.x * .5) * .17) * posOS.y;
                float offsetZ = sin(_Time.y * 3 + posWS.y * .5) * .12 * posOS.y;
                posWS.x += offsetX;
                posWS.z += offsetZ;
                posWS.y -= sin(length(float2(posOS.x + offsetX, posOS.z + offsetZ))) * .5 * posOS.y;
#endif
                vo.posCS = TransformWorldToHClip(posWS);

                //不須使用TransformObjectToWorldNormal，因為DrawIndirectInstance不包含旋轉，所以向量本來就是world space了。
                vo.normal = vi.normal;
                vo.normal.xz = Rotate(vo.normal.xz, rand * 180);

                //half3 albedo = lerp(_GroundColor, _BaseColor * (baseColor + .2), min(1, posOS.y * 30));
                half3 albedo = _BaseColor * (baseColor + .2);
                float fogFactor = ComputeFogFactor(vo.posCS.z);

               

                vo.color = MixFog(albedo, fogFactor);
                vo.ambient = SampleSH(vo.normal);
                vo.posWS = posWS;
                return vo;
            }

            half4 frag(v2f fi, float FacingSign : VFACE) : SV_Target
            {
                
                float3 ambient = SampleSH(fi.normal);
                Light mainLight;
                mainLight = GetMainLight(TransformWorldToShadowCoord(fi.posWS));

                float3 normal = fi.normal;
                if (FacingSign <= 0)
                {
                    normal *= -1;
                }
                float lambert = dot(mainLight.direction, normalize(normal));
                lambert = max(0, lambert) * 0.2 + 0.8;
                
                float3 light = fi.ambient + mainLight.color * lambert * mainLight.shadowAttenuation;
                float3 color = fi.color * light;
                return half4(color,1);
            }
            ENDHLSL
        }


        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            Cull Off
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local BILLBOARD
            #pragma shader_feature_local RANDOM_ROTATE
            #pragma shader_feature_local RANDOM_SCALE
            #pragma shader_feature_local ENALBLE_SWING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct a2v
            {
                float4 posOS   : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 posCS  : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)

            half3 _BaseColor;
            half3 _GroundColor;
            float _Scale;
            float3 _Offset;

            StructuredBuffer<float3> _AllInstancesTransformBuffer;
            StructuredBuffer<uint> _VisibleInstancesIDBuffer;
            CBUFFER_END

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float2 Rotate(float2 pos,float euler)
            {
                float s, c;
                sincos(euler, s, c);
                pos = mul(float2x2(c, -s, s, c), pos);
                return pos;
            }

            v2f vert(a2v vi, uint instanceID : SV_InstanceID)
            {
                v2f vo;

                float3 pivotPosWS = _AllInstancesTransformBuffer[_VisibleInstancesIDBuffer[instanceID]];
                float3 posOS = vi.posOS.xyz;

#if BILLBOARD
                float3 cameraRightWS = UNITY_MATRIX_V[0].xyz;  //world space camera Right unit vector
                float3 cameraUpWS = UNITY_MATRIX_V[1].xyz;  //world space camera Up unit vector
                //UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector
                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;

                float off = (sin(pivotPosWS.x * 95.4643 + pivotPosWS.z) * .15 + .85);
                posOS = vi.posOS.x * cameraRightWS * off;
                posOS += float3(vi.posOS.y * .3, vi.posOS.y, vi.posOS.y * .3) * cameraUpWS;
#endif

                float rand = random(pivotPosWS.xz);
                posOS *= _Scale;

#if RANDOM_ROTATE
                posOS.xz = Rotate(posOS.xz, rand * 180);
#endif
#if RANDOM_SCALE
                posOS *= max(.5, rand);
#endif

                float3 posWS = posOS + pivotPosWS + _Offset;
#if ENALBLE_SWING
                float offsetX = (sin(_Time.y * 2 + posWS.x * .5) * .17) * posOS.y;
                float offsetZ = sin(_Time.y * 3 + posWS.y * .5) * .12 * posOS.y;
                posWS.x += offsetX;
                posWS.z += offsetZ;
                posWS.y -= sin(length(float2(posOS.x + offsetX, posOS.z + offsetZ))) * .5 * posOS.y;
#endif
                vo.posCS = TransformWorldToHClip(posWS);
                return vo;
            }

            half4 frag(v2f fi) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}