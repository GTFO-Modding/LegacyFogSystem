using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LegacyFogSystem.Injects;
[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Awake))]
internal static class Inject_Reorder_UI_Apply
{
    private static void Postfix(PlayerManager __instance)
    {
        var localPrefab = __instance.m_prefabLocal;
        var fpsCameraObj = localPrefab.GetComponentInChildren<FPSCamera>(includeInactive: true).gameObject;
        var oldUIApply = fpsCameraObj.GetComponent<UI_Apply>();
        var newUIApply = fpsCameraObj.AddComponent<UI_Apply>();
        newUIApply.m_UIBlitterMaterial = oldUIApply.m_UIBlitterMaterial;
        UnityEngine.Object.Destroy(oldUIApply);
    }
}
