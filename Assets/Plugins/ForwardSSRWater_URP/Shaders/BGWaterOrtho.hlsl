#ifndef BADDOG_WATER_ORTHO
#define BADDOG_WATER_ORTHO

inline float GetOrthoEyeDepth(float rawDepth) 
{
	#if defined(UNITY_REVERSED_Z)
		#if UNITY_REVERSED_Z == 1
			rawDepth = 1.0f - rawDepth;
		#endif
	#endif

	return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
}

#endif

