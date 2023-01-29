#ifndef SimpleColor
#define SimpleColor

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#ifdef HLSL_EDITOR_ONLY
#define _MAIN_LIGHT_SHADOWS
#define _ADDITIONAL_LIGHTS 
#define USE_FAKE_CONE_NORMAL 1
#endif

sampler2D _MainTex;
sampler2D _EmissionMap;

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


#include "PBR_Toon_Lighting.hlsl" 
#include "PBR_Toon_DataStruct.hlsl" 

//VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
//{
//	VertexNormalInputs tbn;
//	tbn.tangentWS = real3(1.0, 0.0, 0.0);
//	tbn.bitangentWS = real3(0.0, 1.0, 0.0);
//	tbn.normalWS = TransformObjectToWorldNormal(normalOS);
//	return tbn;
//}

//One liner pseudo random
float nrand(float2 uv)
{
	return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

			
Varyings Vert(Attributes i)
{
	Varyings o;
	
	
	VertexPositionInputs vertexInput = GetVertexPositionInputs(i.posOS);
	VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

	
	o.positionCS = vertexInput.positionCS;
	
	if (_EyeJitterMul > 0)
	{
		o.positionCS.x += nrand(floor(_Time.xx * 700)) * _EyeJitterMul;
	}
	
	
	float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
	o.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
	o.normalWS = vertexNormalInput.normalWS;
	o.uv = i.uv;
	
#if USE_FAKE_CONE_NORMAL	
	float3 fakeNormal = i.posOS - _FakeNormalOffset;
	fakeNormal.y = 0;
	fakeNormal = normalize(fakeNormal);
	fakeNormal = TransformObjectToWorldNormal(fakeNormal);
	o.normalWS = fakeNormal;
#endif
	
	return o;
}

float4 Frag(Varyings i) : SV_Target
{

	if (_UseScreenDoor)
	{
		ScreenDoorTransparency(_Alpha, i.positionCS, _ScreenDoorScale.xy);
	}
	
	ToonSurfaceData surfaceData = InitializeSurfaceData(i, _BaseColor);
 	LightingData2 lightData = InitializeLightingData(i);
	
#if DISALBLE_TOONSHADER
	return float4(surfaceData.albedo, surfaceData.alpha);
#endif
	
	float3 indirectLight = GetGI_Default(surfaceData, lightData);
	
	Light mainLight = GetMainLight();
	float3 shadowTestPosWS = lightData.positionWS + mainLight.direction * _ReceiveShadowMappingPosOffset;


#ifdef _MAIN_LIGHT_SHADOWS
    // compute the shadow coords in the fragment shader now due to this change
    // https://forum.unity.com/threads/shadow-cascades-weird-since-7-2-0.828453/#post-5516425

    // _ReceiveShadowMappingPosOffset will control the offset the shadow comparsion position, 
    // doing this is usually for hide ugly self shadow for shadow sensitive area like face
    float4 shadowCoord = TransformWorldToShadowCoord(shadowTestPosWS);
    mainLight.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
#endif 
	half3 mainLightResult = ShadeMainLight(surfaceData, lightData, mainLight);
	
	
	half3 additionalLightSumResult = 0;

#ifdef _ADDITIONAL_LIGHTS
    // Returns the amount of lights affecting the object being renderer.
    // These lights are culled per-object in the forward renderer of URP.
    int additionalLightsCount = GetAdditionalLightsCount();
    for (int j = 0; j < additionalLightsCount; ++j)
    {
        // Similar to GetMainLight(), but it takes a for-loop index. This figures out the
        // per-object light index and samples the light buffer accordingly to initialized the
        // Light struct. If ADDITIONAL_LIGHT_CALCULATE_SHADOWS is defined it will also compute shadows.
        int perObjectLightIndex = GetPerObjectLightIndex(j);
        Light light = GetAdditionalPerObjectLight(perObjectLightIndex, lightData.positionWS); // use original positionWS for lighting
        light.shadowAttenuation = AdditionalLightShadow(perObjectLightIndex, shadowTestPosWS, 0, 0, 0); // use offseted positionWS for shadow test

        // Different function used to shade additional lights.
        additionalLightSumResult += ShadeAdditionalLight(surfaceData, lightData, light);
    }
#endif
	
	// emission
	half3 emissionResult = ShadeEmission(surfaceData, lightData);
	
	half3 color = CompositeLight_PreventOverBright(indirectLight, mainLightResult, additionalLightSumResult, emissionResult, surfaceData);
	
	//Fog
	color = MixFog(color, i.positionWSAndFogFactor.w);
	
	return float4(color, surfaceData.alpha);
}

//---------------ShadowCaster--------------------------
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

struct Varyings_Shadow
{
	float4 positionCS : SV_POSITION;
};

//a special uniform for applyShadowBiasFixToHClipPos() only, it is not a per material uniform, 
//so it is fine to write it outside our UnityPerMaterial CBUFFER
float3 _LightDirection;

float4 GetShadowPositionHClip(Attributes input)
{
	float3 positionWS = TransformObjectToWorld(input.posOS.xyz);
	float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	return positionCS;
}

Varyings_Shadow Vert_Shadow(Attributes i)
{
	Varyings_Shadow o;
	VertexPositionInputs vertexInput = GetVertexPositionInputs(i.posOS);
	VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);
	
	o.positionCS = GetShadowPositionHClip(i);
	return o;
}

void Frag_Shadow(Varyings_Shadow i)
{
	if (_UseScreenDoor)
	{
		ScreenDoorTransparency(_Alpha, i.positionCS, _ScreenDoorScale.xy);
	}
	return;
}

//---------------ShadowCaster--------------------------


//---------------DepthOnly--------------------------

struct Varyings_Depth
{
	float4 positionCS : SV_POSITION;
};

Varyings_Depth Vert_Depth(Attributes i)
{
	Varyings_Depth o;
	VertexPositionInputs vertexInput = GetVertexPositionInputs(i.posOS);
	o.positionCS = vertexInput.positionCS;
	return o;
}

void Frag_Depth(Varyings_Depth i)
{
	//if (_UseScreenDoor)
	//{
	//	ScreenDoorTransparency(_Alpha, i.positionCS, _ScreenDoorScale.xy);
	//}
	
	return;
}
//---------------DepthOnly--------------------------

#endif