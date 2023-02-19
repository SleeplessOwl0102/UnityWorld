#ifndef BADDOG_WATER_LIGHTING
#define BADDOG_WATER_LIGHTING

	float3 WaveNormal(BGWaterVertexOutput vertexOutput)
	{
		half3 waterNormal1 = tex2D(_MainWave, vertexOutput.mainWaveUV.xy).xyz;
		half3 waterNormal2 = tex2D(_MainWave, vertexOutput.secondWaveUV.xy).xyz;
		half3 waterNormal3 = tex2D(_SecondWave, vertexOutput.secondWaveUV.zw).xyz;

		half3 waterNormal = ((waterNormal1 + waterNormal2) * 0.6667 - 0.6667) * half3(_SecondWaveBumpScale, _SecondWaveBumpScale, 1);

		waterNormal3 = waterNormal3 * 2 - 1;
		waterNormal += (waterNormal3 * half3(_MainWaveBumpScale, _MainWaveBumpScale, 1));

		return normalize(TransformTangentToWorld(waterNormal, float3x3(vertexOutput.worldTangentDir.xyz, vertexOutput.worldBitangentDir.xyz, vertexOutput.worldNormalDir.xyz)));
	}

	BGLightingData PrepareLighting(BGWaterVertexOutput vertexOutput)
	{
		BGLightingData lightingData = (BGLightingData)0;

		lightingData.worldPos = float3(vertexOutput.worldNormalDir.w, vertexOutput.worldTangentDir.w, vertexOutput.worldBitangentDir.w);
		lightingData.worldNormal = WaveNormal(vertexOutput);
		lightingData.worldLightDir = normalize(_MainLightPosition.xyz);


		#if defined(_BGWATER_ORTHO_ON)
			lightingData.worldViewDir = normalize(UNITY_MATRIX_V[2].xyz);
		#else
			lightingData.worldViewDir = normalize(_WorldSpaceCameraPos.xyz - lightingData.worldPos);
		#endif

		half3 H = normalize(lightingData.worldLightDir + lightingData.worldViewDir);

		lightingData.NoL = saturate(dot(lightingData.worldLightDir, lightingData.worldNormal));
		lightingData.NoV = saturate(dot(lightingData.worldNormal, lightingData.worldViewDir));
		lightingData.NoH = saturate(dot(lightingData.worldNormal, H));
		lightingData.LoH = saturate(dot(lightingData.worldLightDir, H));
		lightingData.R = normalize(reflect(-lightingData.worldViewDir, lightingData.worldNormal));

		lightingData.diffuseColor = _WaterBaseColor;
		lightingData.specularColor = half3(0.04, 0.04, 0.04);
		lightingData.lightColor = _MainLightColor.rgb;

		lightingData.screenUV = vertexOutput.screenPos.xy / vertexOutput.screenPos.w;

		#if defined(UNITY_SINGLE_PASS_STEREO)
			lightingData.screenUV.xy = UnityStereoTransformScreenSpaceTex(lightingData.screenUV.xy);
		#endif

		return lightingData;
	}

	half3 IndirectDiffuse(BGLightingData lightingData)
	{
        return SampleSH(lightingData.worldNormal); 
	}

	half3 Diffuse(BGLightingData lightingData)
	{
		return lightingData.lightColor * lightingData.NoL;
	}

	half3 Specular(BGLightingData lightingData)
	{
		float D = (-0.004) / (lightingData.NoH * lightingData.NoH - 1.005);
		D *= D;

		half x = 1 - lightingData.LoH;
		half x2 = x * x;
		half x5 = x2 * x2 * x;

		float F = lightingData.specularColor + (1 - lightingData.specularColor) * x5;

		return lightingData.lightColor * D * F * PI * _SpecularIntensity;
	}

	half3 IndirectSpecular(BGLightingData lightingData)
	{
		half3 probe = GlossyEnvironmentReflection(lightingData.R, 0, 1);

		half fresnelTerm = 1.0 - saturate(dot(lightingData.worldNormal, lightingData.worldViewDir));
		fresnelTerm *= fresnelTerm;
		fresnelTerm *= fresnelTerm;

		return probe.rgb * lerp(lightingData.specularColor, 1, fresnelTerm);
	}

	half4 GetSSRLighting(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		#if defined(_BGWATER_ORTHO_ON)
			float3 uvz = GetSSRUVZOrtho(vertexOutput, lightingData);
		#else
			float3 uvz = GetSSRUVZ(vertexOutput, lightingData);
		#endif

		half3 ssrColor = lerp(half3(0, 0, 0), SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvz.xy) * _SSRIntensity, uvz.z > 0);

		return half4(ssrColor, uvz.z);
	}

	half3 GetReflectionWithSSR(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		half3 indirectDiffuse = IndirectDiffuse(lightingData);
		half3 diffuse = Diffuse(lightingData);
		half3 specular = Specular(lightingData);
		half3 indirectSpecular = IndirectSpecular(lightingData);

#if defined(_BGWATER_SSR_ON)
		half4 ssrLighting = GetSSRLighting(vertexOutput, lightingData);
		indirectSpecular = lerp(lerp(indirectSpecular, ssrLighting.rgb, ssrLighting.a), ssrLighting, ssrLighting.a > 0.99);
#endif

		indirectSpecular *= _EnviromentIntensity;

		return (indirectDiffuse + diffuse) * lightingData.diffuseColor + specular + indirectSpecular;
	}

	half4 GetRefraction(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		float2 screenUV = lightingData.screenUV;
		float2 grabUV = screenUV;

		half3 worldViewDir = normalize(lightingData.worldViewDir);
		half worldViewDirY = abs(worldViewDir.y);

		#if defined(_BGWATER_ORTHO_ON)
			float depth = GetOrthoEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV));
		#else
			float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV), _ZBufferParams);
		#endif
		depth = depth - vertexOutput.screenPos.z;

		half2 deltaUV = lightingData.worldNormal.xz * _WaterDistortScale * saturate(depth) * worldViewDirY / vertexOutput.screenPos.z;

		#if defined(_BGWATER_ORTHO_ON)
			float newDepth = GetOrthoEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + deltaUV));
		#else
			float newDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + deltaUV), _ZBufferParams);
		#endif
		newDepth = newDepth - vertexOutput.screenPos.z;

		half signDepth = saturate(newDepth * 10);
		grabUV = grabUV + deltaUV * signDepth;

		depth = lerp(depth, newDepth, signDepth);

		half viewMultiplier = (worldViewDirY + _WaterMuddyScale) * _WaterDepthOffset * _WaterDepthOffset;
		depth *= viewMultiplier;

		half alpha = saturate(1 - depth);
		alpha = saturate(1.02 - pow(alpha, (dot(lightingData.worldNormal.xyz, worldViewDir) * 5 + 6)));

		half4 refraction = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUV);
		refraction.rgb = lerp(refraction.rgb, refraction.rgb * _WaterMuddyColor * _WaterMuddyScale, alpha);
		refraction.a = alpha;

		return refraction;
	}

#endif

