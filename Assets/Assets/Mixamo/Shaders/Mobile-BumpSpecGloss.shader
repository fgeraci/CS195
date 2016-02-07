//1a. Take Gloss out of Alpha Channel
//1b. Rename it to Specular
//2. Add a texture input for Gloss
//3. Add Specular_Amount and Gloss_Amount

Shader "Mixamo/Mobile/Bump Spec Gloss" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (1,1,1,1)
	_Specular ("Specular", 2D) = "white" {}
	_Specular_Amount ("Specular Amount",Range(0.0,2.0)) = 1.0
	
	_Gloss("Gloss",2D) = "white" {}
	_Gloss_Amount("Gloss Amount",Range(0.0,2.0)) = 1.0
	
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {} 
}
SubShader { 
	Tags { "RenderType"="Opaque"}
	LOD 400 
	
CGPROGRAM
#include "UnityCG.cginc"
#include "Lighting.cginc"

#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview novertexlights

inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	fixed nh = max (0, dot (s.Normal, halfDir));
	fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten*2);
	c.a = 0.0;
	return c;
}

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Gloss;
sampler2D _Specular;

half _Specular_Amount;
half _Gloss_Amount;

fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 gls = tex2D(_Gloss, IN.uv_MainTex);
	fixed4 spec = tex2D(_Specular,IN.uv_MainTex);
	
	o.Albedo = tex.rgb * _Color.rgb;
	o.Alpha = tex.a * _Color.a;
	o.Gloss = (spec.r + spec.g + spec.b) / 3.0 * _Gloss_Amount;
	o.Specular = (gls.r + gls.g + gls.b) / 3.0 * _Specular_Amount;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Transparent/VertexLit"
}
