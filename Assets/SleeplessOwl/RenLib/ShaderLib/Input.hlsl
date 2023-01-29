#ifndef REN_INPUT
#define REN_INPUT

struct a2v_Full
{
	float3 posOS : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

struct v2f_Full
{
	//TODO
	float4 posCS : SV_POSITION;
	float3 posWS : TEXCOORD1;
};

struct a2v_Base
{
	float3 posOS : POSITION;
	half4 color : COLOR;
	float2 uv : TEXCOORD0;
};

#endif