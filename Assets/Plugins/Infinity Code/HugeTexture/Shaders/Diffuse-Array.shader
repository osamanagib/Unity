/*           INFINITY CODE          */
/*     https://infinity-code.com    */

Shader "Huge Texture/Diffuse Array"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2DArray) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Cols("Cols", Int) = 1
		_Rows("Rows", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.5
		#pragma require 2darray

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		half _Cols;
		half _Rows;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float uvx = IN.uv_MainTex.x * _Cols;
			float uvy = IN.uv_MainTex.y * _Rows;
			float uvz = floor(uvy) * _Cols + floor(uvx);
			uvx = uvx - floor(uvx);
			uvy = uvy - floor(uvy);

            fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uvx, uvy, uvz)) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
