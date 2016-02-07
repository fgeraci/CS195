//1a. Take Gloss out of Alpha Channel
//1b. Rename it to Specular
//2. Add a texture input for Gloss
//3. Add Specular_Amount and Gloss_Amount

Shader "Mixamo/Transparent/Cook Torrance" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (1,1,1,1)
	
	_Specular ("Specular", 2D) = "white" {}
	_Specular_Amount ("Specular Amount",Range(0.0, 2.0)) = 1.0
	
	_Gloss("Gloss",2D) = "white" {}
	_Gloss_Amount("Gloss Amount",Range(0.0, 2.0)) = 1.0
	
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {} 
}
SubShader { 
  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}   
	LOD 400
	
CGPROGRAM
#include "UnityCG.cginc"
#include "Lighting.cginc"

#pragma target 3.0
#pragma surface surf CookTorrance alpha

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Gloss;
sampler2D _Specular;

half _SpecAmount;
half _GlossAmount;

fixed4 _Color;

#include "Mixamo-Lighting.cginc" 

struct Input {
	float2 uv_MainTex;
  float3 viewDir;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 gls = lerp(fixed4(0.5), fixed4(0.25), tex2D(_Gloss, IN.uv_MainTex));
	fixed4 spec = pow(tex2D(_Specular,IN.uv_MainTex),1.0/0.7);
	
	o.Albedo = tex.rgb * _Color.rgb;
	o.Alpha = tex.a * _Color.a;
	o.Gloss = (spec.r + spec.g + spec.b) / 3.0;
	o.Specular = (gls.r + gls.g + gls.b) / 3.0;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Transparent/VertexLit"
}
