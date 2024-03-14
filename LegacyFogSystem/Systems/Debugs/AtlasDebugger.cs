using GTFO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems.Debugs;
internal sealed class AtlasDebugger : MonoBehaviour
{

    private Material _DEBUGMat;
    private RenderTexture _RT;

    private void Start()
    {
        _DEBUGMat = AssetAPI.GetLoadedAsset<Material>("Assets/LegacyLitVolume/Mats/DebugAtlas.mat");
        _RT = new RenderTexture(512, 256, 0, RenderTextureFormat.RGHalf);
        _RT.Create();
    }

    private void OnPostRender()
    {
        if (CL_ShadowAtlas.AtlasTexture != null)
        {
            var tex = CL_ShadowAtlas.AtlasTexture;
            Graphics.Blit(tex, _RT, _DEBUGMat);
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0.0f, 0.0f, 512, 256), _RT);
    }
}
