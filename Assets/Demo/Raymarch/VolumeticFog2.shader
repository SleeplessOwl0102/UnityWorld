Shader "Raymarch/Volumetic Fog2"
{
    Properties
    {
        NoiseTex("Texture", 3D) = "white" {}
        NoiseTex2("Texture", 3D) = "white" {}
        _FogColor("_FogColor", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZTest Off
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Core.hlsl"
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Noise.hlsl"
            #include "Assets/SleeplessOwl/RenLib/ShaderLib/Util.hlsl"


            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };


            TEXTURE3D(NoiseTex);
            SAMPLER(samplerNoiseTex);
            TEXTURE3D(NoiseTex2);
            SAMPLER(samplerNoiseTex2);

            float4 _FogColor;

            TEXTURE2D(_CameraDepthTexture);

#define MAX_STEPS 10
#define MAX_DIST 20000
#define SURF_DIST 1e-3//0.001

            float GetDist(float3 p)
            {
                //round box
                float b = .5;
                float3 q = abs(p) - b;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);

                //sphere
                float d = length(p) - 0.5;
                return d;
            }

            float SampleDepthTexture(float2 uv)
            {
                return LOAD_TEXTURE2D_LOD(_CameraDepthTexture, uv.xy, 0).r;
            }

            float4 _WorldSpaceLightPos0;

            float Raymarch(float3 ro, float3 rd, float depth)
            {
                float dO = 0;
                float dS;

                float3 p = ro;

                //逼近邊緣
                for (int i = 0; i < 10; i++)
                {
                    dS = GetDist(p);
                    if (dS < 0)
                        break;
                    dO += dS;
                    p = ro + dO * rd;
                    if (dS<SURF_DIST || dO>MAX_DIST)
                        break;
                }
                bool leave = 0;
                float dens = 0;
                for (int j = 0; j < 100; j++)
                {
                    p = p + ((0.01 + nrand(p.xy) / 400) * rd);
                    dS = GetDist(p);

                    float4 cp = TransformObjectToHClip(p);
                    //from clip space to NDC
                    float z = cp.z / cp.w;

                    if (z < depth)
                        break;

                    if (dS < 0)
                    {
                        float3 s = p;
                        float3 s2 = p;
                        float3 s3 = p;
                        s *= 1;
                        s.x += _Time.x * .8;
                        s.y += _Time.x * .65;
                        s.z += _Time.x * 0;

                        s2 *= 3.5;
                        s2.x += _Time.x * 1.5;
                        s2.y += _Time.x * 1;
                        s2.z += _Time.x * 2;

                        float tt = SAMPLE_TEXTURE3D(NoiseTex, samplerNoiseTex, s);
                        float tt2 = SAMPLE_TEXTURE3D(NoiseTex2, samplerNoiseTex2, s2);
                        float noise = (1 - tt) * 3 + .3;
                        noise += tt2 * 3;

                        noise *= 1-abs(p.y + .5);
                        noise = smoothstep(.3, .35, noise);
                        dens += noise * min(1, abs(dS) * 10);
                    }
                    
                }
                dens = dens / 100;
                return dens;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                o.hitPos = v.vertex;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                //ray origin
                //object space camera pos
                float3 ro = i.ro;//float3(0,0,-3);

                //ray direction
                //hit pos 包圍盒邊界
                float3 rd = normalize(i.hitPos - ro);//normalize(float3(uv.x, uv.y, 1));

                float depth = SampleDepthTexture(i.vertex.xy);
                float dens = Raymarch(ro, rd, depth);

                float4 col = 0;
                col.rgb = _FogColor.rgb;
                col.a = dens * _FogColor.a;
                
                return col;
            }
            ENDHLSL
        }
    }
}
