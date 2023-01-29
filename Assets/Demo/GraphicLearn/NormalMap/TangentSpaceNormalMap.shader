Shader "#Graphic Learning/Normal/Tangent Space NormalMap"
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
    #define _NORMALMAP
    #include "Packages/RenLib/ShaderLib/Core.hlsl"
    
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
        float4 tangent : TEXCOORD2;
        float3 worldPos : TEXCOORD3;
        float2 uv: TEXCOORD1;
    };

    float3 UnityObjectToWorldDir(in float3 dir) 
    {
        return normalize(mul((float3x3)unity_ObjectToWorld, dir));
    }

    v2f Vert(a2v i)
    {
        v2f o;
        o.posCS = TransformObjectToHClip(i.posOS);
        o.worldPos = TransformObjectToWorld(i.posOS);
        o.uv = TRANSFORM_TEX(i.uv, _MainTex);
        o.worldNormal = TransformObjectToWorldNormal(i.normal);
        o.tangent = float4(UnityObjectToWorldDir(i.tangent.xyz), i.tangent.w);
        return o;
    }
    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
        return cross(normal, tangent.xyz) *
            (binormalSign * unity_WorldTransformParams.w);
    }
    float4 Frag(v2f i): SV_Target
    {
        Light mainLight = GetMainLight();
        half4 combineColor = tex2D(_MainTex, i.uv);
        
        float3 normal;

        float3 tangentNormal;
        tangentNormal = UnpackNormalScale(tex2D(_NormalMap, i.uv), _BumpScale);
        float3 binormal = cross(i.worldNormal, i.tangent.xyz) * i.tangent.w;

        normal = normalize(
            tangentNormal.x * i.tangent +
            tangentNormal.y * binormal +
            tangentNormal.z * i.worldNormal
        );

        float lambert = Lambert(normal, normalize(mainLight.direction), .1);
        combineColor.rgb = combineColor.rgb * lambert;
        return combineColor;
    }
    
    ENDHLSL
    
}
