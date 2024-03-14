using BepInEx;
using BepInEx.Unity.IL2CPP;
using Enemies;
using GTFO.API;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using LegacyFogSystem.Systems;
using LegacyFogSystem.Systems.Debugs;
using Player;
using System.Linq;
using UnityEngine;

namespace LegacyFogSystem;
[BepInPlugin("LegacyFogSystem", "LegacyFogSystem", VersionInfo.Version)]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
internal class EntryPoint : BasePlugin
{
    private Harmony _Harmony = null;

    public override void Load()
    {
        ClassInjector.RegisterTypeInIl2Cpp<LegacyLitVolume>();
        ClassInjector.RegisterTypeInIl2Cpp<AtlasDebugger>();
        //ClassInjector.RegisterTypeInIl2Cpp<ScreenDebug>();
        ClassInjector.RegisterTypeInIl2Cpp<PostProcessingObserver>();

        _Harmony = new Harmony($"{VersionInfo.RootNamespace}.Harmony");
        _Harmony.PatchAll();

        CFG.Init(Config);
    }

    public override bool Unload()
    {
        _Harmony.UnpatchSelf();
        return base.Unload();
    }
}