Shader "#RenLib/NRP/Outline"
{
	Properties
	{
		_MainTex("Diffuse", 2D) = "white" { }

		[Header(Outline)]
		_OutlineWidth("OutlineWidth", Float) = 2.0
		_OutLineColor("OutLineColor", Color) = (1, 1, 1, 1)
		[Toggle(ENABLE_AVERAGE_NORMAL)]
		_Average_Normal("Use average normal", Float) = 1

		[Header(Shadow)]
		_shadowSmooth("_shadowSmooth", Range(0, 1)) = 0.1
		_shadowRange("_shadowRange", Range(0, 1)) = 0.5
		_shadowColor("_shadowColor", Color) = (0.5, 0.5, 0.5, 1)

		//ScreenDoor
		[Header(ScreenDoor Transparency)]
		[Toggle(ENABLE_SCREEN_DOOR_TRANSPARENCY)] 
		_ScreenDoorTransparency("Enable Screen Door Transparency", Float) = 0

		[ShowIf(_ScreenDoorTransparency)]
		_ScreenDoorScale("ScreenDoorScale",Vector) = (1,1,0,0)

		[ShowIf(_ScreenDoorTransparency)]
		_Alpha("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline" = "LightweightPipeline"
		}

		Pass
		{
			Name "SimpleColor"
			Tags 
			{
				"LightMode" = "LightweightForward"
				"RenderType" = "Opaque" 
			}

			ZWrite On
			Cull Back
			ZTest LEqual
			Blend Off

			HLSLPROGRAM
			#pragma target 4.5

			#pragma multi_compile_local __ ENABLE_SCREEN_DOOR_TRANSPARENCY
			
			#include "SimpleColor.hlsl"
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

			#pragma shader_feature_local ENABLE_AVERAGE_NORMAL
			#pragma multi_compile_local __ ENABLE_SCREEN_DOOR_TRANSPARENCY

			#include "Outline.hlsl"
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
			Cull[_Cull]

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

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
			ENDHLSL
		}

		//Pass
		//{
		//	Name "DepthOnly"
		//	Tags{"LightMode" = "DepthOnly"}

		//	ZWrite On
		//	ColorMask 0
		//	Cull[_Cull]

		//	HLSLPROGRAM
		//	#pragma exclude_renderers gles gles3 glcore
		//	#pragma target 4.5

		//	#pragma vertex DepthOnlyVertex
		//	#pragma fragment DepthOnlyFragment

		//	// -------------------------------------
		//	// Material Keywords
		//	#pragma shader_feature_local_fragment _ALPHATEST_ON
		//	#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

		//	//--------------------------------------
		//	// GPU Instancing
		//	#pragma multi_compile_instancing
		//	#pragma multi_compile _ DOTS_INSTANCING_ON

		//	#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
		//	#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
		//	ENDHLSL
		//}
	}
}
