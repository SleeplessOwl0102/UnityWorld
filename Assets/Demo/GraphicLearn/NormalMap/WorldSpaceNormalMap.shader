Shader "#Graphic Learning/Normal/World Space NormalMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _NormalMap ("Texture", 2D) = "white" { }
        
        _BumpScale ("Bump Scale", Range(-30, 30.0)) = 10.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
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
    
    sampler2D _NormalMap;
    float4 _NormalMap_TexelSize;
    
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
        
        float3 normal;
        
        //一般情況下unity中normal map使用DXT5nm (R5 G6 B5 A8) 格式儲存，所以將X移到W，以換取更高精度
        //mobile平台不支持DXT5格式，此時會使用常規的RGB編碼
        //Unity中提供了UnpackNormal來協助取值範圍的轉換與平台格式問題
        //ex.
        //normal = UnpackNormalScale(tex2D(_NormalMap, i.uv), _BumpScale);

        //以下為DXT5nm格式下的計算過程
        //normal map中值的範圍為(0,1)，在使用前須將其轉換回(-1,1)的區間
        normal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;

        //透過縮放xy分量，來調整之後計算出來的z分量大小，控制凹凸程度
        normal.xy *= _BumpScale;
        //為DXT5nm格式下normal map只儲存了xy分量但因為其為單位向量，所以可以反推出z值
        normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));

        //因為使用XZ平面，無偏移的法向量是(0,1,0)，而normal map轉換到(-1,1)區間後無偏移時是(0,0,1)，所以yz值交換
        normal = normal.xzy;
        normal = normalize(normal);

        float lambert = Lambert(normal, normalize(mainLight.direction), .1);
        combineColor.rgb = combineColor.rgb * lambert;
        return combineColor;
    }
    
    ENDHLSL
    
}
