
Shader "RenLab/Skybox/VolumticClound"
{
    Properties
    {
        NoiseTex("Texture", 3D) = "white" {}
        NoiseTex2("Texture", 3D) = "white" {}

        _BgColorTop("_BgColorTop", Color) = (1,1,1)
        _BgColorMiddle("_BgColorMiddle", Color) = (0,0,0)
        _BgColorHeightPow("_BgColorHeightPow", range(0.0001,1)) = 1

        _CloundCol("_CloundCol", Color) = (1,1,1)
        _CloundShadowCol("_CloundShadowCol", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature FUZZY
            
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Core.hlsl"
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Input.hlsl"
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Hash.hlsl"
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Util.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 color : COLOR;
                float3 posWS : TEXCOORD1;
                float3 eyeRay : TEXCOORD2;
                
            };

            float3 _BgColorMiddle, _BgColorTop;
            float _BgColorHeightPow;

            float3 _CloundCol, _CloundShadowCol;

            Texture3D<float> NoiseTex;
            SamplerState samplerNoiseTex;
            Texture3D<float> NoiseTex2;
            SamplerState samplerNoiseTex2;

            float4 _WorldSpaceLightPos0;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.eyeRay = normalize(o.posWS);
                float height = v.uv.y; //top = 1, middle = 0, bottom = -1
                height = saturate(height);
                o.color = lerp(_BgColorMiddle, _BgColorTop, pow(height, _BgColorHeightPow));
                return o;
            }

            float SampleNoise(float3 pos)
            {
                float dens =  saturate(NoiseTex.SampleLevel(samplerNoiseTex, pos , 0) );
                dens = smoothstep(.38, .6, dens);
                float dens2 = saturate(NoiseTex2.SampleLevel(samplerNoiseTex2, pos * 25 + _Time.x * float3(-5, .4, 3), 0));
                float value = (dens * 1.5 + dens2 * .7) * .5;
                return value;
            }

            float4 frag(v2f i) : SV_Target
            {
                //return float4(i.eyeRay, 1);
                float3 posWS = i.posWS;
                float s = 3000 / i.eyeRay.y;
                float3 startPos = i.eyeRay * s;
                startPos += i.eyeRay * nrand(startPos.xy) * 100;


                float3 curPos = startPos;
                float maxStep = 30;
                float dens = 0;
                float value;
                float shadowTotal = 0;
                
                //[Loop]
                for (int step = 0; step <= maxStep; step++)
                {
                    
                    float dist = length(curPos);
                    float mul = (dist * 2) / 10000;
                    float mul2 = pow((step + maxStep) / maxStep,2);
                    //mul = 1;
                    //mul2 = 1;
                    value = SampleNoise(curPos * 0.00004 + _Time.x * float3(.5, .1, .5));

                    value = smoothstep(.3, .55, value) / maxStep ;
                    dens += value;

                    float3 lightStartPos = curPos;
                    float3 lightCurPos = lightStartPos;
                    float maxLightStep = 5;
                    float value2 = 0;
                    float shadow = 0;
                    //[Loop]
                    for (int step = 0; step <= maxLightStep; step++)
                    {
                        value2 = SampleNoise(lightCurPos * 0.00004 + _Time.x * float3(.5, .1, .5));
                        value2 = value2 / maxLightStep/1.4;
                        //value2 = smoothstep(.4, .65, value2) / maxLightStep;
                        shadow += value2;
                        lightCurPos += 100 * float3(0,1,0);
                    }
                    shadowTotal += shadow ;

                    curPos += 100* mul * i.eyeRay;
                }
                dens = saturate(dens);
                dens = smoothstep(.3, .8, dens);
                float3 cloundCol = dens * lerp(_CloundCol, _CloundShadowCol,saturate( shadowTotal / maxStep));


                float3 combineCol = i.color * (1 - dens);
                combineCol = combineCol + cloundCol;

                return float4(combineCol, 1);
            }
            ENDHLSL
        }
    }
}