Shader "BadDog/URP/BGWater"
{
	Properties
	{
		[Header(Three Direction Wave)]
		_MainWave("Main Wave", 2D) = "white" {}
		_MainWaveBumpScale("Main Wave Bump Scale", Range(0, 2)) = 1
		_SecondWave("Second Wave", 2D) = "white" {}
		_SecondWaveBumpScale("Second Normal Bump Scale", Range(0, 2)) = 1
		_MainWaveTilingOffset("Main Wave Tiling Offset", Vector) = (1, 1, 1, 1)
		_SecondWaveTilingOffset("Second Wave Tiling Offset", Vector) = (1, 1, -1, 1)
		_ThirdWaveTilingOffset("Third Wave Tiling Offset", Vector) = (1, 1, 1, -1)

		[Header(Water)]
		_WaterBaseColor("Water Base Color", Color) = (1.0, 1.0, 1.0, 1)

		[Header(Muddy)]
		_WaterMuddyColor("Water Muddy Color", Color) = (1.0, 1.0, 1.0, 1)
		_WaterMuddyScale("Water Muddy Scale", Range(0, 2)) = 1
		_WaterDepthOffset("Water Depth Offset", Range(0, 1)) = 1

		[Header(Specular)]
		_SpecularIntensity("Specular Intensity", Range(0, 8)) = 1

		[Header(Refraction)]
		_WaterDistortScale("Distort Scale", Range(0, 10)) = 1

		[Header(Enviroment Reflection)]
		_EnviromentIntensity("Enviroment Intensity", Range(0, 10)) = 1

		[Header(Screen Space Reflection)]
		[Toggle] _BGWATER_SSR("Screen Space Reflection", Float) = 1.0
		_SSRMaxSampleCount("SSR Max Sample Count", Range(0, 64)) = 12
		_SSRSampleStep("SSR Sample Step", Range(4, 32)) = 16
		_SSRIntensity("SSR Intensity", Range(0, 2)) = 0.5

		[Toggle] _BGWATER_ORTHO("Orthographic Camera", Float) = 0
	}

	SubShader
	{
        Tags 
		{
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

			HLSLPROGRAM

			#if !defined(UNITY_PASS_FORWARDBASE)
				#define UNITY_PASS_FORWARDBASE
			#endif

			#pragma vertex VertexForward
			#pragma fragment FragForward

			#pragma target 3.0

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog 
            #pragma multi_compile_instancing
			#pragma multi_compile _ _BGWATER_SSR_ON
			#pragma multi_compile _ _BGWATER_ORTHO_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "./BGWaterBase.hlsl"

			ENDHLSL
		}
	}

	FallBack "VertexLit"
}