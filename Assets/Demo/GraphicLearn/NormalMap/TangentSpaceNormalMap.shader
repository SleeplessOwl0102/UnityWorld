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
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"
        }
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

    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
    {
        return cross(normal, tangent.xyz) *
            (binormalSign * unity_WorldTransformParams.w);
    }

    float4 Frag(v2f i): SV_Target
    {
        Light mainLight = GetMainLight();
        half4 combineColor = tex2D(_MainTex, i.uv);

        float3 worldNormal;

        //法线贴图的值（切线空间法向量）
        //Unity中提供了UnpackNormal來協助取值範圍的轉換與平台格式問題s
        float3 noalmalMap = UnpackNormalScale(tex2D(_NormalMap, i.uv), _BumpScale);

        //之所以加了这个值，是因为在OpenGL和DirectX平台上UV走向不一样
        //OpenGL平台，U水平向右，V垂直向上即+y
        //DirectX平台，U水平向右，V垂直向下即-y
        //所以在DirectX平台上，需要将法线贴图的V方向取反
        float3 bitangent = normalize(cross(i.worldNormal, i.tangent.xyz) * i.tangent.w);
        
        //建立TBN矩阵
        //Unity的矩阵是先填充行，再填充列
        // Tangent x y z
        // Binormal x y z
        // Normal x y z
        float3x3 TBN = float3x3(i.tangent.xyz, bitangent, i.worldNormal);
        worldNormal = normalize(mul(noalmalMap, TBN)); //法线贴图的切线空间法线方向->世界空间法线方向

        //另一种理解方式，计算结果是相同的
        worldNormal = normalize(
            noalmalMap.x * i.tangent + //R通道，计算tangent方向的法线偏移量
            noalmalMap.y * bitangent + //G通道，计算binormal方向的法线偏移量
            noalmalMap.z * i.worldNormal //B通道，值的大小在结果正规化后，相当于控制凹凸的程度
        );


        float lambert = Lambert(worldNormal, normalize(mainLight.direction), .1);
        combineColor.rgb = combineColor.rgb * lambert;
        return combineColor;
    }
    ENDHLSL

}