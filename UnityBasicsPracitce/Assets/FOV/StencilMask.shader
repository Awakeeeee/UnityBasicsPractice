Shader "FOVStencil/StencilMask" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {

		//----------------------------------Stencil mask-------------------------
		Tags { "RenderType"="Opaque" "Queue"="Geometry-100"}

		//turn off the color output of this shader
		ColorMask 0
		//depth test uses z distance with camera, we want things under the mask be drawn, not be hided, so turn zwite off to turn off writing to depth buffer
		ZWrite off

		Stencil{	//define stencil buffer
			Ref 1	//the value write to stencil buffer is 1(could be 0 - 255), this is the reference value
			Comp always  //the comparison function of stencil buffer. compare this reference value with the value in stencil buffer(less - only render pixel whose ref less than the value in stencil buffer. always - always render without conditions)
			Pass replace //what to do if stencil test passes: replace means if stencil test pass, write reference value of this into stencil buffer
		}
		//-----------------------------------------------------------------------

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
