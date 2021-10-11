Shader "Stencil/Read"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 150
		
		Stencil{
			Ref [_StencilRef]
			Comp Equal
		}
		
		CGPROGRAM
		
		#pragma surface surf Lambert
		sampler2D _MainTex;
		
		struct Input {
			float2 uv_MainTex;
		};
		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		
		ENDCG
	}
}
