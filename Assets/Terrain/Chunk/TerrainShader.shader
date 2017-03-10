Shader "Custom/TerrainShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		// Custom Properties
		_Scale ("Scale", Float) = 1.0
		_Materials ("Materials", 2DArray) = "" {}
		_Resolution ("Resolution", Int) = 32
		_Density ("Density", Int) = 4
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		//#pragma surface surf Standard fullforwardshadows
		#pragma target 3.5

		float4 _Color : COLOR;
		UNITY_DECLARE_TEX2DARRAY(_Materials);		
		sampler2D _MainTex;
		float _Glossiness;
		float _Metallic;
		float _Scale;
		int _Resolution;
		int _Density;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
			float2 interpolation;
            int matID1;
			int matID2;
			int matID3;
		};
		void vert (inout appdata_full v, out Input o) {	
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.matID1 = v.color.x;
			o.matID1 = v.color.y;
			o.matID1 = v.color.z;
			o.interpolation.x = texcoord.x;
			o.interpolation.y = texcoord.y;
		} 
		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 wPos = IN.worldPos;
			float3 wNorm = IN.worldNormal;

			int currentTex = 0; //IN.materialID.x;

			float3 blending = float3(abs(wNorm.x), abs(wNorm.y), abs(wNorm.z));
			blending = normalize(max (blending, float3(0.00001, 0.00001, 0.00001))); // Force weights to sum to 1.0
			float b = (blending.x + blending.y + blending.z);
			blending = blending / b;

			float4 xaxis = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(wPos.yz * _Scale, currentTex)); //tex2D(_MainTex, wPos.yz * _Scale);
			float4 yaxis = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(wPos.xz * _Scale, currentTex));//tex2D(_MainTex, wPos.xz * _Scale);
			float4 zaxis = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(wPos.xy * _Scale, currentTex));//tex2D(_MainTex, wPos.xy * _Scale);
			float4 tex = (xaxis * blending.x) + (yaxis * blending.y) + (zaxis * blending.z);
			tex.w = 1;

			fixed4 c = tex * _Color;
			o.Albedo = tex.xyzw;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
