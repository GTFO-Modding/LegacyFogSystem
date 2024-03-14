using LegacyFogSystem.Systems.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    private LitBuffer<sCL_SpotLight> _CB_SpotLight;
    private LitBuffer<sCL_PointLight> _CB_PointLight;
    private LitBuffer<sCL_PointLight> _CB_EffectLight;
    private LitBuffer<sCL_FogSphere> _CB_FogSphere;

    private void Setup_ComputeBuffer()
    {
        _CB_SpotLight = new (32);
        _CB_PointLight = new (32);
        _CB_EffectLight = new (32);
        _CB_FogSphere = new (32);
    }

    private void Clear_ComputeBuffer()
    {
        _CB_SpotLight?.Dispose();
        _CB_SpotLight = null;

        _CB_PointLight?.Dispose();
        _CB_PointLight = null;

        _CB_EffectLight?.Dispose();
        _CB_EffectLight = null;

        _CB_FogSphere?.Dispose();
        _CB_FogSphere = null;
    }
}
