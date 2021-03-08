/*           INFINITY CODE          */
/*     https://infinity-code.com    */

Shader "Huge Texture/Unlit Array" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2DArray) = "white" {}
		_Cols("Cols", Int) = 1
		_Rows("Rows", Int) = 1
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma require 2darray
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			UNITY_DECLARE_TEX2DARRAY(_MainTex);
			float4 _MainTex_ST;
			half _Cols;
			half _Rows;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float uvx = i.texcoord.x * _Cols;
				float uvy = i.texcoord.y * _Rows;
				float uvz = floor(uvy) * _Cols + floor(uvx);
				uvx = uvx - floor(uvx);
				uvy = uvy - floor(uvy);

				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uvx, uvy, uvz));
				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
			ENDCG
		}
	}
}
