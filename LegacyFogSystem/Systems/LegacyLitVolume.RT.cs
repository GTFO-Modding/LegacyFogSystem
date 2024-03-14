using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    private RenderTexture _RT_FrustumVoxel_Body;
    private RenderTexture _RT_FrustumVoxel_Reblend;
    private RenderTexture _RT_FrustumVoxel_Accumulate;
    private RenderTextureDescriptor _RTD_Froxel;

    private int _VoxelDepth;
    private int _VoxelWidth;
    private int _VoxelHeight;
    private readonly int[] _Calc_GroupCount = new int[3];
    private readonly int[] _Blur_GroupCount = new int[3];
    private readonly int[] _Accum_GroupCount = new int[3];

    private int _CurrentFogQuality = -1;
    private Resolution _CurrentRes;
    private int _ScrWidth;
    private int _ScrHeight;

    

    private void Update_CheckRes()
    {
        var res = Screen.currentResolution;
        var fogQuality = CellSettingsManager.GetIntValue(eCellSettingID.Video_FogQuality);
        if (res.width != _CurrentRes.width || res.height != _CurrentRes.height || fogQuality != _CurrentFogQuality)
        {
            _CurrentFogQuality = fogQuality;
            _CurrentRes = res;
            _ScrWidth = res.width;
            _ScrHeight = res.height;
            Clear_RenderTextures();
            Create_RenderTextures();
            UpdateOnce_OnResChange();
        }
    }

    private void Create_RenderTextures()
    {
        int depth;
        int voxelFactor;
        switch (_CurrentFogQuality)
        {
            case 0:
                voxelFactor = 16;
                depth = 96;
                break;

            case 1:
                voxelFactor = 16;
                depth = 112;
                break;

            case 2:
                voxelFactor = 12;
                depth = 112;
                break;

            case 3:
                voxelFactor = 12;
                depth = 128;
                break;

            default:
                voxelFactor = 8;
                depth = 128;
                break;
        }

        _VoxelWidth = _ScrWidth / voxelFactor;
        _VoxelHeight = _ScrHeight / voxelFactor;
        _VoxelDepth = depth;

        CalcCS.GetKernelThreadGroupSizes(_Kernel_Calc, out uint x, out uint y, out uint z);
        _Calc_GroupCount[0] = Mathf.CeilToInt(Mathf.Max(1, _VoxelWidth / (float)x));
        _Calc_GroupCount[1] = Mathf.CeilToInt(Mathf.Max(1, _VoxelHeight / (float)y));
        _Calc_GroupCount[2] = Mathf.CeilToInt(Mathf.Max(1, _VoxelDepth / (float)z));

        CalcCS.GetKernelThreadGroupSizes(_Kernel_Accumulate, out x, out y, out _);
        _Accum_GroupCount[0] = Mathf.CeilToInt(Mathf.Max(1, _VoxelWidth / (float)x));
        _Accum_GroupCount[1] = Mathf.CeilToInt(Mathf.Max(1, _VoxelHeight / (float)y));
        _Accum_GroupCount[2] = 1;

        BlurCS.GetKernelThreadGroupSizes(_Kernel_Passthrough, out x, out y, out z);
        _Blur_GroupCount[0] = Mathf.CeilToInt(Mathf.Max(1, _VoxelWidth / (float)x));
        _Blur_GroupCount[1] = Mathf.CeilToInt(Mathf.Max(1, _VoxelHeight / (float)y));
        _Blur_GroupCount[2] = Mathf.CeilToInt(Mathf.Max(1, _VoxelDepth / (float)z));

        _RTD_Froxel = new RenderTextureDescriptor()
        {
            width = _VoxelWidth,
            height = _VoxelHeight,
            volumeDepth = _VoxelDepth,
            msaaSamples = 1,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            useMipMap = false,
            enableRandomWrite = true,
            colorFormat = RenderTextureFormat.ARGBHalf
        };

        //RGB - Scattering
        _RT_FrustumVoxel_Body = new RenderTexture(_RTD_Froxel)
        {
            name = "RT LLV Froxel Body",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Trilinear
        };
        _RT_FrustumVoxel_Body.Create();

        _RT_FrustumVoxel_Reblend = new RenderTexture(_RTD_Froxel)
        {
            name = "RT LLV Froxel Reblend History",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Trilinear
        };
        _RT_FrustumVoxel_Reblend.Create();

        //RGB - Scattered Result
        //A - Transmittance
        _RT_FrustumVoxel_Accumulate = new RenderTexture(_RTD_Froxel)
        {
            name = "RT LLV Froxel Accum",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Trilinear
        };
        _RT_FrustumVoxel_Accumulate.Create();
    }

    private void Clear_RenderTextures()
    {
        if (_RT_FrustumVoxel_Body != null)
        {
            _RT_FrustumVoxel_Body.Release();
            _RT_FrustumVoxel_Body = null;
        }

        if (_RT_FrustumVoxel_Reblend != null)
        {
            _RT_FrustumVoxel_Reblend.Release();
            _RT_FrustumVoxel_Reblend = null;
        }

        if (_RT_FrustumVoxel_Accumulate != null)
        {
            _RT_FrustumVoxel_Accumulate.Release();
            _RT_FrustumVoxel_Accumulate = null;
        }
    }
}
