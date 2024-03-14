using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal static class SID
{
    public static readonly int FogColorAbsorbtion = ID("_FogColorAbsorbtion");
    public static readonly int LightAttenuation = ID("_LightAttenuation");

    public static readonly int DensityParams = ID("_DensityParams");
    public static readonly int DensityNoiseParams = ID("_DensityNoiseParams");
    public static readonly int DensityNoiseTexture = ID("_DensityNoiseTexture");

    public static readonly int FogVolume_Body = ID("_FogVolume_Body");
    public static readonly int FogVolume_Accumulate = ID("_FogVolume_Accumulate");

    public static readonly int CB_PointLight = ID("_CB_PointLight");
    public static readonly int CB_EffectLight = ID("_CB_EffectLight");
    public static readonly int CB_SpotLight = ID("_CB_SpotLight");
    public static readonly int CB_FogSphere = ID("_CB_FogSphere");

    public static readonly int VF_VoxelDimension = ID("_VF_VoxelDimension");

    public static readonly int VF_Inv_Proj = ID("_VF_Inv_Proj");
    public static readonly int VF_Inv_View = ID("_VF_Inv_View");
    public static readonly int VF_Prev_VP = ID("_VF_Prev_VP");
    public static readonly int VF_FogParams = ID("_VF_FogParams");
    public static readonly int VF_ReprojectionParams = ID("_VF_ReprojectionParams");
    public static readonly int VF_ScatteringParams = ID("_VF_ScatteringParams");
    public static readonly int VF_CameraWorldPos = ID("_VF_CameraWorldPos");

    public static readonly int VF_Volume = ID("_VF_Volume");

    public static readonly int SA_AtlasTexture = ID("_SA_AtlasTexture");
    public static readonly int SA_AtlasCellDim = ID("_SA_AtlasCellDimension");

    public static readonly int TEMP_Grab = ID("_TMP_Grab");
    public static readonly int TEMP_BlurCopy = ID("_TMP_BlurCopy");
    public static readonly int TEMP_Blurry = ID("_TMP_Blurry");

    public static readonly int Input = ID("_Input");
    public static readonly int Output = ID("_Output");


    private static int ID(string name) => Shader.PropertyToID(name);
}
