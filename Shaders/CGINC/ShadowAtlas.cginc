#define ATLAS_WIDTH 8
#define ATLAS_HEIGHT 5

float2 _SA_AtlasCellDimension;
Texture2D<float2> _SA_AtlasTexture;

SamplerState sa_trilinear_clamp_sampler;

//R - Depth, G - Light Cookie
void GetShadowAtlas(float shadowID, float2 spotUV, out float cookie, out float shadowDepth)
{
	if (shadowID < 0.0)
	{
		cookie = 1.0;
		shadowDepth = 1.0;
		return;
	}

	// Atlas Order:
	// 0 1 2 3 4
	// 5 6 7 8 9
	// 10 ...
	//

	uint atlasIndex = uint(shadowID);
	uint atlasRow = uint(atlasIndex % ATLAS_HEIGHT);
	uint atlasColumn = uint(atlasIndex / ATLAS_HEIGHT);
	uint2 atlasStartPos = uint2(atlasColumn * _SA_AtlasCellDimension.x, atlasRow * _SA_AtlasCellDimension.y);
	uint2 atlasPos = uint2(spotUV.x * _SA_AtlasCellDimension.x, spotUV.y * _SA_AtlasCellDimension.y);

	float2 shadow = _SA_AtlasTexture[atlasStartPos + atlasPos];

	cookie = shadow.g;
	shadowDepth = shadow.r;
}