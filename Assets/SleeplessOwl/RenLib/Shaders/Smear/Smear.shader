Shader "Custom/Smear"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Position("Position", Vector) = (0, 0, 0, 0)
		_PrevPosition("Prev Position", Vector) = (0, 0, 0, 0)
		_NoiseScale("Noise Scale", range(0.1, 100)) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}

		Pass
		{
			Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "LightMode" = "LightweightForward" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct a2v
			{
				float4 posOS : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			half4 _MainTex_ST;
			half4 _PrevPosition;
			half4 _Position;
			half _NoiseScale;

			half Hash31(half3 p)
			{
				return frac(dot(sin(p), 43758.5453123));
			}

			v2f vert(a2v v)
			{
				v2f o;

				half4 worldPos = mul(unity_ObjectToWorld, v.posOS);
				half4 world = mul(unity_ObjectToWorld, float4(0,0,0,1));
				float3 local = worldPos - world;
				float3 tt = _PrevPosition.xyz - _Position.xyz;
				float3 smearOffset = 0;
				if (dot(local, tt)>0)
				{
					//利用噪声对位移进行扰动，_NoiseScale控制噪声的平滑过渡程度（噪声密度）
					smearOffset = (_PrevPosition - _Position) * Hash31(worldPos * _NoiseScale);
				}
				o.worldPos = worldPos;
				//将位移作用于顶点并最终变换到投影坐标
				worldPos.xyz += smearOffset ;
				o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(1,1,1,1);
			}	
		ENDCG
		}
	}
}