Shader "Custom/OutlineShader2D" 
{

//TODO ------------2 passes idea still have some problems--------------

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_OutlineCol ("Outline Color", Color) = (1, 1, 1, 1)
		_OutlineSickness ("Outline Sickness", Range(0, 5)) = 1
	}
	SubShader {

	//Tags from Unity sprite-default
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off	//you can see the sprite in both front and back
		ZWrite Off
		Lighting Off
		Blend One OneMinusSrcAlpha	//can be transparent

		//Pass 2 : outline pass, draw the outline base first because the sprite should render on top
		pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _OutlineCol;
			float _OutlineSickness;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 col : COLOR;
			};

			v2f vert(appdata_t IN)
			{
				v2f o;
				o.uv = IN.texcoord;
				float4 enlargedVertex = IN.vertex * _OutlineSickness;
				o.pos = UnityObjectToClipPos(enlargedVertex);
				o.col = _OutlineCol;

				return o;
			}

			//Only position info in vertex is used
			fixed4 frag(v2f IN) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, IN.uv) * IN.col;
				col.rgb *= col.a;
				return col;
			}

			ENDCG
		}

		//Pass 1 : normal render(no lighting for now)
	pass{
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
			fixed4 col : COLOR;
		};

		sampler2D _MainTex;
		fixed4 _Color;

		v2f vert(appdata_t IN)
		{
			v2f o;
			o.uv = IN.texcoord;
			o.pos = UnityObjectToClipPos(IN.vertex);
			o.col = IN.color * _Color;
			return o;
		}

		fixed4 frag(v2f IN) : SV_TARGET
		{
			fixed4 col = tex2D(_MainTex, IN.uv) * IN.col;
			col.rgb *= col.a;
			return col;
		}

		ENDCG
	}


	}
	FallBack "Diffuse"
}
