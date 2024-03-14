#include <MathExt.cginc>
#include <ShadowAtlas.cginc>

struct PointLight //Point Light
{
	float3 position;
	float invSqrRange;
	float3 irradiance;
	float physical;
};

struct SpotLight //Spot Light
{
	float4x4 VP;
	float3 position;
	float invSqrRange;
    float invNearRange;
	float3 irradiance;
	float shadowIndex;
};

struct FogSphere
{
	float3 position;
	float invSqrRange;
	float3 irradiance;
	float densityModifier;
};

float distanceSqrFalloff(float invSqrRange, float3 origin, float3 pos)
{
    float sqrDist = sqrDistance(origin, pos);
	float falloff = saturate(1.0 - (sqrDist * invSqrRange));
	return falloff * falloff;
}

float getPointLightFalloff(float3 worldPos, PointLight light)
{
	return distanceSqrFalloff(light.invSqrRange, light.position, worldPos);
}

float getFogSphereFalloff(float3 worldPos, FogSphere fogSphere)
{
	return distanceSqrFalloff(fogSphere.invSqrRange, fogSphere.position, worldPos);
}

float getSpotLightFalloff(float3 worldPos, SpotLight light)
{
	float3 screenPos = multPoint(light.VP, worldPos);

	if (screenPos.x > 1.0 || screenPos.x < -1.0) return 0.0;
	if (screenPos.y > 1.0 || screenPos.y < -1.0) return 0.0;
	if (screenPos.z < 0.0) return 0.0;

    #ifdef USE_SHADOW_ATLAS
    if (light.shadowIndex >= 0.0)
    {
        float cookie, shadowDepth;
        float2 shadowUV = (screenPos.xy + float2(1.0, 1.0)) * float2(0.5, 0.5);
        GetShadowAtlas(light.shadowIndex, shadowUV, cookie, shadowDepth);

        //Lazy code
        float sqrDist = sqrDistance(light.position, worldPos);
        if (shadowDepth > 0.0)
        {
            float3 dir = normalize(worldPos - light.position);
            //float projection = 1.0 / dot(float3(0.0, 0.0, -1.0), dir);
            float depthDistance = lerp(1/light.invNearRange, sqrt(1/light.invSqrRange), shadowDepth);
            //depthDistance *= projection;

            if (sqrDist > (depthDistance * depthDistance))
            {
                return 0.0;
            }
        }

        
	    float distanceFactor = 1.0 - saturate(sqrDist * light.invSqrRange);
        return saturate((distanceFactor * distanceFactor) * cookie);
    }
    #endif

    //Fallback
    float angleDist = dot(screenPos.xy, screenPos.xy);
	if (angleDist > 1.0 || angleDist < 0.0) return 0.0;
	angleDist = saturate(angleDist);
    float angleFactor = 1.0 - (angleDist * angleDist);

	float sqrDist = sqrDistance(light.position, worldPos);
	float distanceFactor = 1.0 - saturate(sqrDist * light.invSqrRange);

    return saturate((distanceFactor * distanceFactor) * angleFactor);
}
