Shader "Unlit/ScreenSpaceOffsetOrbitLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Float) = 0.01
        _Scale ("Scale", Float) = 1
        
        [Toggle()]
        _BlendMode ("Blend Mode", Float) = 0
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
            Cull off
            
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
            float _BlendMode;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex).xyz;
                float3 cameraDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);

                
                float3 normal = (v.vertex.xyz);
                float3 worldNormal = normalize(mul(UNITY_MATRIX_M, normal));
                float3 crossVector = normalize(cross(cameraDir, worldNormal));

                float3 final = _BlendMode == 0 ? crossVector : worldNormal;
                //float3 clipNormal = normalize(UnityObjectToClipPos(worldNormal + crossVector).xyz);
                float3 clipNormal = normalize(mul( UNITY_MATRIX_VP, final ));
                float ratio = _ScreenParams.y/_ScreenParams.x;
                clipNormal.x *=   ratio;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.xy += float3(clipNormal * _Width* o.vertex.w * (v.uv.y-0.5)).xy;
                
                
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = _Color;
                col.a = 1-(distance(i.uv.y,.5f)) - 0.5;
                col.a = smoothstep(0.01,0.15,col.a) * .5;
                return col;
            }
            ENDCG
        }
    }
}