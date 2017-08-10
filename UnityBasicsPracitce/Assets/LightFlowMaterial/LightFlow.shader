Shader "Custom/LightFlow" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white"{}
		_LightTex("Light Texture", 2D) = "white"{}
		_MaskTex("Mask Texture", 2D) = "white"{}
		_Thickness("Light Thickness", float) = 1
		_Lightness("Flow Lightness", float) = 1
		_Speed("Flow Speed", float) = 0.5
	}

	SubShader 
	{
		//Tags{ "Queue" = "Transparent" "RenderType" = "Transparent"}

		LOD 100	//shader level of detail. [https://docs.unity3d.com/Manual/SL-ShaderLOD.html]

		//TODO why must be transparent setting everywhere, where shows the transparency?
		//Blend SrcAlpha OneMinusSrcAlpha

		pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _LightTex;
			sampler2D _MaskTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Thickness;
			float _Speed;
			float _Lightness;

			struct v2f{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				//UNITY_FOG_COORDS(1)
			};

			v2f vert(appdata_full i)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);	//unity built-in method to implement tiling and offset, _MainTex_ST is used inside
				o.pos = UnityObjectToClipPos(i.vertex);
				//UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			float4 frag(v2f IN) : SV_TARGET
			{
				float col = tex2D(_MainTex, IN.uv) * _Color;

				float2 uv = IN.uv * _Thickness;	//TODO This is not exactly 'thickness', what is it?
				uv.x += _Time.y * _Speed;
				uv.y += _Time.y * _Speed;
				float LightAlphaOnThisPixel = tex2D(_LightTex, uv).a;
				float MaskAlphaOnThisPixel = tex2D(_MaskTex, IN.uv).a;	//pick from IN.uv not varified uv

				col = col + LightAlphaOnThisPixel * MaskAlphaOnThisPixel * _Lightness;	//only the pixel inside light flow shape & inside mask will be highlighted
				//UNITY_APPLY_FOG(IN.fogCoord, col);
				return col;
			}

			ENDCG
		}
	}
}
