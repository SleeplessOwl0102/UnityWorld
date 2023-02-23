#ifndef REN_CORE
#define REN_CORE
/*-------------------------------------------------------------------
reference: Render Pipeline Core / Common.hlsl

Convention:

	Unity is Y up and left handed in world space
	Caution: When going from world space to view space, unity is right handed in view space and the determinant of the matrix is negative
	For cubemap capture (reflection probe) view space is still left handed (cubemap convention) and the determinant is positive.

	The lighting code assume that 1 Unity unit (1uu) == 1 meters.  This is very important regarding physically based light unit and inverse square attenuation
	space at the end of the variable name
	WS: world space
	RWS: Camera-Relative world space. A space where the translation of the camera have already been substract in order to improve precision
	VS: view space
	OS: object space
	CS: Homogenous clip spaces
	TS: tangent space
	TXS: texture space
	Example: NormalWS

	normalized / unormalized vector
	normalized direction are almost everywhere, we tag unormalized vector with un.
	Example: unL for unormalized light vector

	use capital letter for regular vector, vector are always pointing outward the current pixel position (ready for lighting equation)
	capital letter mean the vector is normalize, unless we put 'un' in front of it.
	V: View vector  (no eye vector)
	L: Light vector
	N: Normal vector
	H: Half vector

--------------------------------------------------------------------*/

//Dependent Catagory
//Dependent on universal render pipelines
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#ifdef REN_LIGHTING
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

float3 GetCameraPosWS()
{
	return _WorldSpaceCameraPos;
}

float4 GetScreenScale()
{
	return _ScaledScreenParams;
}

float4 TransformHClipToScreen(float4 posCS)
{
	//URP Core.hlsl ComputeScreenPos(posCS);
	float4 o = posCS * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = posCS.zw;
	return o;
}

float3 TransformNormalViewToNDC(float3 normalVS, float posCS_W)
{
	return normalize(mul((float3x3) UNITY_MATRIX_P, normalVS.xyz)) * posCS_W;
}

float Gray(float3 col)
{
	return (col.r + col.g + col.b) / 3;
}

float Lambert(float3 normalWS, float3 lightDirWS , float minValue = 0)
{
	return max(0, dot(normalWS, lightDirWS)) * (1 - minValue) + minValue;
}

#endif