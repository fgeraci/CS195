Shader "Mixamo/Cook Torrance" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (1,1,1,1)
	
	_Specular ("Specular", 2D) = "white" {}
	_SpecAmount ("Specular Amount",Range(0.0, 2.0)) = 1.0
	
	_Gloss("Gloss",2D) = "white" {}
	_GlossAmount("Gloss Amount",Range(0.0, 2.0)) = 1.0
	
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {} 
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
CGPROGRAM
#include "UnityCG.cginc"
#include "Lighting.cginc"

#pragma target 3.0
#pragma surface surf CookTorrance 

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Gloss;
sampler2D _Specular;

half _SpecAmount;
half _GlossAmount;

#include "Mixamo-Lighting.cginc"

fixed4 _Color;
 
struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	float4 tex = tex2D(_MainTex, IN.uv_MainTex);
	float4 gls = lerp(float4(0.5), float4(0.25), tex2D(_Gloss, IN.uv_MainTex));
	float4 spec = pow(tex2D(_Specular,IN.uv_MainTex), float4(1.0/0.7));

	o.Albedo = tex.rgb * _Color.rgb;
	o.Alpha = tex.a * _Color.a;
	o.Gloss = (spec.r + spec.g + spec.b) / 3.0;
	o.Specular = (gls.r + gls.g + gls.b) / 3.0;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Diffuse"
}
