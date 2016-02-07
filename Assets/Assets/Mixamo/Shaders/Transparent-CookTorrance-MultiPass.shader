Shader "Mixamo/Transparent/Cook Torrance MultiPass" {
Properties {
  _MainTex ("Base and Alpha (RGBA)", 2D) = "white" {}
  _Specular ("Specular", 2D) = "black" {}
  _SpecAmount ("Specular Amount", Range(0.0, 2.0)) = 1.0
  _Gloss ("Gloss", 2D) = "white" {}
  _GlossAmount ("Gloss Amount", Range(0.0, 2.0)) = 1.0
  _BumpMap ("BumpMap", 2D) = "bump" {}
  _Color ("Main Color", Color) = (1,1,1,1)
  _RimColor ("Rim Color", Color) = (0.2, 0.2, 0.2, 0.0)
  _RimPower ("Rim Power", Range(0.5, 8.0)) = 2.5
}

CGINCLUDE

  sampler2D _MainTex;
  sampler2D _Specular;
  sampler2D _Gloss;
  sampler2D _BumpMap;
  float4 _Color;
  float4 _RimColor;
  float _RimPower;
  float _GlossAmount;
  float _SpecAmount;

  #include "Mixamo-Lighting.cginc"
  
  struct Input {
    float2 uv_MainTex;
    float3 viewDir;
  };
  
  void surf (Input IN, inout SurfaceOutput o) {
    half4 c = tex2D (_MainTex, IN.uv_MainTex);
    
    o.Albedo = c.rgb * _Color.rgb;
    o.Alpha = c.a;
    o.Normal = UnpackNormal(tex2D (_BumpMap, IN.uv_MainTex));
    
    fixed4 gls = lerp(fixed4(0.5), fixed4(0.25), tex2D(_Gloss, IN.uv_MainTex));
    fixed4 spec = pow(tex2D(_Specular,IN.uv_MainTex),1.0/0.7);
    o.Gloss = (spec.r + spec.g + spec.b) / 3.0;
    o.Specular = (gls.r + gls.g + gls.b) / 3.0;
  }

ENDCG

SubShader {
  Tags { "RenderType"="Transparent" "Queue"="AlphaTest"}
  LOD 200

  // prime the alpha values
  ZWrite On
  ZTest Less
  Cull Off
  AlphaTest Equal 1
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask A

  CGPROGRAM
  #pragma surface alphaPass Lambert noforwardadd alpha

  void alphaPass(Input IN, inout SurfaceOutput o) {
    half4 c = tex2D (_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * _Color.rgb;
    o.Alpha = c.a;
  }

  ENDCG

  // CORE PASS - this is the main pass
  ZWrite Off
  ZTest Equal
  Cull Back
  AlphaTest Off
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask RGBA

  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf CookTorrance noforwardadd 

  ENDCG

  // fringe back face pass
  ZWrite Off
  ZTest Less
  Cull Front
  AlphaTest Off
  Blend SrcAlpha OneMinusSrcAlpha
  
  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf CookTorrance noforwardadd 

  ENDCG

  // fringe alpha pass, just the front details
  ZWrite On
  ZTest Less
  Cull Back
  AlphaTest Off
  Blend SrcAlpha OneMinusSrcAlpha

  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf CookTorrance noforwardadd 

  ENDCG
  
}

  FallBack "Transparent/VertexLit"
}