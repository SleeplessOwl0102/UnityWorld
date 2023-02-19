#ifndef BADDOG_WATER_SSR
#define BADDOG_WATER_SSR

	float UVJitter(in float2 uv)
	{
		return frac((52.9829189 * frac(dot(uv, float2(0.06711056, 0.00583715)))));
	}

	void SSRRayConvert(float3 worldPos, out float4 clipPos, out float3 screenPos)
	{
		clipPos = TransformWorldToHClip(worldPos);
		float k = ((1.0) / (clipPos.w));

		screenPos.xy = ComputeScreenPos(clipPos).xy * k;

		#if defined(_BGWATER_ORTHO_ON)
			screenPos.z = GetOrthoEyeDepth(clipPos.z);
			clipPos.w = screenPos.z;
		#else
			screenPos.z = k;
		#endif

		#if defined(UNITY_SINGLE_PASS_STEREO)
			screenPos.xy = UnityStereoTransformScreenSpaceTex(screenPos.xy);
		#endif
	}

	float3 SSRRayMarch(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		float4 startClipPos;
		float3 startScreenPos;

		SSRRayConvert(lightingData.worldPos, startClipPos, startScreenPos);

		float4 endClipPos;
		float3 endScreenPos;

		SSRRayConvert(lightingData.worldPos + lightingData.R, endClipPos, endScreenPos);

		if (((endClipPos.w) < (startClipPos.w)))
		{
			return float3(0, 0, 0);
		}

		float3 screenDir = endScreenPos - startScreenPos;

		float screenDirX = abs(screenDir.x);
		float screenDirY = abs(screenDir.y);

		float dirMultiplier = lerp( 1 / (_ScreenParams.y * screenDirY), 1 / (_ScreenParams.x * screenDirX), screenDirX > screenDirY ) * _SSRSampleStep;

		screenDir *= dirMultiplier;

		half lastRayDepth = startClipPos.w;

		half sampleCount = 1 + UVJitter(vertexOutput.pos) * 0.1;

		float3 lastScreenMarchUVZ = startScreenPos;
		float lastDeltaDepth = 0;

#if defined (SHADER_API_OPENGL) || defined (SHADER_API_D3D11) || defined (SHADER_API_D3D12)
		[unroll(64)]
#else
		UNITY_LOOP
#endif
		for(int i = 0; i < _SSRMaxSampleCount; i++)
		{
			float3 screenMarchUVZ = startScreenPos + screenDir * sampleCount;

			if((screenMarchUVZ.x <= 0) || (screenMarchUVZ.x >= 1) || (screenMarchUVZ.y <= 0) || (screenMarchUVZ.y >= 1))
			{
				break;
			}

			#if defined(_BGWATER_ORTHO_ON)
				float sceneDepth = GetOrthoEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenMarchUVZ.xy));
			#else
				float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenMarchUVZ.xy), _ZBufferParams);
			#endif
			#if defined(_BGWATER_ORTHO_ON)
				half rayDepth = screenMarchUVZ.z;
			#else
				half rayDepth = 1.0 / screenMarchUVZ.z;
			#endif
			half deltaDepth = rayDepth - sceneDepth;

			if((deltaDepth > 0) && (sceneDepth > startClipPos.w) && (deltaDepth < (abs(rayDepth - lastRayDepth) * 2)))
			{
				float samplePercent = saturate(lastDeltaDepth / (lastDeltaDepth - deltaDepth));
				samplePercent = lerp(samplePercent, 1, rayDepth >= _ProjectionParams.z);
				float3 hitScreenUVZ = lerp(lastScreenMarchUVZ, screenMarchUVZ, samplePercent);
				return float3(hitScreenUVZ.xy, 1);
			}

			lastRayDepth = rayDepth;
			sampleCount += 1;

			lastScreenMarchUVZ = screenMarchUVZ;
			lastDeltaDepth = deltaDepth;
		}

		float4 farClipPos;
		float3 farScreenPos;

		SSRRayConvert(lightingData.worldPos + lightingData.R * 100000, farClipPos, farScreenPos);

		if((farScreenPos.x > 0) && (farScreenPos.x < 1) && (farScreenPos.y > 0) && (farScreenPos.y < 1))
		{
			#if defined(_BGWATER_ORTHO_ON)
				float farDepth = GetOrthoEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, farScreenPos.xy));
			#else
				float farDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, farScreenPos.xy), _ZBufferParams);
			#endif

			if(farDepth > startClipPos.w)
			{
				return float3(farScreenPos.xy, 1);
			}
		}

		return float3(0, 0, 0);
	}

	float3 GetSSRUVZ(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		#if defined(UNITY_SINGLE_PASS_STEREO)
			half ssrWeight = 1;

			half NoV = lightingData.NoV * 2;
			ssrWeight *= (1 - NoV * NoV);
		#else
			float screenUV = lightingData.screenUV * 2 - 1;
			screenUV *= screenUV;

			half ssrWeight = saturate(1 - dot(screenUV, screenUV));

			half NoV = lightingData.NoV * 2.5;
			ssrWeight *= (1 - NoV * NoV);
		#endif

		if (ssrWeight > 0.005)
		{
			float3 uvz = SSRRayMarch(vertexOutput, lightingData);
			uvz.z *= ssrWeight;
			return uvz;
		}

		return float3(0, 0, 0);
	}

	float3 GetSSRUVZOrtho(BGWaterVertexOutput vertexOutput, BGLightingData lightingData)
	{
		float3 uvz = SSRRayMarch(vertexOutput, lightingData);
		return uvz;
	}

#endif

