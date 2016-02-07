// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.projection' with 'UNITY_MATRIX_P'

// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.projection' with 'UNITY_MATRIX_P'

Shader "Mixamo/Toon" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("RGB (RGB)", Color) = (1,1,1,1)
		
		_BumpMap ("Normal (RGB)", 2D) = "bump" {}
		
		_SpecMap ("Specular Map (R)", 2D) = "white" {}
    _SpecAmount ("Specular Amount", Float) = 1.0
    _SpecCutoff ("SpecCutoff", Range(0, 1)) = 0.5
		
		_RampValues ("Ramp Values (RGB)", Color) = (0.83, 0.5, 0.0, 1.0)
		_RampThresholds ("Ramp Thresholds (RGB)", Color) = (0.83, 0.5, 0.0, 1.0)
		_RampBlend ("Ramp Blend", Float) = 0.08
		
		_GlossMap("Gloss Map (R)",2D) = "white" {}
    _GlossAmount ("Gloss Amount", Float) = 1.0
		
		_LightCoefficient ("Light Coefficient", Float) = 1.0
		
		_Fresnel ("Fresnel Amount", Float) = 0.0
		_FresnelCutoff ("Fresnel Cutoff", Float) = 0.4
		
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Float) = .002
	}
	SubShader { 
		Tags { "RenderType"="Opaque" }
		LOD 200
		Name "TOON"
		
		CGPROGRAM

		#pragma surface surf RampLambert nolightmap noambient noforwardadd vertex:vert
		#pragma target 3.0
		#include "Lighting.cginc"

		


		sampler2D _MainTex; 
		sampler2D _Ramp;
		sampler2D _BumpMap;
		sampler2D _SpecMap;
		sampler2D _GlossMap;
		
		//samplerCUBE _Cube;
		half4 _Color;
		half _SpecAmount;
    half _SpecCutoff;
    half _GlossAmount;

		half _LightCoefficient;
		
		half4 _RampValues;
		half4 _RampThresholds;
		half _RampBlend;
		
		half _Fresnel;
		half _FresnelCutoff;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_SpecMap;
			float2 uv_GlossMap;
			float3 worldRefl;
			float3 viewDir;
			float3 worldNormal;
			float3 sphericalHarmonic;
			INTERNAL_DATA
		};
		
		struct MyOutput {
		    half3 Albedo;
		    half3 Normal;
		    half3 Ambient;
		    half3 Emission;
		    half Specular;
		    half Gloss;
		    half Alpha;
		};
		
		void vert (inout appdata_full v, out Input o) {
    #if (defined (SHADER_API_D3D11)) || (defined (SHADER_API_D3D11_9X))
    	o = (Input)0;
    #endif

      // evaluate SH light
      float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);
      o.worldNormal = worldN;
      o.sphericalHarmonic = ShadeSH9 (float4 (worldN, 1.0));
    }
		
		half4 LightingRampLambert (MyOutput s, half3 lightDir, half3 viewDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
			half diff = (NdotL * 0.5 + 0.5);
			
			// ramp
			half rampScalar = _RampValues.b;
			half mixB = clamp((diff-_RampThresholds.g)/_RampBlend, 0.0, 1.0);
			half mixA = clamp((diff-_RampThresholds.r)/_RampBlend, 0.0, 1.0);
			rampScalar = lerp(rampScalar, _RampValues.g, mixB);
			half3 ramp = lerp(rampScalar, _RampValues.r, mixA);
			
			half4 c;
			
			// spec highlight
			half3 h = normalize (lightDir + viewDir);
     	float nh = max (0.0, dot (s.Normal, h));
      float spec = smoothstep(_SpecCutoff, 1.0, pow (nh, s.Gloss * _GlossAmount * 128.0) * s.Specular) * _SpecAmount;
      
      half3 highlight = clamp(spec * atten, 0.0, 1.0);
			
			// ramp lighting
			c.rgb = (s.Albedo * _LightColor0.rgb * ramp * 2  + highlight) * atten + s.Ambient;
			
			c.a = s.Alpha; 
			
			return c;
    }
      	
		void surf (Input IN, inout MyOutput o) {
			
			fixed4 gls = lerp(fixed4(0.5), fixed4(0.25), tex2D(_GlossMap, IN.uv_MainTex)); //gloss map
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));			
			//half3 worldNorm = WorldNormalVector(IN, o.Normal);

			// rim coefficients
			half fresnel =_Fresnel * smoothstep(_FresnelCutoff, 1, (1 - min(pow(dot(o.Normal, normalize(IN.viewDir)), 2), 1)));
			half3 rim = fresnel * IN.sphericalHarmonic;
			// rim from cube map
			
			//half3 rim = fresnel * upness * texCUBE(_Cube, worldNorm).rgb;ß
			//o.Rim = rim;
			//o.Emission = tex2D(_GlowTex, IN.uv_GlowTex).rgb * _GlowStrength;
						
			o.Gloss = gls.r;
			o.Albedo = _Color.rgb * c.rgb; //texCUBE(_Cube, worldNorm).rgb;
			o.Specular = tex2D(_SpecMap, IN.uv_SpecMap).r;
			o.Ambient = (_LightCoefficient * IN.sphericalHarmonic * o.Albedo) + rim;
			o.Alpha = c.a;
		}
		
		ENDCG
      

		CGINCLUDE
		#include "UnityCG.cginc"
 
		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 color : COLOR;
		};
 
		struct v2f {
			float4 pos : POSITION;
			float4 color : COLOR;
		};
 
		uniform float _Outline;
		uniform float4 _OutlineColor;
 
		v2f outline_vert(appdata v) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
 
			float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
			float2 offset = TransformViewToProjection(norm.xy);
 
			o.pos.xy += offset * o.pos.z * _Outline;
			o.color = _OutlineColor*v.color;
			return o;
		}
		ENDCG
 
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
 
			CGPROGRAM
			#pragma vertex outline_vert
			#pragma fragment outline_frag
			half4 outline_frag(v2f i) :COLOR { return i.color; }
			ENDCG
		}
	}
}
