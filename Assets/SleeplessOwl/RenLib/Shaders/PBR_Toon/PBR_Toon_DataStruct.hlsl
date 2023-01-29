#ifndef PBR_Toon_DataStruct
#define PBR_Toon_DataStruct

struct Attributes
{
	float3 posOS : POSITION;
	float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float3 normalWS : NORMAL;
	float2 uv : TEXCOORD0;
	float4 positionWSAndFogFactor : TEXCOORD1; // xyz: positionWS, w: vertex fog factor
};

struct ToonSurfaceData
{
	half3 albedo;
	half alpha;
	half3 emission;
};

struct LightingData2
{
	half3 normalWS;
	float3 positionWS;
	half3 viewDirectionWS;
	float4 shadowCoord;
};

ToonSurfaceData InitializeSurfaceData(Varyings input, float4 baseColor)
{
	ToonSurfaceData output;

    // albedo & alpha
	float4 baseColorFinal = tex2D(_MainTex, input.uv) * baseColor;
	output.albedo = baseColorFinal.rgb;
	output.alpha = baseColorFinal.a;
	
    // emission
	output.emission = 0;
	if (_UseEmission)
	{
		output.emission = tex2D(_EmissionMap, input.uv).rgb * _EmissionMapChannelMask * _EmissionColor.rgb;
	}
	
	return output;
}

LightingData2 InitializeLightingData(Varyings input)
{
	LightingData2 lightingData;
	lightingData.positionWS = input.positionWSAndFogFactor.xyz;
	lightingData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - lightingData.positionWS);
	lightingData.normalWS = normalize(input.normalWS); //interpolated normal is NOT unit vector, we need to normalize it

	return lightingData;
}


void ScreenDoorTransparency(float alpha, float4 screenPos, float2 scaleScreenSize = float2(1, 1))
{
	if (alpha >= 1)
		return;
	
	const float4x4 thresholdMatrix =
	{
		1, 9, 3, 11,
		13, 5, 15, 7,
		4, 12, 2, 10,
		16, 8, 14, 6
	};
	
	float2 pixelPos = screenPos.xy * scaleScreenSize;
	float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
	clip(alpha - threshold);
}


#endif