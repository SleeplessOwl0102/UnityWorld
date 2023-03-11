Shader "Unlit/BillBoardFakeAtmosphere"
{
    //TODO 
    // 当相机很近时，相机实际上会因为遮挡的关系，无法看到整个球体，可能要透过一些Raymarch的方式来改进
    Properties
    {
        [HDR]
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Float) = 0.01

        _Scale ("Scale", range(1,2)) = 1.3
        _PowerInner ("PowerInner", range(0,5)) = 1
        _PowerOuter ("PowerOuter", range(0,5)) = 1
        _AtmosphereDotAdd ("AtmosphereDotAdd", range(-1,5)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="transparent"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        
        Cull Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;

            float4 _Color;
            float _Width;
            float _AtmosphereDotAdd;
            float _PowerInner;
            float _PowerOuter;
            float _Scale;

            v2f vert(appdata v)
            {
                v2f o;
                //o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;

                
                //billboard mesh towards camera
                // float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                // float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23,
                //                            1);
                // float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
                // float4 outPos = mul(UNITY_MATRIX_P, viewPos);
                //
                // o.pos = outPos;
                // return o;



                //only for quad
                 float3 posOS = v.vertex.xyz;
                 float3 cameraRightWS = UNITY_MATRIX_V[0].xyz;  //world space camera Right unit vector
                 float3 cameraUpWS = UNITY_MATRIX_V[1].xyz;  //world space camera Up unit vector
                 float3 cameraRightOS = normalize(mul((float3x3)unity_ObjectToWorld, cameraRightWS));
                 float3 cameraUpOS = normalize( mul((float3x3)unity_ObjectToWorld, cameraUpWS));
                 posOS = posOS.x * cameraRightOS + posOS.y * cameraUpOS;
                

                o.pos = UnityObjectToClipPos(posOS);
                //

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = abs(i.uv - 0.5) * 2;

                float2 normal = normalize(i.uv - 0.5);
                float2 sun = normalize(float2(-1, 1));
                float dotLight = dot(normal, sun) + _AtmosphereDotAdd;

                float dist = length(uv);
                float scale1Dist = 1 / _Scale;
                dist = 1 - distance(dist, scale1Dist);

                float w = .5;
                float lineA = smoothstep(w - _Width, w + _Width, dist - .5);
                float4 col = _Color;

                float powva = _PowerOuter;
                if (length(uv) <= scale1Dist)
                    powva = _PowerInner;
                
                col.a = pow(lineA, powva) * dotLight;

                return col;
            }
            ENDCG
        }
    }
}