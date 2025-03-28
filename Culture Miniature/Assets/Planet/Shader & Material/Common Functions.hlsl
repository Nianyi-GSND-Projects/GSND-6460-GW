/* Constant */

static float QUARTER_PI = atan(1);
static float INV_QUARTER_PI = 1 / QUARTER_PI;
static float PI = atan(1) * 4;
static float INV_PI = 1 / PI;
static float TAU = atan(1) * 8;
static float INV_TAU = 1 / TAU;

/* Aliases */

#define PLANET_HEIGHTMAP heightMap;

/* Type definition */

struct TerrainInfo {
	float2 geo;
	float altitude;
	float2 gradient;
	float laplacian;

	float3 normal;
	float3 tangent;
	float3 cotangent;
};

/* Coordinate conversion */

float2 Geo2Uv(in float2 geo) {
	return geo * float2(INV_PI, INV_PI * 2);
}
float2 Uv2Geo(in float2 uv) {
	return uv * float2(PI, PI / 2);
}

float2 Local2Geo(in float3 local) {
	return float2(atan2(local.x, local.z), atan2(local.y, length(local.zx)));
}
float3 Geo2Local(in float2 geo) {
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

float3 FindTangent(in float3 normal) {
	float3 tangent = cross(normal, float3(0, 1, 0));
	return normalize(tangent);
}

void RotateTerrainInfo(inout TerrainInfo source) {
	float3 tangent = source.tangent;
	source.tangent = source.cotangent;
	source.cotangent = -tangent;
}

/* Height map */

float ExtractHeight(in float4 col) {
	return col.a * 2 - 1;
}

float SampleHeightSimple_Geo(in sampler2D heightMap, in float2 geo) {
	float2 uv = Geo2Uv(geo);
	float4 col = SampleTex2D(heightMap, uv);
	return ExtractHeight(col);
}

float SampleHeightSimple_Local(in sampler2D heightMap, in float3 local) {
	return SampleHeightSimple_Geo(heightMap, Local2Geo(local));
}

void SampleHeight_Geo(in sampler2D heightMap, in float2 geo, out TerrainInfo info) {
	float2 uv = Geo2Uv(geo);
	info.geo = geo;
	float4 col = SampleTex2D(heightMap, uv);

	info.altitude = ExtractHeight(col);
	info.gradient = col.rg;
	info.laplacian = col.b;
	info.laplacian = 0;  // DEBUG

	info.normal = Geo2Local(geo);
	info.tangent = FindTangent(info.normal);
	info.cotangent = cross(info.normal, info.tangent);
}

void SampleHeight_Local(in sampler2D heightMap, in float3 local, out TerrainInfo info) {
	SampleHeight_Geo(heightMap, Local2Geo(local), info);
}

float CalculateHeightD_Local(in sampler2D heightMap, in TerrainInfo origin, in float angularPixelSize) {
	float a = SampleHeightSimple_Local(heightMap, RotateVectorAxisAngle(origin.normal, origin.tangent, -angularPixelSize * 0.5));
	float b = SampleHeightSimple_Local(heightMap, RotateVectorAxisAngle(origin.normal, origin.tangent, +angularPixelSize * 0.5));
	return (b - a) / angularPixelSize;
}

float CalculateHeightDD_Local(in sampler2D heightMap, in TerrainInfo origin, in float angularPixelSize) {
	float a = SampleHeightSimple_Local(heightMap, RotateVectorAxisAngle(origin.normal, origin.tangent, -angularPixelSize * 0.5));
	float b = SampleHeightSimple_Local(heightMap, RotateVectorAxisAngle(origin.normal, origin.tangent, +angularPixelSize * 0.5));
	return (b + a - 2 * origin.altitude) / (angularPixelSize * angularPixelSize);
}

float CalculateHeightLaplacianLayered_Local(in sampler2D heightMap, in TerrainInfo origin, in int endingIteration) {
	float sum = 0;
	TerrainInfo rotated = origin;
	RotateTerrainInfo(rotated);

	for(int i = 3; i < endingIteration; ++i) {
		float scalar = pow(2, -i), localSum = 0;
		float angularPixelSize = scalar * QUARTER_PI * 2;
		localSum += CalculateHeightDD_Local(heightMap, origin, angularPixelSize);
		localSum += CalculateHeightDD_Local(heightMap, rotated, angularPixelSize);
		sum += scalar * localSum * angularPixelSize;
	}

	return sum;
}

float2 CalculateHeightGradient_Geo(in sampler2D heightMap, in float2 geo, in float subdivisionLevel) {
	float3 local = Geo2Local(geo);
	float3 tangent = FindTangent(local), cotangent = cross(tangent, local);
	float angularPixelSize = pow(2, -subdivisionLevel) * QUARTER_PI * 2;

	TerrainInfo origin;
	SampleHeight_Local(heightMap, local, origin);
	TerrainInfo rotated = origin;
	RotateTerrainInfo(rotated);

	float2 gradient;
	gradient.x = CalculateHeightD_Local(heightMap, origin, angularPixelSize);
	gradient.y = CalculateHeightD_Local(heightMap, rotated, angularPixelSize);
	gradient *= angularPixelSize;

	return gradient;
}

float3 CalculateTangentSpaceNormal(in TerrainInfo origin, in float strength) {
	return -normalize(float3(origin.gradient * strength, -1));
}

/* Rendering (outputting) */

float4 RenderTerrainHeightFull(in TerrainInfo info) {
	return float4(info.gradient, info.laplacian, clamp((info.altitude + 1) * 0.5, 0, 1));
}