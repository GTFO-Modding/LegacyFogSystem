using CullingSystem;
using FX_EffectSystem;
using LegacyFogSystem.Systems.Buffers;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    public static float DensityMultiplier = 1.0f;

    public PreLitVolume PreLit;

    public float ActualSpotlightIrr = 1.0f;
    public float ActualPointlightIrr = 2.25f;
    public float ActualEffectlightIrr = 2.5f;
    public float ActualSpotlightRange = 1f;
    public float ActualSpotlightAngle = 1f;
    public float ActualPointlightRange = 1f;
    public float ActualEffectlightRange = 1.0f;
    public float ActualFogSphereIrr = 150.0f;

    private void Update_LinkPreLitVolume()
    {
        if (PreLit == null)
            return;

        FogColor = PreLit.m_fogColor;
        FogAmbience = PreLit.m_fogAmbience;

        Density = PreLit.m_fogDensity * DensityMultiplier;
        DensityHeight = PreLit.m_densityHeightAltitude;
        DensityHeightRange = PreLit.m_densityHeightRange;
        DensityHeightMaxBoost = PreLit.m_densityHeightMaxBoost * DensityMultiplier;
        DensityNoiseDirection = PreLit.m_densityNoiseDirection;
        DensityNoiseScale = PreLit.m_densityNoiseScale;
        DensityNoiseSpeed = PreLit.m_densityNoiseSpeed;
    }

    private void Update_CollectLight()
    {
        _CB_PointLight.ClearBuffer();
        _CB_EffectLight.ClearBuffer();
        _CB_SpotLight.ClearBuffer();
        _CB_FogSphere.ClearBuffer();

        int count = 0;
        foreach (var light in C_CullingManager.VisiblePointLights)
        {
            if (count >= _CB_PointLight.MaxSize) break;
            if (!light.IsShown) continue;
            if (!light.IsRegistered) continue;

            var lightComp = light.m_unityLight;
            var range = lightComp.range * ActualPointlightRange;
            var radiance = new Vector3(lightComp.color.r, lightComp.color.g, lightComp.color.b);
            radiance *= lightComp.intensity * ActualPointlightIrr;
            _CB_PointLight.TempBuffer[count] = new()
            {
                Position = lightComp.transform.position,
                Irradiance = radiance,
                InvRangeSqr = 1.0f / (range * range),
                Physical = 0.0f,
            };
            count++;
        }
        _CB_PointLight.PushTempBuffer(count);

        count = 0;
        foreach (var light in ClusteredRendering.s_visibleEffectLights)
        {
            if (count >= _CB_EffectLight.MaxSize) break;
            if (!light.IsVisible) continue;
            if (!light.enabled) continue;
            if (!light.WantsToShow) continue;

            var color = light.Color;
            var radiance = new Vector3(color.r, color.g, color.b);
            radiance *= light.Intensity * ActualEffectlightIrr;
            var range = light.Range * ActualEffectlightRange;
            _CB_EffectLight.TempBuffer[count] = new()
            {
                Position = light.transform.position,
                Irradiance = radiance,
                InvRangeSqr = 1.0f / (range * range),
                Physical = light.Physical
            };
            count++;
        }
        _CB_EffectLight.PushTempBuffer(count);

        count = 0;
        foreach (var light in C_CullingManager.VisibleSpotLights)
        {
            if (count >= _CB_SpotLight.MaxSize) break;
            if (!light.IsShown) continue;
            if (!light.IsRegistered) continue;

            var lightComp = light.m_unityLight;
            var range = lightComp.range * ActualSpotlightRange;
            var angle = lightComp.spotAngle * ActualSpotlightAngle;
            var radiance = new Vector3(lightComp.color.r, lightComp.color.g, lightComp.color.b);
            radiance *= lightComp.intensity * ActualSpotlightIrr;

            var shadowGenerator = light.m_shadowGenerator;
            var proj = Matrix4x4.Perspective(angle, shadowGenerator.m_aspect, shadowGenerator.m_zNear, shadowGenerator.m_zFar);
            proj = GL.GetGPUProjectionMatrix(proj, true);

            var view = Matrix4x4.TRS(lightComp.transform.position, lightComp.transform.rotation, new Vector3(1, 1, -1)).inverse;
            var vp = proj * view;

            _CB_SpotLight.TempBuffer[count] = new()
            {
                Position = lightComp.transform.position,
                Irradiance = radiance,
                InvRangeSqr = 1.0f / (range * range),
                ViewProjection = vp,
                InvNearRange = shadowGenerator.InvNearRange,
                ShadowIndex = shadowGenerator.ShadowID
            };
            count++;
        }
        _CB_SpotLight.PushTempBuffer(count);

        count = 0;
        foreach (var fogSphere in PreLitVolume.s_activeFogSpheres)
        {
            if (count >= _CB_FogSphere.MaxSize) break;

            var fogData = fogSphere.m_data;
            fogData.Radiance *= ActualFogSphereIrr;
            fogData.Density *= DensityMultiplier;
            _CB_FogSphere.TempBuffer[count] = fogData;
            count++;
        }
        _CB_FogSphere.PushTempBuffer(count);
    }
}
