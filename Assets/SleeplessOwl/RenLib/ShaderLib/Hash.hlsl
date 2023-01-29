#ifndef REN_HASH
#define REN_HASH

half3 Hash33(half3 p)
{
    half3 mod = half3(0.1031, 0.11369, 0.13787);
        
    p = frac(p * mod);
    p += dot(p, p.yxz + 19.19);
        
    return - 1.0 + 2.0 * frac(half3((p.x + p.y) * p.z, (p.x + p.z) * p.y, (p.y + p.z) * p.x));
}
    
half2 Hash22(half2 p)
{
    p = half2(dot(p, half2(127.1, 311.7)),
    dot(p, half2(269.5, 183.3)));
        
    return - 1.0 + 2.0 * frac(sin(p) * 43758.5453123) * 0.8;
}
    
half Hash11(half n)
{
    return frac(sin(n) * 43758.5453123);
}

half Hash21(half2 p)
{
	p = frac(p * half2(123.34, 345.45));
	p += dot(p, p + 34.345);
	return frac(p.x * p.y);
}

// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint Hash_UInt(uint s)
{
	s ^= 2747636419u;
	s *= 2654435769u;
	s ^= s >> 16;
	s *= 2654435769u;
	s ^= s >> 16;
	s *= 2654435769u;
	return s;
}

#endif