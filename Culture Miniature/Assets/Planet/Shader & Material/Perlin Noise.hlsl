#pragma once

// Source: https://briansharpe.wordpress.com/2011/11/15/a-fast-and-simple-32bit-floating-point-hash-function/

float4 FAST_32_hash( float3 gridcell ) {
	//    gridcell is assumed to be an integer coordinate
	const float2 OFFSET = float2( 26.0, 161.0 );
	const float DOMAIN = 71.0;
	const float SOMELARGEFLOAT = 951.135664;
	float4 P = float4( gridcell.xy, gridcell.yz + 1.0.xx );
	P = P - floor(P * ( 1.0 / DOMAIN )) * DOMAIN;    //    truncate the domain
	P += OFFSET.xyxy;                                //    offset to interesting part of the noise
	P *= P;                                          //    calculate and return the hash
	return frac( P.xzxz * P.yyww * ( 1.0 / SOMELARGEFLOAT.x ).xxxx );
}

// Source: https://gist.github.com/oscnord/35cbe399853b338e281aaf6221d9a29b

// hash based 3d value noise
// function taken from https://www.shadertoy.com/view/XslGRr
// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

// ported from GLSL to HLSL
float hash( float n ) {
	return sin(n)*43758.5453;
}
float fhash( float n ) {
	return frac(hash(n));
}

// The noise function returns a value in the range -1.0f -> 1.0f
float noise( float3 x ) {
	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0-2.0*f);
	float n = p.x + p.y*57.0 + 113.0*p.z;

	float r = lerp(lerp(lerp( fhash(n+0.0), fhash(n+1.0),f.x),
		lerp( fhash(n+57.0), fhash(n+58.0),f.x),f.y),
		lerp(lerp( fhash(n+113.0), fhash(n+114.0),f.x),
		lerp( fhash(n+170.0), fhash(n+171.0),f.x),f.y),f.z);
	return r * 2 - 1;
}

// Below is written by Nianyi.

float Perlin3D(in float3 pos, in float frequency, in float amplitude, in uint seed) {
	pos *= frequency;
	pos += FAST_32_hash(float3(seed, seed + 71, seed + 59)).xyz;
	return noise(pos) * amplitude;
}

float3 Perlin3D_Gradient(in float3 pos, in float frequency, in float amplitude, in uint seed) {
	float d = 0.0001 * frequency;
	float v = Perlin3D(pos, frequency, amplitude, seed);
	return (1 / d) * float3(
		Perlin3D(pos + float3(d, 0, 0), frequency, amplitude, seed) - v,
		Perlin3D(pos + float3(0, d, 0), frequency, amplitude, seed) - v,
		Perlin3D(pos + float3(0, 0, d), frequency, amplitude, seed) - v
	);
}