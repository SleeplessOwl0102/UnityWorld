Shader "Raymarch/PostFog"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/RenLib/ShaderLib/Core.hlsl"
			#include "Packages/RenLib/ShaderLib/Noise.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 ray : TEXCOORD2;
			};

			float4 _CamWorldSpace;
			float4x4 _InverseView;
			float4x4 _CamFrustum, _WorldToCam;
			sampler2D _PostSource;
			TEXTURE2D(_CameraDepthTexture);

			float2 Raymarch(float3 ro, float3 rd, float depth)
			{
				float3 p = ro;
				float dens = 0;
				float dd = 0;

				//p = p + depth/2 * rd;

				for (int j = 0; j < 110; j++)
				{
					p = p + 0.05 * j * rd;
					dd += 0.05 * j;
					float z = LinearEyeDepth(p, _WorldToCam);

					if (z > depth || dd > depth * 0.3 + 200)
						break;

					float3 s = p;
					float3 s2 = p;
					s.x += _Time.x * 30;
					float noise = ValueNoise_3D(s / 10) * 2;
					s2.x += _Time.x * 200;
					noise += ValueNoise_3D(s2 / 6) * 1;
					noise *= .333;

					float miny = 10 + z / 30;
					float maxy = 15 + z / 3;
					float thick = maxy - miny;
					float curthick = p.y - miny;
					curthick = min(curthick, thick);
					if (p.y > miny)
						noise *= pow(1 - (curthick / thick),3);

					dens += noise * j;
				}

				dens = dens / 2500;

				return float2(0, dens);
			}


			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = GetFullScreenTriangleVertexPosition(vertexID);
				o.uv.xy = GetFullScreenTriangleTexCoord(vertexID);

				//calculate world space ray vector
				float4 clipPos = float4(o.vertex.x, -o.vertex.y, 1, 1);// need negative y, but why???
				float3 viewPos = mul(unity_CameraInvProjection, clipPos);
				o.ray = mul(_InverseView, float4(viewPos, 0));
				return o;
			}


			float4 frag(v2f i) : SV_Target
			{
				//return float4(i.ray.xyz, 1);
				i.ray.xyz = normalize(i.ray.xyz);
				float3 texColor = tex2D(_PostSource, i.uv.xy).rgb;
				float3 color = texColor;

				float depth = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, i.vertex.xy, 0).r;
				float depth01 = Linear01Depth(depth, _ZBufferParams);
				depth = LinearEyeDepth(depth, _ZBufferParams);
				float3 rayDirection = normalize(i.ray);
				float2 result = Raymarch(_CamWorldSpace, rayDirection, depth);
				float ss = result.y;


				if (depth < 5)
					ss *= depth / 5;

				color = color * (1 - ss) + float3(.8,.8,.95) * ss;
				//depth /= 1000;

				return float4(color, 1);
			}
			ENDHLSL
		}
	}
}