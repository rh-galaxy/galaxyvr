Shader "Custom/Emission"
{
	Properties
	{
		_BloomColor("Bloom Color", Color) = (1, 0, 0, 1)
		_Strength("Strength", Float) = 1.0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
            sampler2D_float _CameraDepthTexture;
			float4 _BloomColor;
			float _Strength;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 texCoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float3 texCoord : TEXCOORD0;
                float linearDepth : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
                output.texCoord = input.texCoord;
                
                output.screenPos = ComputeScreenPos(output.pos);
                output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);

                return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				/* Draw the object shape but with the glow color and taking into account for the depth */
                float4 color = float4(0, 0, 0, 0);
				float2 uv = input.screenPos.xy / input.screenPos.w;
				float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				camDepth = Linear01Depth (camDepth);
                float diff = saturate(input.linearDepth - camDepth);
                if (diff < 0.001)
                    color = _BloomColor * _Strength;
                return color;
			}

			ENDCG
		}
    }
}