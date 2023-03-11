Shader "Unlit/OrbitLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Float) = 0.01
        _Scale ("Scale", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="transparent" "RenderPipeline"="UniversalRenderPipeline"
        }
        LOD 100

        Pass
        {

            blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Width;
            float _Scale;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex));
                float distToCamera = length(worldPos - _WorldSpaceCameraPos);
                o.worldPos = worldPos;
                o.worldPos.w = o.vertex.w;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {

                
                // sample the texture
                i.uv = abs(i.uv - 0.5) * 2;

                float4 col = _Color;

                float dist = length(i.uv.xy);
                float cut = step(dist, 1.001);
                dist = 1 - distance(dist, 1);

                float nw = _Width * pow(i.worldPos.w,1.5)/_Scale;
                
                float lineA = smoothstep(0.5 - nw, 0.5 + nw, dist * .5);

                col.a = pow(length(i.uv.xy), 12)*.2f;
                col.a *= cut;
                col += lineA * 2;
                return col;
            }
            ENDCG
        }
    }
}