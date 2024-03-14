using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Injects;
[HarmonyPatch(typeof(LocalPlayerAgent), nameof(LocalPlayerAgent.Setup))]
internal static class Inject_ModifyAmbient
{
    private static void Postfix(LocalPlayerAgent __instance)
    {
        var ambient = __instance.m_ambientLight;
        if (ambient != null)
        {
            ambient.Intensity = 0.12f;
            ambient.Range = 1.62f;
        }
    }
}
