Shader "Custom/TerrainShader" {
	Properties {
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
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
		#pragma target 4.0

		#pragma surface surf Standard fullforwardshadows vertex:vert
		//#pragma surface surf Standard fullforwardshadows

		float4 _Color : COLOR;
		UNITY_DECLARE_TEX2DARRAY(_Materials);		
		sampler2D _MainTex;
		float _Glossiness;
		float _Metallic;
		float _Scale;
		int _Resolution;
		int _Density;

		struct vertexdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input {
			float3 worldPos;
			float3 worldNormal;
			float intensity;

			float matID1; // can't use int3s; they give errors
			float matID2;
		};
		void vert (inout vertexdata v, out Input o) {	
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.worldPos = v.vertex;
			o.worldNormal = v.normal;
			o.intensity = v.texcoord.z;
			o.matID1 = v.texcoord.x;
			o.matID2 = v.texcoord.y;
		}

		float3 blend(float4 texture1, float a1, float4 texture2, float a2)
		{
			float depth = 0.2;
			float ma = max(texture1.a + a1, texture2.a + a2) - depth;

			float b1 = max(texture1.a + a1 - ma, 0);
			float b2 = max(texture2.a + a2 - ma, 0);

			return (texture1.rgb * b1 + texture2.rgb * b2) / (b1 + b2);
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 wPos = IN.worldPos;
			float3 wNorm = IN.worldNormal;

			float2 texCoords;
			float3 blending = float3(abs(wNorm.x), abs(wNorm.y), abs(wNorm.z));
			if(blending.y > blending.z && blending.y > blending.x) { // y axis
				texCoords = wPos.xz;
			}
		    if(blending.z > blending.x && blending.z > blending.y) { // z axis
				texCoords = wPos.xy;
			}
 
			texCoords = texCoords * _Scale;

			float4 topColor = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(texCoords, IN.matID1));
			float4 botColor = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(texCoords, IN.matID2));
			//botColor = UNITY_SAMPLE_TEX2DARRAY(_Materials, float3(texCoords, 1));

			float4 finalColor = (topColor * (1 - IN.intensity)) + (botColor * IN.intensity);

			//float3 finalColor = blend(topColor, IN.intensity, botColor, 1.0 - IN.intensity);

			fixed4 c = finalColor;//fixed4(finalColor, 1);
			o.Albedo = c.xyz; 
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
