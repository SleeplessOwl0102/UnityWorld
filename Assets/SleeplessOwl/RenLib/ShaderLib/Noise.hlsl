#ifndef REN_NOISE
#define REN_NOISE
    
#include "Hash.hlsl"
half2 Fade2(half2 t)
{
	return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}
    
half3 Fade2(half3 t)
{
	return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}
    
half3 Fade1(half3 t)
{
	return t * t * (3 - 2 * t);
}
    
half ValueNoise_3D(half3 p)
{
	half3 pi = floor(p);
	half3 pf = p - pi;
        
	half3 t = Fade2(pf);
        
	half3 vec = half3(110, 241, 171);
	half n = dot(pi, vec);
        
	half g1 = Hash11(n + dot(vec, half3(0, 0, 0)));
	half g2 = Hash11(n + dot(vec, half3(1, 0, 0)));
	half g3 = Hash11(n + dot(vec, half3(0, 1, 0)));
	half g4 = Hash11(n + dot(vec, half3(1, 1, 0)));
        
	half g5 = Hash11(n + dot(vec, half3(0, 0, 1)));
	half g6 = Hash11(n + dot(vec, half3(1, 0, 1)));
	half g7 = Hash11(n + dot(vec, half3(0, 1, 1)));
	half g8 = Hash11(n + dot(vec, half3(1, 1, 1)));
        
	half x1 = lerp(g1, g2, t.x);
	half x2 = lerp(g3, g4, t.x);
        
	half x3 = lerp(g5, g6, t.x);
	half x4 = lerp(g7, g8, t.x);
        
	half y1 = lerp(x1, x2, t.y);
	half y2 = lerp(x3, x4, t.y);
        
	return lerp(y1, y2, t.z);
}

half SimplexNoise_3D(half3 p)
{
	half tetrahedraToCube = 0.333333333;
	half cubeToTetrahedra = 0.166666667;
        
	half skew = (p.x + p.y + p.z) * tetrahedraToCube;
	half3 i = floor(p + skew);
	half unskew = (i.x + i.y + i.z) * cubeToTetrahedra;
	half3 d0 = p - i + unskew;
        
	half3 e = step(half3(0, 0, 0), d0 - d0.yzx);
	half3 i1 = e * (1.0 - e.zxy);
	half3 i2 = 1.0 - e.zxy * (1.0 - e);
        
	half3 d1 = d0 - (i1 - 1.0 * cubeToTetrahedra);
	half3 d2 = d0 - (i2 - 2.0 * cubeToTetrahedra);
	half3 d3 = d0 - (1.0 - 3.0 * cubeToTetrahedra);
        
	half4 h = max(0.6 /*0.6 or 0.5*/ - half4(dot(d0, d0), dot(d1, d1), dot(d2, d2), dot(d3, d3)), 0.0);
	half4 n = h * h * h * h * half4(dot(d0, Hash33(i)), dot(d1, Hash33(i + i1)), dot(d2, Hash33(i + i2)), dot(d3, Hash33(i + 1.0)));
        
	return dot(half4(31.316, 31.316, 31.316, 31.316), n);
}

half PerlinNoise_3D(half3 p)
{
	half3 pi = floor(p);
	half3 pf = p - pi;
	half3 t = Fade2(pf);
        
	half3 p1 = half3(0, 0, 0);
	half3 p2 = half3(1, 0, 0);
	half3 p3 = half3(0, 1, 0);
	half3 p4 = half3(1, 1, 0);
	half3 p5 = half3(0, 0, 1);
	half3 p6 = half3(1, 0, 1);
	half3 p7 = half3(0, 1, 1);
	half3 p8 = half3(1, 1, 1);
        
	half g1 = dot(Hash33(pi + p1), pf - p1);
	half g2 = dot(Hash33(pi + p2), pf - p2);
	half g3 = dot(Hash33(pi + p3), pf - p3);
	half g4 = dot(Hash33(pi + p4), pf - p4);
        
	half g5 = dot(Hash33(pi + p5), pf - p5);
	half g6 = dot(Hash33(pi + p6), pf - p6);
	half g7 = dot(Hash33(pi + p7), pf - p7);
	half g8 = dot(Hash33(pi + p8), pf - p8);
        
	half x1 = lerp(g1, g2, t.x);
	half x2 = lerp(g3, g4, t.x);

	half x3 = lerp(g5, g6, t.x);
	half x4 = lerp(g7, g8, t.x);
        
	half y1 = lerp(x1, x2, t.y);
	half y2 = lerp(x3, x4, t.y);
        
	return lerp(y1, y2, t.z) + 0.5;
}
	
half PerlinNoise_2D(half2 p)
{
	half2 pi = floor(p);
	half2 pf = p - pi;
	half2 t = Fade2(pf);
        
	half2 p1 = half2(0, 0);
	half2 p2 = half2(1, 0);
	half2 p3 = half2(0, 1);
	half2 p4 = half2(1, 1);
        
	half g1 = dot(Hash22(pi + p1), pf - p1);
	half g2 = dot(Hash22(pi + p2), pf - p2);
	half g3 = dot(Hash22(pi + p3), pf - p3);
	half g4 = dot(Hash22(pi + p4), pf - p4);
        
	half x1 = lerp(g1, g2, t.x);
	half x2 = lerp(g3, g4, t.x);
        
	half y1 = lerp(x1, x2, t.y);
        
	return y1 + 0.5;
}

float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
{
	//make value smaller to avoid artefacts
	float3 smallValue = cos(value);
	//get scalar value from 3d vector
	float random = dot(smallValue, dotDir);
	//make value more random by making it bigger and then taking the factional part
	random = frac(sin(random) * 143758.5453);
	return random;
}

float3 rand3dTo3d(float3 value)
{
	return float3(
		rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
		rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
		rand3dTo1d(value, float3(73.156, 52.235, 09.151))
	);
}

float3 voronoiNoise(float3 value)
{
	float3 baseCell = floor(value);

			//first pass to find the closest cell
	float minDistToCell = 10;
	float3 toClosestCell;
	float3 closestCell;
			[unroll]
	for (int x1 = -1; x1 <= 1; x1++)
	{
				[unroll]
		for (int y1 = -1; y1 <= 1; y1++)
		{
					[unroll]
			for (int z1 = -1; z1 <= 1; z1++)
			{
				float3 cell = baseCell + float3(x1, y1, z1);
				float3 cellPosition = cell + rand3dTo3d(cell);
				float3 toCell = cellPosition - value;
				float distToCell = length(toCell);
				if (distToCell < minDistToCell)
				{
					minDistToCell = distToCell;
					closestCell = cell;
					toClosestCell = toCell;
				}
			}
		}
	}

			//second pass to find the distance to the closest edge
	float minEdgeDistance = 10;
			[unroll]
	for (int x2 = -1; x2 <= 1; x2++)
	{
				[unroll]
		for (int y2 = -1; y2 <= 1; y2++)
		{
					[unroll]
			for (int z2 = -1; z2 <= 1; z2++)
			{
				float3 cell = baseCell + float3(x2, y2, z2);
				float3 cellPosition = cell + rand3dTo3d(cell);
				float3 toCell = cellPosition - value;

				float3 diffToClosestCell = abs(closestCell - cell);
				bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
				if (!isClosestCell)
				{
					float3 toCenter = (toClosestCell + toCell) * 0.5;
					float3 cellDifference = normalize(toCell - toClosestCell);
					float edgeDistance = dot(toCenter, cellDifference);
					minEdgeDistance = min(minEdgeDistance, edgeDistance);
				}
			}
		}
	}

	float random = rand3dTo1d(closestCell);
	return float3(minDistToCell, random, minEdgeDistance);
}

//TODO Worley Noise
#endif