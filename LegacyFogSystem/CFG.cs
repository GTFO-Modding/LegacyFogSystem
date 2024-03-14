using BepInEx.Configuration;
using LegacyFogSystem.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TenCC.Utils.ResilientTask;

namespace LegacyFogSystem;

internal enum PostProcessingPipeline
{
    Unchanged,
    Legacy,
    Mixed
}

internal static class CFG
{
    internal static void Init(ConfigFile cfg)
    {
        var reduceFog = cfg.Bind("User", "Reduce Fog Density", false, "Reduce Fog Density by 35% (Useful for R6+ Rundowns)");
        var shaderType = cfg.Bind("User", "Post Processing Pipeline", PostProcessingPipeline.Legacy, "Aka Shader Type to use // Unchanged is R6's lighting // Legacy is Old rundowns settings // Mixed is in between Default and Legacy");
        LegacyLitVolume.DensityMultiplier = reduceFog.Value ? 0.35f : 1.0f;
        PostProcessingObserver.Pipeline = shaderType.Value;

    }
}
