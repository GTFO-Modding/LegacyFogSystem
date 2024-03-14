using GTFO.API;
using HarmonyLib;
using LegacyFogSystem.Systems;
using LegacyFogSystem.Systems.Debugs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace LegacyFogSystem.Injects;
[HarmonyPatch(typeof(ClusteredRendering), nameof(ClusteredRendering.Setup))]
internal class Inject_SetupLegacyPreLit
{
    static void Postfix(ClusteredRendering __instance)
    {
        var prelit = __instance.GetComponent<PreLitVolume>();
        if (prelit == null)
        {
            Logger.Error("PreLitVolume is missing?!");
            return;
        }

        RenderPipe.Config.smoothnessLimit = 1.0f;
        ShaderIDs.VolumesEnabled = Shader.PropertyToID("_NoDontAlterTheVolumeSettingsThankYou");
        Shader.SetGlobalFloat("_VolumesEnabled", 0.0f);

        __instance.UpdateLitVolume = false;
        __instance.GetComponent<AmplifyOcclusionEffect>().Intensity = 0.0f;

        var litVolume = __instance.GetComponent<LegacyLitVolume>();
        if (litVolume == null)
        {
            litVolume = __instance.gameObject.AddComponent<LegacyLitVolume>();
            litVolume.PreLit = prelit;
            litVolume.DensityNoise = prelit.m_densityNoise;
            litVolume.CalcCS = AssetAPI.GetLoadedAsset<ComputeShader>("Assets/LegacyLitVolume/Shaders/CalcLitVolume.compute");
            litVolume.BlurCS = AssetAPI.GetLoadedAsset<ComputeShader>("Assets/LegacyLitVolume/Shaders/BlurLitVolume.compute");
            litVolume.BlendMat = AssetAPI.GetLoadedAsset<Material>("Assets/LegacyLitVolume/Mats/BlendFog.mat");

            //__instance.gameObject.AddComponent<ScreenDebug>();
            //__instance.gameObject.AddComponent<AtlasDebugger>();
            __instance.gameObject.AddComponent<PostProcessingObserver>();
        }
    }
}
