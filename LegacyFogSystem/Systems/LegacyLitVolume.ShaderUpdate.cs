using SNetwork_Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    private Vector3 _NoiseOffset;
    private float _Coeff_Noise = 0.75f;
    private float _Coeff_Phase = 0.0f;
    private float _Coeff_Absorbtion = 0.087f;
    private float _Coeff_Extinction = 0.5f;

    private void UpdateOnce_OnResChange()
    {
        var dim = new Vector3(_VoxelWidth, _VoxelHeight, _VoxelDepth);
        CalcCS.SetVector(SID.VF_VoxelDimension, dim);
        CalcCS.SetTexture(_Kernel_Calc, SID.DensityNoiseTexture, DensityNoise);
        CalcCS.SetTexture(_Kernel_Calc, "_FogVolume_Reblend", _RT_FrustumVoxel_Reblend);
        CalcCS.SetTexture(_Kernel_Calc, SID.FogVolume_Body, _RT_FrustumVoxel_Body);
        CalcCS.SetTexture(_Kernel_Accumulate, SID.FogVolume_Body, _RT_FrustumVoxel_Body);
        CalcCS.SetTexture(_Kernel_Accumulate, SID.FogVolume_Accumulate, _RT_FrustumVoxel_Accumulate);
        BlurCS.SetVector("_Dimension", dim);

        BlendMat.SetVector(SID.VF_VoxelDimension, dim);
        BlendMat.SetTexture(SID.VF_Volume, _RT_FrustumVoxel_Accumulate);
    }

    private void Update_ShaderProperties()
    {
        Vector4 absorbtion;
        absorbtion.x = 1f - FogColor.r;
        absorbtion.y = 1f - FogColor.g;
        absorbtion.z = 1f - FogColor.b;

        var lowDensity = Mathf.Min(Density, DensityHeightMaxBoost);
        absorbtion.w = FogColor.a * (1f - lowDensity) + lowDensity;

        _NoiseOffset += DensityNoiseSpeed * Time.deltaTime * DensityNoiseDirection;
        _NoiseOffset.x %= 1.0f;
        _NoiseOffset.y %= 1.0f;
        _NoiseOffset.z %= 1.0f;

        CalcCS.SetFloat(SID.LightAttenuation, LightAttenuation);
        CalcCS.SetVector(SID.FogColorAbsorbtion, absorbtion);
        CalcCS.SetVector(SID.DensityParams, new Vector4(Density, DensityHeightMaxBoost, DensityHeight, DensityHeightRange));
        CalcCS.SetVector(SID.DensityNoiseParams, new Vector4(_NoiseOffset.x, _NoiseOffset.y, _NoiseOffset.z, DensityNoiseScale));
        CalcCS.SetVector(SID.VF_ScatteringParams, new Vector4(_Coeff_Noise, _Coeff_Phase, absorbtion.w * _Coeff_Absorbtion, _Coeff_Extinction));

        var atlas = CL_ShadowAtlas.AtlasTexture;
        if (atlas != null)
        {
            var atlasCellDimension = Vector2.one * CL_ShadowAtlas.cShadowResolution;
            CalcCS.SetTexture(_Kernel_Calc, SID.SA_AtlasTexture, atlas);
            CalcCS.SetVector(SID.SA_AtlasCellDim, atlasCellDimension);
        }
    }
}

