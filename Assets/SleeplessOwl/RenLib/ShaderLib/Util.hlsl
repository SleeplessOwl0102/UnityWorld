#ifndef REN_UTIL
#define REN_UTIL

#include "Hash.hlsl"

void ScreenDoorTransparency(float alpha, float4 screenPos, float2 scaleScreenSize = float2(1, 1))
{
	if(alpha >= 1)
		return;
	
	const float4x4 thresholdMatrix =
	{
		1, 9, 3, 11,
		13, 5, 15, 7,
		4, 12, 2, 10,
		16, 8, 14, 6
	};
	
	float2 pixelPos = screenPos.xy * scaleScreenSize;
	float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
	clip(alpha - threshold);
}

half GetAspect()
{
	//将近裁剪面右上角位置的顶点变换到观察空间
	//通过逆投影矩阵（Inverse Projection Matrix）将点转换到观察空间（View space）。
	half4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
	//求得屏幕宽高比
	half aspect = abs(nearUpperRight.y / nearUpperRight.x);
	//return _ScreenParams.y / _ScreenParams.x;
	return aspect;
}

//One liner pseudo random
float nrand(float2 uv)
{
	return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float Random_UInt(uint seed)
{
	return float(Hash_UInt(seed)) / 4294967295.0; // 2^32-1
}

// Uniformaly distributed points on a unit sphere
// http://mathworld.wolfram.com/SpherePointPicking.html
float3 RandomUnitVector_UInt(uint seed)
{
	float PI2 = 6.28318530718;
	float z = 1 - 2 * Random_UInt(seed);
	float xy = sqrt(1.0 - z * z);
	float sn, cs;
	sincos(PI2 * Random_UInt(seed + 1), sn, cs);
	return float3(sn * xy, cs * xy, z);
}

// Euler angles rotation matrix
float3x3 Euler3x3(float3 v)
{
	float sx, cx;
	float sy, cy;
	float sz, cz;

	sincos(v.x, sx, cx);
	sincos(v.y, sy, cy);
	sincos(v.z, sz, cz);

	float3 row1 = float3(sx * sy * sz + cy * cz, sx * sy * cz - cy * sz, cx * sy);
	float3 row3 = float3(sx * cy * sz - sy * cz, sx * cy * cz + sy * sz, cx * cy);
	float3 row2 = float3(cx * sz, cx * cz, -sx);

	return float3x3(row1, row2, row3);
}

// Rotation with angle (in radians) and axis
float3x3 AngleAxis3x3(float angle, float3 axis)
{
	float c, s;
	sincos(angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
    );
}
    
#endif