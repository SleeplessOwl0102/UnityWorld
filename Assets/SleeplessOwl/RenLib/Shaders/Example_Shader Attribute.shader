Shader "#Ren/Shader Attribute Example"
{
	Properties
	{
		//DataType
		[Header(Base DataType)] 
		_Float("Float", float) = 1
		_Range("Float Range", Range(0, 255)) = 100
		[PowerSlider(3.0)] _PowSlider("Float Range Power Slider", Range(0.01, 10)) = 0.08// A slider with 3.0 response curve
		[IntRange] _Alpha("Int Range", Range(0, 255)) = 100

		_Vector("Vector", vector) = (0,0,0,0)
		_Color("Color", Color) = (1,1,1,1)
		[HDR] _HDRColor("HDR Color", Color) = (1,0,0,1)
		
		//Texture
		[Space(10)]
		[Header(Texture)] 
		_MainTex("Texture", 2D) = "white" {}
		// 2D can use built-in default textures: “white”, “black”, “gray” or “bump”
		_MainTex2("Cube Map", Cube) = "" {}
		_MainTex3("display name", 3D) = "" {}

		[Normal] _MainTex4("Normal Texture", 2D) = "white" {}
		[NoScaleOffset]	_MainTex5("NoScaleOffset Texture", 2D) = "white" {}
		[HideInInspector] _MainTex6("HideInInspector Texture", 2D) = "white" {}
		[PerRendererData] _MainTex7("PerRenderer Texture", 2D) = "white" {}

		//ShowIf Toggle
		[Space(10)]
		[Header(Custom ShowIf Attribute)]
		[Toggle] _Toggle("ShowIf Toggle", Float) = 1
		[ShowIf(_Toggle)] _Prop1("Hello", Float) = 0

		//Keyword
		[Space(10)]
		[Header(Keyword)]
		[Toggle(ENABLE_FANCY)] 
		_Fancy("Keyword ENABLE_FANCY toggle", Float) = 0 // Will set "ENABLE_FANCY" shader keyword when set.
		[KeywordEnum(None, Add, Multiply)] 
		_Overlay("Keyword Enum", Float) = 0//#pragma multi_compile _OVERLAY_NONE, _OVERLAY_ADD, _OVERLAY_MULTIPLY

		//Enum
		[Space(10)]
		[Header(Enum)]
		[Enum(UnityEngine.Rendering.BlendOp)]  _BlendOp("BlendOp", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Float) = 0

		// A subset of blend mode values, just "One" (value 1) and "SrcAlpha" (value 5).
		[Enum(One,1,SrcAlpha,5)] _Blend2("Blend mode subset (only One, SrcAlpha)", Float) = 1

		[Enum(Off, 0, On, 1)]_ZWriteMode("ZWriteMode", float) = 1
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", float) = 2
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTestMode", Float) = 4
		[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("ColorMask", Float) = 15

		[Header(Stencil)]
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8
		[IntRange]_StencilWriteMask("Stencil Write Mask", Range(0,255)) = 255
		[IntRange]_StencilReadMask("Stencil Read Mask", Range(0,255)) = 255
		[IntRange]_Stencil("Stencil ID", Range(0,255)) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("Stencil Pass", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilFail("Stencil Fail", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail("Stencil ZFail", Float) = 0

	}
	SubShader
	{
		Pass
		{
			BlendOp[_BlendOp]
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWriteMode]
			ZTest[_ZTestMode]
			Cull[_CullMode]
			ColorMask[_ColorMask]

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
				Pass[_StencilPass]
				Fail[_StencilFail]
				ZFail[_StencilZFail]
			}
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				// sample the texture
				half4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDHLSL
		}
	}
}
