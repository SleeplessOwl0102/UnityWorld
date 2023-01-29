#ifndef PBR_Toon_Lighting
#define PBR_Toon_Lighting

#include "PBR_Toon_DataStruct.hlsl" 
#include "PBR_Toon_Diffuse.hlsl" 

half3 GetGI_Default(ToonSurfaceData surfaceData, LightingData2 lightingData)
{
    // hide 3D feeling by ignoring all detail SH
    // SH 1 (only use this)
    // SH 234 (ignored)
    // SH 56789 (ignored)
    // we just want to tint some average envi color only
	half3 averageSH = SampleSH(0);
	half3 indirectLight = averageSH * _IndirectLightMul;
	
	
	indirectLight = max(indirectLight, _IndirectLightMinColor);
	return indirectLight;
}

half3 ShadeMainLight(ToonSurfaceData surfaceData, LightingData2 lightingData, Light light)
{
	half3 N = lightingData.normalWS;
	half3 L = light.direction;
	half3 V = lightingData.viewDirectionWS;
	half3 H = normalize(L + V);
	half3 R = reflect(-L, N);
	half NoL = dot(N, L);

	half lightAttenuation = 1;

	lightAttenuation *= lerp(1, light.shadowAttenuation, _ReceiveShadowMappingAmount);
	lightAttenuation *= min(4, light.distanceAttenuation); //prevent light over bright if point/spot light too close to vertex

	half celShadeResult = smoothstep(_CelShadeMidPoint - _CelShadeSoftness, _CelShadeMidPoint + _CelShadeSoftness, NoL);
	lightAttenuation *= lerp(celShadeResult, 1, _MainLightIgnoreCelShade);

	lightAttenuation *= _DirectLightMultiplier;
	
	if (_UseSpecular)
	{
		half specular = pow(max(0, dot(R, V)), _SpecularPow) * _SpecularMul * light.shadowAttenuation;
		lightAttenuation += specular;
	}
	
	return light.color * (lightAttenuation);
}

half3 ShadeAdditionalLight(ToonSurfaceData surfaceData, LightingData2 lightingData, Light light)
{
	half3 N = lightingData.normalWS;
	half3 L = light.direction;
	half3 V = lightingData.viewDirectionWS;
	half3 H = normalize(L + V);

	half NoL = dot(N, L);

	half lightAttenuation = 1;
	
	lightAttenuation *= lerp(1, light.shadowAttenuation, _ReceiveShadowMappingAmount);
	lightAttenuation *= min(4, light.distanceAttenuation); //prevent light over bright if point/spot light too close to vertex

	half celShadeResult = smoothstep(_CelShadeMidPoint - _CelShadeSoftness, _CelShadeMidPoint + _CelShadeSoftness, NoL);

	lightAttenuation *= lerp(celShadeResult, 1, _AdditionalLightIgnoreCelShade);

	return light.color * lightAttenuation;
}

half3 ShadeEmission(ToonSurfaceData surfaceData, LightingData2 lightingData)
{
	half3 emissionResult = lerp(surfaceData.emission, surfaceData.emission * surfaceData.albedo, _EmissionMulByBaseColor); // optional mul albedo
	
	
	half3 N = lightingData.normalWS;
	half3 V = lightingData.viewDirectionWS;
	float rim = 1.0 - saturate(dot(normalize(V), N));
	rim = pow(rim, _RimLightPower);
	emissionResult += _RimLightColor * rim;
	
	return emissionResult;
}

half3 CompositeLight(half3 indirect, half3 mainLight, half3 additionalLight, half3 emission, ToonSurfaceData surfaceData)
{
	return surfaceData.albedo * max(indirect, mainLight + additionalLight) + emission;
}

half3 CompositeLight_PreventOverBright(half3 indirect, half3 mainLight, half3 additionalLight, half3 emission, ToonSurfaceData surfaceData)
{
	half3 rawLightSum = max(indirect, mainLight + additionalLight); // pick the highest between indirect and direct light
	half lightLuminance = Luminance(rawLightSum);
	half3 finalLightMulResult = rawLightSum / max(1, lightLuminance / max(1, log(lightLuminance))); // allow controlled over bright using log
	
	return surfaceData.albedo * finalLightMulResult + emission;
}


#endif