using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace LegacyFogSystem;
internal static class CommandBufferExtensions
{
    public static void DispatchCompute(this CommandBuffer cmd, ComputeShader cs, int kernel, int[] groupCount)
    {
        cmd.DispatchCompute(cs, kernel, groupCount[0], groupCount[1], groupCount[2]);
    }
}
