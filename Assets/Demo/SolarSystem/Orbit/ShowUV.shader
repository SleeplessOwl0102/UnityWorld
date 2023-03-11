Shader "Unlit/ShowUV"
{
    Properties
    {
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
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = float4(1,1,1,1);
                col.rg = i.uv.xy;
                return col;
            }
            ENDCG
        }
    }
}