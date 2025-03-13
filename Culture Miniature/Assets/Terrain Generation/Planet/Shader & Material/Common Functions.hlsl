/* Constant */

static float HALF_PI = atan(1);
static float INV_HALF_PI = 1 / HALF_PI;

/* Aliases */

#define PLANET_HEIGHTMAP heightMap;

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

/* Height map */

float ExtractHeight(in float4 col) {
	return col.a * 2 - 1;
}

void SampleHeight_Geo(in sampler2D heightMap, in float2 geo, out TerrainInfo info) {
	float2 uv = Geo2Uv(geo);
	info.geo = geo;
	float4 col = SampleTex2D(heightMap, uv);
	info.altitude = ExtractHeight(col);
	info.gradient = col.rg;
	info.laplacian = col.b;
	info.laplacian = 0;  // DEBUG
}

void SampleHeight_Local(in sampler2D heightMap, in float3 local, out TerrainInfo info) {
	SampleHeight_Geo(heightMap, LocalToGeo(local), info);
}

float CalculateHeightSecondDerivative_Local(in sampler2D heightMap, in float3 origin,
	in float3 tangent, in float originHeight, in float angularPixelSize
) {
	angularPixelSize *= 0.5;
	TerrainInfo terrain;
	SampleHeight_Local(heightMap, RotateVectorAxisAngle(origin, tangent, angularPixelSize), terrain);
	float a = terrain.altitude;
	SampleHeight_Local(heightMap, RotateVectorAxisAngle(origin, tangent, -angularPixelSize), terrain);
	float b = terrain.altitude;
	return (b + a - 2 * originHeight) / angularPixelSize;
}

float CalculateHeightLaplacianLayered_Local(in sampler2D heightMap, in float3 local, in int endingIteration) {
	local = normalize(local);

	float3 tangent = cross(local, float3(1, 0, 0));
	if(length(tangent) < 0.1)
		tangent = cross(local, float3(0, 1, 0));
	tangent = normalize(tangent);

	TerrainInfo terrain;
	SampleHeight_Local(heightMap, local, terrain);
	float originHeight = terrain.altitude;

	float baseAngularPixelSize = HALF_PI;
	float sum = 0, totalEnergy = 0;

	for(int i = 2; i < endingIteration; ++i) {
		float scalar = pow(2, -i), localSum = 0;
		totalEnergy += scalar;
		float angularPixelSize = scalar * baseAngularPixelSize;
		localSum += CalculateHeightSecondDerivative_Local(heightMap, local, tangent, originHeight, angularPixelSize);
		localSum += CalculateHeightSecondDerivative_Local(heightMap, local, cross(tangent, local), originHeight, angularPixelSize);
		sum += scalar * localSum;
	}

	return sum / totalEnergy;
}

/* Rendering (outputting) */

float4 RenderTerrainHeightFull(in TerrainInfo info) {
	return float4(info.gradient, info.laplacian, clamp((info.altitude + 1) * 0.5, 0, 1));
}