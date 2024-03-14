#include <MathExt.cginc>

float3 _VF_VoxelDimension;

float4x4 _VF_Inv_View;
float4x4 _VF_Inv_Proj;
float4x4 _VF_Prev_VP;

float3 _VF_CameraWorldPos;

#define FogNearClip _VF_FogParams.x
#define FogFarClip _VF_FogParams.y
#define FogConversionExponent _VF_FogParams.w
float4 _VF_FogParams;

#define NoiseCoeff _VF_ScatteringParams.x
#define PhaseCoeff _VF_ScatteringParams.y
#define AbsorbtionCoeff _VF_ScatteringParams.z
#define ExtinctionCoeff _VF_ScatteringParams.w
float4 _VF_ScatteringParams; 

#define ReprojectionBlend _VF_ReprojectionParams.x
#define HasPreviousFrame _VF_ReprojectionParams.z
#define FrameCount _VF_ReprojectionParams.w
float4 _VF_ReprojectionParams;

// --
// Math
// --

float PhaseFactor(float3 lightPos, float3 worldPos, float3 camPos)
{
	if (PhaseCoeff > 0.0)
	{
		float3 toCam = normalize(camPos - worldPos);
		float3 lightDirection = normalize(worldPos - lightPos);
		return lerp(1.0, Rayleigh(lightDirection, toCam), PhaseCoeff);
	}
	else
	{
		return 1.0;
	}
}

// --
// Conversion
// --

float3 ScreenToViewRay(float2 screen)
{
	float3 viewRay = multPoint(_VF_Inv_Proj, float3(screen, 1.0));
	return normalize(viewRay);
}

float PlaneDistanceToWorldDistance(float3 viewRay)
{
	return 1.0 / dot(viewRay, float3(0, 0, -1));
}

float WorldDistanceToPlaneDistance(float3 viewRay)
{
	return 1.0 / dot(viewRay, float3(0, 0, 1));
}

float ScreenZToPlaneDistance(float screenZ)
{
	float distance = pow(abs(screenZ), FogConversionExponent) * (FogFarClip - FogNearClip) + FogNearClip;
	return distance;
}

float3 ThreadIDToScreen(uint3 id)
{
	float3 screen = id;
	screen += 0.5f;
	screen *= float3(2.0 / _VF_VoxelDimension.x, 2.0 / _VF_VoxelDimension.y, 1.0 / _VF_VoxelDimension.z);
	screen += float3(-1.0, -1.0, 0.0);

	return screen;
}

float3 ThreadIDToScreenWithJitter(uint3 id)
{
	float3 screen = id;
	float3 jitter = HALTON_SEQUENCE[FrameCount % MAX_HALTON_SEQUENCE];
	jitter.xyz -= 0.5f;
	screen += jitter;
	screen += 0.5f;
	screen *= float3(2.0 / _VF_VoxelDimension.x, 2.0 / _VF_VoxelDimension.y, 1.0 / _VF_VoxelDimension.z);
	screen += float3(-1.0, -1.0, 0.0);

	return screen;
}

float3 ScreenToWorldPosition(float2 screen, float planeDistance)
{
	float3 viewRay = ScreenToViewRay(screen);
	float projection = PlaneDistanceToWorldDistance(viewRay);
	return multPoint(_VF_Inv_View, viewRay * planeDistance * projection);
}

float2 ScreenToUV(float2 screen)
{
	float2 uv = screen;
	uv += float2(1.0, 1.0);
	uv *= float2(0.5, 0.5);
	return uv;
}

float3 ScreenWithPlaneToUV(float2 screen, float planeDistance)
{
	float2 uv = ScreenToUV(screen);
	float z = pow(invLerp(FogNearClip, FogFarClip, planeDistance), 1 / FogConversionExponent);
	return float3(uv, z);
}

// --
// Scattering
// --

float SliceTickness(float2 screen, float z)
{
	float3 viewRay = ScreenToViewRay(screen);
	float projection = PlaneDistanceToWorldDistance(viewRay);
	float nextSliceDist = ScreenZToPlaneDistance(z + (1.0 / _VF_VoxelDimension.z)) * projection;
	float sliceDist = ScreenZToPlaneDistance(z) * projection;
	return nextSliceDist - sliceDist;
}

float4 ScatterStep(float3 accum, float accumTransmittance, float4 slice, float tickness)
{
	float sliceDensity = max(slice.a, 0.0000001);
	float sliceTransmittance = exp(-sliceDensity * tickness * ExtinctionCoeff); //Beer Law
	float3 sliceLightIntegral = slice.rgb * (1.0 - sliceTransmittance) / sliceDensity;

	accum += sliceLightIntegral * accumTransmittance;
	accumTransmittance *= sliceTransmittance;

	return float4(accum, accumTransmittance);
}

// --
// Sample
// --

float4 SampleFog(sampler3D volume, float2 uv, float planeDistance)
{
	float z = pow(invLerp(FogNearClip, FogFarClip, planeDistance), 1 / FogConversionExponent);
	return tex3D(volume, float3(uv, z));
}