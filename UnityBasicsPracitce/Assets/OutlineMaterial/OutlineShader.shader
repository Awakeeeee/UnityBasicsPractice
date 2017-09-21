Shader "Custom/OutlineShader" 
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
		_OutlineSickness ("Outline Sickness", Range(0, 1)) = 0.01
	}
	SubShader {

		//Pass 1 : normal render(no lighting for now)
	pass{
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		fixed4 _Color;

		v2f vert(appdata_base IN)
		{
			v2f o;
			o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
			o.pos = UnityObjectToClipPos(IN.vertex);
			return o;
		}

		fixed4 frag(v2f IN) : SV_TARGET
		{
			fixed4 col = tex2D(_MainTex, IN.uv);

			return col * _Color;
		}

		ENDCG
	}


		//Pass 2 : outline pass
		pass
		{
			Cull Front	//donnot render when look in 'front' of the mesh

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _OutlineColor;
			float _OutlineSickness;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 nor : TEXCOORD1;
			};

			v2f vert(appdata_base IN)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(IN.vertex);
				//TODO confusion idea
				//get object normals in clip space(instead of model space), enlarget the model pos along clip normal direction, which form the outline
				o.nor = mul((float3x3)UNITY_MATRIX_MVP, IN.normal);
				o.pos.xy += o.nor.xy * _OutlineSickness;

				return o;
			}

			//Only position info in vertex is used
			fixed4 frag(v2f IN) : SV_TARGET
			{
				return _OutlineColor;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
