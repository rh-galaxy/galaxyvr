Shader "Custom/Planet_Clouds" 
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_AlphaScale("Alpha Scale", Range(0.0, 2.0)) = 1.0
		[NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Range(0.0, 1.0)) = 1.0
        _DisplacementClouds ("Clouds displacement", Range(0, .1)) = 0.1
	}
	CGINCLUDE
	#define _GLOSSYENV 1
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	ENDCG

	SubShader
		{
			Tags
			{ 
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}
			LOD 300
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			
				CGPROGRAM
				#pragma target 3.0
				#include "UnityPBSLighting.cginc"
				//#pragma surface surf Standard
				#pragma surface surf Standard fullforwardshadows alpha vertex:vert
				//#pragma surface surf Lambert
				//#define UNITY_PASS_FORWARDBASE
				sampler2D _MainTex;
				sampler2D _BumpMap;
				uniform float _BumpScale;
				uniform float _AlphaScale;
	            uniform float _DisplacementClouds;

				struct Input 
				{
					float2 uv_MainTex;
				};

				void vert (inout appdata_full v) 
				{
					v.vertex.xyz += v.normal * _DisplacementClouds;
				}

				void surf(Input IN, inout SurfaceOutputStandard o) 
				{
					float4 albedotex = tex2D(_MainTex, IN.uv_MainTex);
					float4 normaltex = tex2D(_BumpMap, IN.uv_MainTex);

					o.Albedo = albedotex.rgb;
					o.Alpha = saturate(albedotex.a * _AlphaScale);
					o.Normal = normalize(UnpackScaleNormal(normaltex, _BumpScale));
					o.Metallic = 0.0f;
					o.Smoothness = 0.0f;
				}
				ENDCG
	}
	FallBack "Diffuse"
}
