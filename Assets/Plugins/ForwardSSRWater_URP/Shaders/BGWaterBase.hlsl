#ifndef BADDOG_WATER_BASE
#define BADDOG_WATER_BASE

	sampler2D _MainWave;
	float4 _MainWave_ST;

	sampler2D _SecondWave;
	float4 _SecondWave_ST;

	half _MainWaveBumpScale;
	half _SecondWaveBumpScale;

	half4 _MainWaveTilingOffset;
	half4 _SecondWaveTilingOffset;
	half4 _ThirdWaveTilingOffset;

	half _WaterDepthOffset;
	half _WaterMuddyScale;
	half _WaterDistortScale;

	half4 _WaterBaseColor;
	half4 _WaterMuddyColor;

	half _SpecularIntensity;
	half _EnviromentIntensity;

	half _SSRMaxSampleCount;
	half _SSRSampleStep;
	half _SSRIntensity;

	TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
	TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);


	#include "./BGWaterOrtho.hlsl"
	#include "./BGWaterStruct.hlsl"
	#include "./BGWaterSSR.hlsl"
	#include "./BGWaterLighting.hlsl"


	BGWaterVertexOutput VertexCommon(BGWaterVertexInput v)
	{
		BGWaterVertexOutput o = (BGWaterVertexOutput)0;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o); 
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

		o.pos = TransformObjectToHClip(v.vertex);

		o.mainWaveUV.xy = v.texcoord * _MainWaveTilingOffset.xy + _Time.r * _MainWaveTilingOffset.zw * 0.1;
		o.secondWaveUV.xy = v.texcoord * _SecondWaveTilingOffset.xy + _Time.r * _SecondWaveTilingOffset.zw * 0.1;
		o.secondWaveUV.zw = v.texcoord * _ThirdWaveTilingOffset.xy + _Time.r * _ThirdWaveTilingOffset.zw * 0.1;

		float3 worldPos = TransformObjectToWorld(v.vertex);

		VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangent);

		o.worldNormalDir = float4(normalInput.normalWS, worldPos.x);
		o.worldTangentDir = float4(normalInput.tangentWS, worldPos.y);
		o.worldBitangentDir = float4(normalInput.bitangentWS, worldPos.z);

		o.mainWaveUV.z = ComputeFogFactor(o.pos.z);

		o.screenPos = ComputeScreenPos(o.pos);
		o.screenPos.z = -TransformWorldToView(worldPos).z;

		return o;
	}

	BGWaterVertexOutput VertexForward(BGWaterVertexInput vertexInput)
	{
		BGWaterVertexOutput vertexOutput = VertexCommon(vertexInput);
		return vertexOutput;
	}

	half4 FragForward(BGWaterVertexOutput vertexOutput) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput); 
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);  

		BGLightingData lightingData = PrepareLighting(vertexOutput);

		// reflection + fog
		half3 reflection = GetReflectionWithSSR(vertexOutput, lightingData);
		reflection = MixFog(reflection, vertexOutput.mainWaveUV.z);

		// refraction
		half4 refraction = GetRefraction(vertexOutput, lightingData);

		// final
		half3 finalColor = lerp(refraction.rgb, reflection, refraction.a);

		return half4(finalColor, 1);
	}

#endif

