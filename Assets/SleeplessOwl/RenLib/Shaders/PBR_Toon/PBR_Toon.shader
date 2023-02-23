Shader "#RenLib/NRP/PBR_Toon"
{
	Properties
	{
		[Header(TestOnly)]
		//no use just for easy to change URP default shader
		_BaseMap("Diffuse", 2D) = "white" { }
		[Toggle(DISALBLE_TOONSHADER)]
		_DisableToonShader("Disable Toon Shader", float) = 0

		[Header(MainColor)]
		_MainTex("Diffuse", 2D) = "white" { }
		_BaseColor("BaseColor", Color) = (1, 1, 1, 1)

		[Toggle(USE_FAKE_CONE_NORMAL)]
		_UseFakeConeNormal("Use Fake Cone Normal", float) = 0
		_FakeNormalOffset("Fake Normal Offset", Vector) = (0,0,0)

		[Header(Eye)]
		_EyeJitterMul("Eye Jitter Mul", Range(0, 0.001)) = 0

		//Shadow
		[Header(Receive Shadow)]
		_ReceiveShadowMappingAmount("ReceiveShadowMappingAmount", Range(0,1)) = 0.75
		_ReceiveShadowMappingPosOffset("ReceiveShadowMappingPosOffset (increase it if is face!)", Float) = 0


		//Lighting
		[Header(Lighting)]
		_IndirectLightMinColor("IndirectLightMinColor", Color) = (0.1,0.1,0.1,1) // can prevent completely black if lightprobe not baked
		_IndirectLightMul("IndirectLightMul", Range(0,1)) = 1
		_DirectLightMultiplier("DirectLightMultiplier", Range(0,1)) = 0.25
		_CelShadeMidPoint("CelShadeMidPoint", Range(-1,1)) = -.5
		_CelShadeSoftness("CelShadeSoftness", Range(0,1)) = 0.05
		_MainLightIgnoreCelShade("MainLightIgnoreCelShade", Range(0,1)) = 0
		_AdditionalLightIgnoreCelShade("AdditionalLightIgnoreCelShade", Range(0,1)) = 0.9

		//Specular
		[Header(Direct Light Specular)]
		[Toggle]_UseSpecular("Use Specular", Float) = 0
		_SpecularPow("SpecularPow", Range(1,300)) = 32
		_SpecularMul("SpecularMul", Range(1,400)) = 200


		//Emission
		[Header(Emission)]
		[Toggle]_UseEmission("UseEmission (on/off Emission completely)", Float) = 0
		[HDR] _EmissionColor("EmissionColor", Color) = (0,0,0)
		_EmissionMulByBaseColor("EmissionMulByBaseColor", Range(0,1)) = 0
		[NoScaleOffset]_EmissionMap("EmissionMap", 2D) = "white" {}
		_EmissionMapChannelMask("EmissionMapChannelMask", Vector) = (1,1,1,0)


		//RimLight
		[Header(RimLight)]
		_RimLightPower("RimPower", Range(1,20)) = 1
		[HDR] _RimLightColor("RimLightColor", Color) = (0,0,0)


		//Outline
		[Header(Outline)]
		_OutlineWidth("OutlineWidth", Float) = 2.0
		_OutLineColor("OutLineColor", Color) = (1, 1, 1, 1)
		_OutlineStartScaleDistance("OutlineStartScaleDistance", Float) = 1.5
		[Toggle(ENABLE_NOISE_OUTLINE_WIDTH)]
		ENABLE_NOISE_OUTLINE_WIDTH("Enable Noise Outline", float) = 0

		[Toggle(ENABLE_AVERAGE_NORMAL)]
		_Average_Normal("Use average normal", Float) = 1


		//ScreenDoor
		[Header(ScreenDoor Transparency)]
		[Toggle(UseScreenDoor)]
		_UseScreenDoor("Enable Screen Door Transparency", Float) = 0

		[ShowIf(_UseScreenDoor)]
		_ScreenDoorScale("ScreenDoorScale",Vector) = (1,1,0,0)

		[ShowIf(_UseScreenDoor)]
		_Alpha("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalForward"
		}

		Pass
		{
			Name "SimpleColor"
			Tags 
			{
				"LightMode" = "UniversalForward"
				"RenderType" = "Opaque" 
			}

			ZWrite On
			Cull Back
			ZTest LEqual
			Blend Off

			HLSLPROGRAM
			#pragma target 4.5


			#pragma multi_compile_local __ USE_FAKE_CONE_NORMAL
			#pragma shader_feature_local DISALBLE_TOONSHADER

			// -------------------------------------
			// Universal Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ /*_ADDITIONAL_LIGHTS_VERTEX*/ _ADDITIONAL_LIGHTS
			//#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog
			// -------------------------------------

			#include "PBR_Toon_Diffuse.hlsl"
			#pragma vertex Vert
			#pragma fragment Frag
			
			ENDHLSL
		}
	
		Pass
		{
			Name "Outline"
			Tags
			{
				"LightMode" = "Outline"
				"RenderType" = "Opaque"
			}

			Cull Front
			ZTest LEqual

			HLSLPROGRAM

			#pragma shader_feature_local __ ENABLE_NOISE_OUTLINE_WIDTH
			#pragma shader_feature_local ENABLE_AVERAGE_NORMAL
			#pragma shader_feature_local DISALBLE_TOONSHADER

			#include "PBR_Toon_Outline.hlsl"
			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			Cull Back

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#include "PBR_Toon_Diffuse.hlsl"
			#pragma vertex Vert_Shadow
			#pragma fragment Frag_Shadow

			/*#include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"*/
			ENDHLSL
		}
		
		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 4.5

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "PBR_Toon_Diffuse.hlsl"

			#pragma vertex Vert_Depth
			#pragma fragment Frag_Depth

			ENDHLSL
		}
	}
}
