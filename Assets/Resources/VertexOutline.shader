Shader "Custom/VertexOutline" {
	Properties {
	}
	SubShader {
		Lighting Off
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf NoLighting

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR; 

		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = IN.color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		
		 fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
     {
         fixed4 c;
         c.rgb = s.Albedo; 
         c.a = s.Alpha;
         return c;
     }
		ENDCG
	} 
	FallBack "Diffuse"
}
