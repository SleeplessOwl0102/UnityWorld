#ifndef Outline
#define Outline

#define REN_LIGHTING
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Core.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Input.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Noise.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Util.hlsl"

//#define ENABLE_AVERAGE_NORMAL 1
//#define ENABLE_SCREEN_DOOR_TRANSPARENCY 1 

CBUFFER_START(UnityPerMaterial)
half _shadowSmooth;
half _shadowRange;
half3 _shadowColor;

half _OutlineWidth;
half4 _OutLineColor;

float4 _ScreenDoorScale;
float _Alpha;
CBUFFER_END

struct v2f
{
	float4 posCS : SV_POSITION;
	float3 posWS : TEXCOORD1;
	float4 color : TEXCOORD2;
	
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	float4 screenPos : TEXCOORD5;
#endif
};

struct a2v
{
	float3 posOS : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

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
	
	float scale = pow((1 / max((abs(posVS.z) - 8), 1)), .7);
	
	
	float jitter = PerlinNoise_3D(i.posOS * 3);
	jitter = max(jitter - .3, 0);
	scale *= jitter;
	
	posCS.xy += 0.001 * _OutlineWidth * ndcNormal.xy * scale;
	o.posCS = posCS;
	
	o.posWS = TransformObjectToWorld(i.posOS);
	o.color = i.color;
	
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	o.screenPos = ComputeScreenPos(o.posCS);
#endif	
    return o;
}

float4 Frag(v2f i) : SV_Target
{
	
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	ScreenDoorTransparency(_Alpha, i.screenPos);
#endif	
	
	half4 combineColor = _OutLineColor;	
	return combineColor;
}

#endif