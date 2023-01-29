#ifndef SimpleColor
#define SimpleColor

#define REN_LIGHTING
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Core.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Input.hlsl"
#include "Assets/SleeplessOwl/RenLib/ShaderLib/Util.hlsl"

#define ENABLE_SCREEN_DOOR_TRANSPARENCY 1 

CBUFFER_START(UnityPerMaterial)
half _shadowSmooth;
half _shadowRange;
half3 _shadowColor;

half _OutlineWidth;
half4 _OutLineColor;

float4 _ScreenDoorScale;
float _Alpha;
CBUFFER_END

sampler2D _MainTex;

struct v2f
{
	float4 posCS : SV_POSITION;
	float3 normalWS : NORMAL;
	float3 posWS : TEXCOORD1;
	float2 uv : TEXCOORD2;
	
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	float4 screenPos : TEXCOORD3;
#endif
};
			
v2f Vert(a2v_Full i)
{
	v2f o;
	o.posCS = TransformObjectToHClip(i.posOS);
	o.posWS = TransformObjectToWorld(i.posOS);
	o.normalWS = TransformObjectToWorldNormal(i.normal);
	o.uv = i.uv;
	
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	o.screenPos = ComputeScreenPos(o.posCS);
#endif	
	return o;
}

float4 Frag(v2f i) : SV_Target
{
#if ENABLE_SCREEN_DOOR_TRANSPARENCY	
	ScreenDoorTransparency(_Alpha, i.screenPos,_ScreenDoorScale.xy);
#endif	
	
	Light mainLight = GetMainLight();
	half4 combineColor = tex2D(_MainTex, i.uv);
	half halfLambert = Lambert(i.normalWS, mainLight.direction, .5);
	half ramp = smoothstep(0, _shadowSmooth, halfLambert - _shadowRange);
	combineColor.rgb = lerp(combineColor.rgb * _shadowColor, combineColor.rgb, ramp);

	//combineColor.rgb = Gray(combineColor.rgb);
	//combineColor.rgb *= float3(.3, .3, 1.5) * 2.5;
	
	return combineColor;
}
#endif