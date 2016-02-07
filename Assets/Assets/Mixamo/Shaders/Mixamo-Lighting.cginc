
inline fixed4 LightingCookTorrance (SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
{
	fixed3 L = normalize(lightDir);
	fixed3 V = normalize(viewDir);
	fixed3 H = normalize(V+L);
	float NdL = saturate(dot(s.Normal, L));
	float NdH = saturate(dot(s.Normal, H));
	float VdH = saturate(dot(V, H));
	float NdV = saturate(dot(s.Normal, V));
	float alpha = acos(NdH);

	//G (geo)
	float G = min(1.0, min(2.0*NdH*NdV/VdH, 2.0*NdH*NdL/VdH)); //geometric attenuation factor
	
	//F (fresnel)
	float F0 = 1.44; // reflection at normal incidence 
	float F = pow(1.0 - VdH, 5.0) * (1.0 - F0) + F0;
	
	//D (roughness)
	float m = s.Specular * _GlossAmount; // roughness
	float D = 1.0 * exp(-(alpha)/(m*m));
	
	float spec = F*D*G*(1.0 - NdV);
	 
	//float DFG = (D * F * G) * (1.0 - NdV);
	//spec = spec_color * DFG * specular_constant;
	half4 c;
	c.rgb = _LightColor0.rgb * NdL * (spec * _SpecAmount * s.Gloss * 32.0 + s.Albedo ) * atten * 2;
	c.a = s.Alpha;
	return c;
}
