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

/* Terrain height */

float GetTerrainHeight(in float4 col) {
	return col.a * 2 - 1;
}

float SampleTerrainHeightGeo(in sampler2D terrainMap, in float2 geo) {
	return GetTerrainHeight(SampleTex2D(terrainMap, geo));
}

float SampleTerrainHeightLocal(in sampler2D terrainMap, in float3 local) {
	return SampleTerrainHeightGeo(terrainMap, LocalToGeo(local));
}

/* Full terrain height info */

void SampleTerrainHeightFullGeo(in sampler2D terrainMap, in float2 geo, out TerrainInfo info) {
	float2 uv = Geo2Uv(geo);
	info.geo = geo;
	float4 col = SampleTex2D(terrainMap, uv);
	info.altitude = GetTerrainHeight(col);
	info.gradient = col.rg;
	info.laplacian = col.b;
}

void SampleTerrainHeightFullLocal(in sampler2D terrainMap, in float3 local, out TerrainInfo info) {
	SampleTerrainHeightFullGeo(terrainMap, LocalToGeo(local), info);
}

/* Rendering (outputting) */

float4 RenderTerrainHeightFull(in TerrainInfo info) {
	return float4(info.gradient, info.laplacian, clamp((info.altitude + 1) * 0.5, 0, 1));
}