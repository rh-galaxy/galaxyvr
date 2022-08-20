Shader "Custom/Bloom"
{
	Properties
	{
		_MainTex("", 2D) = "white" {}
        _Radius("Blur Radius", float) = 4.0
        _Step("Step", float) = 1.0
	}

	SubShader
	{
		Pass
		{
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GlowMap;
            float4 _GlowMap_TexelSize;
            float _TexelSizeX;
            float _Radius;
            float _Step;

            /* Do an horizontal gaussian blur sampling the _GlowMap which is the texture with all the glow colors rendered. */
            float4 horizontalBlur(float2 uv)
            {
                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                float blur = _Radius * _GlowMap_TexelSize.x; 

                sum += tex2Dlod(_GlowMap, float4(uv.x - 4.0*blur*_Step, uv.y, 0, 0)) * 0.0162162162;
                sum += tex2Dlod(_GlowMap, float4(uv.x - 3.0*blur*_Step, uv.y, 0, 0)) * 0.0540540541;
                sum += tex2Dlod(_GlowMap, float4(uv.x - 2.0*blur*_Step, uv.y, 0, 0)) * 0.1216216216;
                sum += tex2Dlod(_GlowMap, float4(uv.x - 1.0*blur*_Step, uv.y, 0, 0)) * 0.1945945946;
                
                sum += tex2Dlod(_GlowMap, float4(uv.x, uv.y, 0, 0)) * 0.2270270270;
                
                sum += tex2Dlod(_GlowMap, float4(uv.x + 1.0*blur*_Step, uv.y, 0, 0)) * 0.1945945946;
                sum += tex2Dlod(_GlowMap, float4(uv.x + 2.0*blur*_Step, uv.y, 0, 0)) * 0.1216216216;
                sum += tex2Dlod(_GlowMap, float4(uv.x + 3.0*blur*_Step, uv.y, 0, 0)) * 0.0540540541;
                sum += tex2Dlod(_GlowMap, float4(uv.x + 4.0*blur*_Step, uv.y, 0, 0)) * 0.0162162162;

                return float4(sum.rgb, 1.0);
            }

			float4 frag(v2f_img input) : COLOR
			{
                return horizontalBlur(input.uv);
			}

			ENDCG
		}

		Pass
		{
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _GlowMap_TexelSize;
            float _TexelSizeY;
            float _Radius;
            float _Step;

            /* This tiem complete the gaussian blur by doing a vertical blur. */
            float4 verticalBlur(float2 uv)
            {
                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                float blur = _Radius * _GlowMap_TexelSize.y; 

                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y - 4.0*blur*_Step, 0, 0)) * 0.0162162162;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y - 3.0*blur*_Step, 0, 0)) * 0.0540540541;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y - 2.0*blur*_Step, 0, 0)) * 0.1216216216;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y - 1.0*blur*_Step, 0, 0)) * 0.1945945946;
                
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y, 0, 0)) * 0.2270270270;
                
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y + 1.0*blur*_Step, 0, 0)) * 0.1945945946;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y + 2.0*blur*_Step, 0, 0)) * 0.1216216216;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y + 3.0*blur*_Step, 0, 0)) * 0.0540540541;
                sum += tex2Dlod(_MainTex, float4(uv.x, uv.y + 4.0*blur*_Step, 0, 0)) * 0.0162162162;

                return float4(sum.rgb, 1.0);
            }

			float4 frag(v2f_img input) : COLOR
			{
                return verticalBlur(input.uv);
			}

			ENDCG
		}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            /* Simple passthrough fragment shader */
			float4 frag(v2f_img input) : COLOR
			{
                return tex2D(_MainTex, input.uv);
			}

			ENDCG
		}

        Pass
        {
            Blend One One

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            /* Simple passthrough fragment shader that combines both images */
			float4 frag(v2f_img input) : COLOR
			{
                return tex2D(_MainTex, input.uv);
			}

			ENDCG
		}
	}
}