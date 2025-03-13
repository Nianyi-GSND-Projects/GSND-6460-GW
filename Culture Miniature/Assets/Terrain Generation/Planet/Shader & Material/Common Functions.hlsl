/* Constant */

static float HALF_PI = atan(1);
static float INV_HALF_PI = 1 / HALF_PI;

/* Type definition */

struct TerrainInfo {
	float2 geo;
	float altitude;
	float2 gradient;
	float laplacian;
};

/* Coordinate conversion */

float2 Geo2Uv(in float2 geo) {
	return geo * float2(INV_HALF_PI * 0.25, INV_HALF_PI * 0.5);
}
float2 Uv2Geo(in float2 uv) {
	return uv * float2(HALF_PI * 4, HALF_PI * 2);
}

float2 LocalToGeo(in float3 local) {
	return float2(atan2(local.x, local.z), atan2(local.y, length(local.zx)));
}
float3 GeoToLocal(in float2 geo) {
	float2 plane = float2(cos(geo.x), sin(geo.x));
	plane *= cos(geo.y);
	return float3(plane.x, sin(geo.y), plane.y);
}

/* Auxiliary functions */

float4 SampleTex2D(in sampler2D tex, in float2 uv) {
#if SHADER_STAGE_VERTEX
	return tex2Dlod(tex, float4(uv, 0, 0));
#else
	return tex2D(tex, uv);
#endif
}

float3 RotateVectorAxisAngle(in float3 vec, in float3 axis, in float3 angle) {
	float3 c = cross(axis, vec);
	return vec + sin(angle) * c + (1 - cos(angle)) * cross(axis, c);
}

/* Terrain height */

float GetTerrainHeight(in float4 col) {
	return col.a * 2 - 1;
}

float SampleTerrainHeightGeo(in sampler2D terrainMap, in float2 geo) {
	return GetTerrainHeight(SampleTex2D(terrainMap, Geo2Uv(geo)));
}

float SampleTerrainHeightLocal(in sampler2D terrainMap, in float3 local) {
	return SampleTerrainHeightGeo(terrainMap, LocalToGeo(local));
}

/* Full terrain height info */

float CalculateTerrainHeightSecondDerivativeLocal(in sampler2D terrainMap, in float3 origin,
	in float3 tangent, in float originHeight, in float angularPixelSize
) {
	angularPixelSize *= 0.5;
	float a = SampleTerrainHeightLocal(terrainMap, RotateVectorAxisAngle(origin, tangent, angularPixelSize));
	float b = SampleTerrainHeightLocal(terrainMap, RotateVectorAxisAngle(origin, tangent, -angularPixelSize));
	return (b + a - 2 * originHeight) / angularPixelSize;
}

float CalculateTerrainHeightLaplacianLocal(in sampler2D terrainMap, in float3 local, in float angularPixelSize) {
	local = normalize(local);
	float3 tangent = cross(local, float3(1, 0, 0));
	if(length(tangent) < 0.1)
		tangent = cross(local, float3(0, 1, 0));
	tangent = normalize(tangent);
	float originHeight = SampleTerrainHeightLocal(terrainMap, local);

	float sum = 0;
	sum += CalculateTerrainHeightSecondDerivativeLocal(terrainMap, local, tangent, originHeight, angularPixelSize);
	sum += CalculateTerrainHeightSecondDerivativeLocal(terrainMap, local, cross(tangent, local), originHeight, angularPixelSize);
	return sum;
}

float CalculateTerrainHeightLaplacianLayeredLocal(in sampler2D terrainMap, in float3 local, in int endingIteration) {
	local = normalize(local);
	float3 tangent = cross(local, float3(1, 0, 0));
	if(length(tangent) < 0.1)
		tangent = cross(local, float3(0, 1, 0));
	tangent = normalize(tangent);
	float originHeight = SampleTerrainHeightLocal(terrainMap, local);

	float baseAngularPixelSize = HALF_PI;
	float sum = 0, totalEnergy = 0;

	for(int i = 2; i < endingIteration; ++i) {
		float scalar = pow(2, -i);
		totalEnergy += scalar;
		float angularPixelSize = scalar * baseAngularPixelSize;
		sum += scalar * CalculateTerrainHeightSecondDerivativeLocal(terrainMap, local, tangent, originHeight, angularPixelSize);
		sum += scalar * CalculateTerrainHeightSecondDerivativeLocal(terrainMap, local, cross(tangent, local), originHeight, angularPixelSize);
	}

	return sum / totalEnergy;
}

void SampleTerrainHeightFullGeo(in sampler2D terrainMap, in float2 geo, out TerrainInfo info) {
	float2 uv = Geo2Uv(geo);
	info.geo = geo;
	float4 col = SampleTex2D(terrainMap, uv);
	info.altitude = GetTerrainHeight(col);
	info.gradient = col.rg;
	info.laplacian = col.b;
	info.laplacian = 0;  // DEBUG
}

void SampleTerrainHeightFullLocal(in sampler2D terrainMap, in float3 local, out TerrainInfo info) {
	SampleTerrainHeightFullGeo(terrainMap, LocalToGeo(local), info);
}

/* Rendering (outputting) */

float4 RenderTerrainHeightFull(in TerrainInfo info) {
	return float4(info.gradient, info.laplacian, clamp((info.altitude + 1) * 0.5, 0, 1));
}