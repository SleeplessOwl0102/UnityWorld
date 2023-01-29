Shader "#Graphic Learning/Normal/BumpMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _BumpTex ("Texture", 2D) = "white" { }
        
        _BumpScale ("Bump Scale", Range(-30, 30.0)) = 10.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Frag
            
            ENDHLSL
        }
    }
    
    HLSLINCLUDE
    
    #define REN_LIGHTING
    #include "Assets/SleeplessOwl/RenLib/ShaderLib/Core.hlsl"
    
    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    sampler2D _BumpTex;
    float4 _BumpTex_TexelSize;
    
    float _BumpScale;
    
    struct a2v
    {
        float3 posOS: POSITION;
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
        float4 color: COLOR0;
        float2 uv: TEXCOORD0;
    };
    
    struct v2f
    {
        float4 posCS: SV_POSITION;
        float3 worldNormal: TEXCOORD0;
        float2 uv: TEXCOORD1;
    };
    
    v2f Vert(a2v i)
    {
        v2f o;
        o.posCS = TransformObjectToHClip(i.posOS);
        o.uv = TRANSFORM_TEX(i.uv, _MainTex);
        o.worldNormal = TransformObjectToWorldNormal(i.normal);
        return o;
    }
    
    float4 Frag(v2f i): SV_Target
    {
        Light mainLight = GetMainLight();
        half4 combineColor = tex2D(_MainTex, i.uv);
        
        //取樣bump貼圖,需要知道該點的斜率，xy方向分別求，所以對於一個點需要取樣四次
        float2 du = float2(0.5 * _BumpTex_TexelSize.x, 0);
        float2 dv = float2(0, 0.5 * _BumpTex_TexelSize.y);
        float bumpValueU = tex2D(_BumpTex, i.uv - du).r - tex2D(_BumpTex, i.uv + du).r;
        float bumpValueV = tex2D(_BumpTex, i.uv - dv).r - tex2D(_BumpTex, i.uv + dv).r;
        

        //用上面的斜率來修改法線的偏移值
        //當_BumpScale > 0, 視覺效果相當於r值越低,越凹陷
        float3 oriNormalWS = normalize(i.worldNormal);
        float3 normalWS = float3(
            oriNormalWS.x + (bumpValueU * _BumpScale ),
            oriNormalWS.y,
            oriNormalWS.z + (bumpValueV * _BumpScale));
        normalWS = normalize(normalWS);

        //若都是單位向量（長度為1），它們的點積就是它們的夾角的餘弦
        //dot(法線方向,表面到光源的方向)
        float lambert = Lambert(normalWS, normalize(mainLight.direction), .1);
        combineColor.rgb = combineColor.rgb * lambert;
        //combineColor.rgb = normalWS;
        return combineColor;
    }
    
    ENDHLSL
    
}
