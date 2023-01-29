#ifndef Outline
#define Outline

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Util.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Noise.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Hash.hlsl"


#ifdef HLSL_EDITOR_ONLY
#define ENABLE_AVERAGE_NORMAL 1
#define ENABLE_SCREEN_DOOR_TRANSPARENCY 1 
#define ENABLE_NOISE_OUTLINE_WIDTH 1
#endif


CBUFFER_START(UnityPerMaterial)
float4 _BaseColor;


// lighting
half3 _IndirectLightMinColor;
half _IndirectLightMul;
half _DirectLightMultiplier;
half _CelShadeMidPoint;
half _CelShadeSoftness;
half _MainLightIgnoreCelShade;
half _AdditionalLightIgnoreCelShade;

//Specular
float _UseSpecular;
half _SpecularPow;
half _SpecularMul;

//Shadow 
float _ReceiveShadowMappingAmount;
float _ReceiveShadowMappingPosOffset;

//Emission
float _UseEmission;
half3 _EmissionColor;
half _EmissionMulByBaseColor;
half3 _EmissionMapChannelMask;

//RimLight
float _RimLightPower;
float3 _RimLightColor;

//Outline
half _OutlineWidth;
half4 _OutLineColor;
half _OutlineStartScaleDistance;

//Transparent
float _UseScreenDoor;
float4 _ScreenDoorScale;
float _Alpha;

float _EyeJitterMul;
float4 _FakeNormalOffset;
CBUFFER_END

struct v2f
{
	float4 posCS : SV_POSITION;
	float3 posWS : TEXCOORD1;
	float4 color : TEXCOORD2;
};

struct a2v
{
	float3 posOS : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

float3 TransformNormalViewToNDC(float3 normalVS, float posCS_W)
{
	return normalize(mul((float3x3) UNITY_MATRIX_P, normalVS.xyz)) * posCS_W;
}

v2f Vert(a2v i)
{
	v2f o;
	float4 posCS = TransformObjectToHClip(i.posOS);
	float3 posWS = TransformObjectToWorld(i.posOS);
	float3 posVS = TransformWorldToView(posWS);
	
#if ENABLE_AVERAGE_NORMAL
	half3 viewNormal = mul((float3x3) UNITY_MATRIX_IT_MV, i.tangent.xyz);
#else
	half3 viewNormal = mul((float3x3) UNITY_MATRIX_IT_MV, i.normal.xyz);
#endif
	
	half3 ndcNormal = TransformNormalViewToNDC(viewNormal.xyz, posCS.w);
	ndcNormal.x *= GetAspect();
	
	float scale = pow((1 / max((abs(posVS.z) - _OutlineStartScaleDistance), 1)), .7);
	
#if ENABLE_NOISE_OUTLINE_WIDTH
	float jitter = PerlinNoise_3D(i.posOS * 2);
	jitter = jitter * 2;
	scale *= jitter;
#endif
	
	posCS.xy += 0.001 * _OutlineWidth * ndcNormal.xy * scale;
	o.posCS = posCS;
	
	o.posWS = TransformObjectToWorld(i.posOS);
	o.color = i.color;
	
    return o;
}

float4 Frag(v2f i) : SV_Target
{
#if DISALBLE_TOONSHADER
	discard;
#endif
	
	if (_UseScreenDoor)
	{
		ScreenDoorTransparency(_Alpha, i.posCS);
	}
	
	half4 combineColor = _OutLineColor;	
	return combineColor;
}

#endif