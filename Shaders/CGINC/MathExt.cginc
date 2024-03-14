#ifndef MATHEXT_DEFINE
#define MATHEXT_DEFINE

static const float PI = 3.141592657f;
static const float EPSILON = 2.4414e-4;

static const int MAX_HALTON_SEQUENCE = 16;

static const float3 HALTON_SEQUENCE[MAX_HALTON_SEQUENCE] = {
	float3( 0.5, 0.333333, 0.2 ),
	float3( 0.25, 0.666667, 0.4 ),
	float3( 0.75, 0.111111, 0.6 ),
	float3( 0.125, 0.444444, 0.8 ),
	float3( 0.625, 0.777778, 0.04 ),
	float3( 0.375, 0.222222, 0.24 ),
	float3( 0.875, 0.555556, 0.44 ) ,
	float3( 0.0625, 0.888889, 0.64 ),
	float3( 0.5625, 0.037037, 0.84 ),
	float3( 0.3125, 0.37037, 0.08 ),
	float3( 0.8125, 0.703704, 0.28 ),
	float3( 0.1875, 0.148148, 0.48 ),
	float3( 0.6875, 0.481482, 0.68 ),
	float3( 0.4375, 0.814815, 0.88 ),
	float3( 0.9375, 0.259259, 0.12 ),
	float3( 0.03125, 0.592593, 0.32 )
};

float luminance(float3 col)
{
	return col.r * 0.3 + col.g * 0.59 + col.b * 0.11;
}

float invLerp(float from, float to, float value){
  return saturate((value - from) / (to - from));
}

float3 RgbToHsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = EPSILON;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HsvToRgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float3 multPoint(float4x4 mtx, float3 pt)
{
	float4 m = mul(mtx, float4(pt, 1.0));
	m /= m.w;
	return m.xyz;
}

float4 tex3DTricubic(sampler3D tex, float3 coord, float3 textureSize)
{
	// Shift the coordinate from [0,1] to [-0.5, textureSize-0.5]
	float3 coord_grid = coord * textureSize - 0.5;
	float3 index = floor(coord_grid);
	float3 fraction = coord_grid - index;
	float3 one_frac = 1.0 - fraction;

	float3 w0 = 1.0/6.0 * one_frac*one_frac*one_frac;
	float3 w1 = 2.0/3.0 - 0.5 * fraction*fraction*(2.0-fraction);
	float3 w2 = 2.0/3.0 - 0.5 * one_frac*one_frac*(2.0-one_frac);
	float3 w3 = 1.0/6.0 * fraction*fraction*fraction;

	float3 g0 = w0 + w1;
	float3 g1 = w2 + w3;
	float3 mult = 1.0 / textureSize;
	float3 h0 = mult * ((w1 / g0) - 0.5 + index); //h0 = w1/g0 - 1, move from [-0.5, textureSize-0.5] to [0,1]
	float3 h1 = mult * ((w3 / g1) + 1.5 + index); //h1 = w3/g1 + 1, move from [-0.5, textureSize-0.5] to [0,1]

	// Fetch the eight linear interpolations
	// Weighting and fetching is interleaved for performance and stability reasons
	float4 tex000 = tex3Dlod(tex, float4(h0, 0));
	float4 tex100 = tex3Dlod(tex, float4(h1.x, h0.y, h0.z, 0));
	tex000 = lerp(tex100, tex000, g0.x); // Weigh along the x-direction

	float4 tex010 = tex3Dlod(tex, float4(h0.x, h1.y, h0.z, 0));
	float4 tex110 = tex3Dlod(tex, float4(h1.x, h1.y, h0.z, 0));
	tex010 = lerp(tex110, tex010, g0.x); // Weigh along the x-direction
	tex000 = lerp(tex010, tex000, g0.y); // Weigh along the y-direction

	float4 tex001 = tex3Dlod(tex, float4(h0.x, h0.y, h1.z, 0));
	float4 tex101 = tex3Dlod(tex, float4(h1.x, h0.y, h1.z, 0));
	tex001 = lerp(tex101, tex001, g0.x); // Weigh along the x-direction

	float4 tex011 = tex3Dlod(tex, float4(h0.x, h1.y, h1.z, 0));
	float4 tex111 = tex3Dlod(tex, float4(h1, 0));
	tex011 = lerp(tex111, tex011, g0.x); // Weigh along the x-direction
	tex001 = lerp(tex011, tex001, g0.y); // Weigh along the y-direction

	return lerp(tex001, tex000, g0.z); // Weigh along the z-direction
}

float Rayleigh(float3 wi, float3 wo)
{
	float cosTheta = dot( wi, wo );
    return (3.0f / (16.0f * PI)) * (1 + cosTheta * cosTheta);
}

float HenyeyGreenstein( float3 wi, float3 wo, float g )
{
	float cosTheta = dot( wi, wo );
	float g2 = g * g;
	float denom = pow( 1.f + g2 - 2.f * g * cosTheta, 3.f / 2.f );
	return ( 1.f / ( 4.f * PI ) ) * ( ( 1.f - g2 ) / max( denom, EPSILON ) );
}

int ihash(int n)
{
	n = (n<<13)^n;
	return (n*(n*n*15731+789221)+1376312589) & 2147483647;
}

float frand(int n)
{
	return ihash(n) / 2147483647.0;
}

float2 cellNoise(int2 p)
{
	int i = p.y*256 + p.x;
	return float2(frand(i), frand(i + 57)) - 0.5;//*2.0-1.0;
}

float sqrDistance(float3 a, float3 b)
{
	float3 del = a - b;
	return dot(del, del);
}

#endif