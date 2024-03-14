using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using static AmplifyOcclusionEffect;
using LegacyFogSystem.Injects;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    private CommandBuffer _CMD_Blend;

    private void Setup_BlendBuffer()
    {
        _CMD_Blend = new CommandBuffer()
        {
            name = "LitVolume Blend Cmd"
        };

        _Cam.AddCommandBuffer(CameraEvent.BeforeHaloAndLensFlares, _CMD_Blend);
    }

    private void Update_BlendBuffer()
    {
        if (_CMD_Blend == null || BlendMat == null)
            return;

        _CMD_Blend.Clear();

        _CMD_Blend.GetTemporaryRT(SID.TEMP_Grab, -1, -1, 24, FilterMode.Point, RenderTextureFormat.DefaultHDR);
        _CMD_Blend.Blit(BuiltinRenderTextureType.CameraTarget, SID.TEMP_Grab);
        _CMD_Blend.SetGlobalTexture("_MainTex", SID.TEMP_Grab);

        //TODO: IDEA: Extract Area of Thermal Scope from FPS item and use it as mask
        //Could be done by
        // - Grabbing Specific Renderer with Thermal Scope Shader.
        // - Draw on RenderTexture
        _CMD_Blend.Blit(SID.TEMP_Grab, BuiltinRenderTextureType.CameraTarget, BlendMat);
        _CMD_Blend.ReleaseTemporaryRT(SID.TEMP_Grab);
    }

    private void Clear_BlendBuffer()
    {
        if (_CMD_Blend != null)
        {
            _Cam.RemoveCommandBuffer(CameraEvent.BeforeHaloAndLensFlares, _CMD_Blend);
            _CMD_Blend.Dispose();
            _CMD_Blend = null;
        }
    }
}
