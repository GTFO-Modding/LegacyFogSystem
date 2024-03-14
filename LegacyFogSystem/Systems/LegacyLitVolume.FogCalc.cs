using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    private CommandBuffer _CMD_Calc;
    private Matrix4x4 _PrevInvProjection;
    private Matrix4x4 _PrevInvView;
    private float _HasPrevFrame = 0.0f;
    private float _ReprojectionBlend = 0.0f;

    private void Setup_CalcBuffer()
    {
        _CMD_Calc = new CommandBuffer()
        {
            name = "LitVolume Calc Cmd"
        };

        _Cam.AddCommandBuffer(CameraEvent.AfterGBuffer, _CMD_Calc);
    }

    private void Update_CalcBuffer()
    {
        if (_CMD_Calc == null || _RT_FrustumVoxel_Body == null)
            return;

        _CMD_Calc.Clear();

        UpdateParams(_CMD_Calc);

        //Calculate
        _CMD_Calc.SetComputeBufferParam(CalcCS, _Kernel_Calc, SID.CB_PointLight, _CB_PointLight.CB);
        _CMD_Calc.SetComputeBufferParam(CalcCS, _Kernel_Calc, SID.CB_EffectLight, _CB_EffectLight.CB);
        _CMD_Calc.SetComputeBufferParam(CalcCS, _Kernel_Calc, SID.CB_SpotLight, _CB_SpotLight.CB);
        _CMD_Calc.SetComputeBufferParam(CalcCS, _Kernel_Calc, SID.CB_FogSphere, _CB_FogSphere.CB);
        _CMD_Calc.DispatchCompute(CalcCS, _Kernel_Calc, _Calc_GroupCount);
        _CMD_Calc.CopyTexture(_RT_FrustumVoxel_Body, _RT_FrustumVoxel_Reblend);

        var previousVP = GL.GetGPUProjectionMatrix(_Cam.projectionMatrix, false) * _Cam.worldToCameraMatrix;
        _CMD_Calc.SetComputeMatrixParam(CalcCS, SID.VF_Prev_VP, previousVP);

        //Accumulate
        _CMD_Calc.DispatchCompute(CalcCS, _Kernel_Accumulate, _Accum_GroupCount);

        //Blur
        _CMD_Calc.GetTemporaryRT(SID.TEMP_BlurCopy, _RTD_Froxel, FilterMode.Trilinear);

        _CMD_Calc.CopyTexture(_RT_FrustumVoxel_Accumulate, SID.TEMP_BlurCopy);
        _CMD_Calc.SetComputeTextureParam(BlurCS, _Kernel_Passthrough, SID.Input, SID.TEMP_BlurCopy);
        _CMD_Calc.SetComputeTextureParam(BlurCS, _Kernel_Passthrough, SID.Output, _RT_FrustumVoxel_Accumulate);
        _CMD_Calc.DispatchCompute(BlurCS, _Kernel_Passthrough, _Blur_GroupCount);

        _CMD_Calc.CopyTexture(_RT_FrustumVoxel_Accumulate, SID.TEMP_BlurCopy);
        _CMD_Calc.SetComputeTextureParam(BlurCS, _Kernel_Passthrough, SID.Input, SID.TEMP_BlurCopy);
        _CMD_Calc.SetComputeTextureParam(BlurCS, _Kernel_Passthrough, SID.Output, _RT_FrustumVoxel_Accumulate);
        _CMD_Calc.DispatchCompute(BlurCS, _Kernel_Passthrough, _Blur_GroupCount);

        _CMD_Calc.ReleaseTemporaryRT(SID.TEMP_BlurCopy);
    }

    private void Clear_CalcBuffer()
    {
        if (_CMD_Calc != null)
        {
            _Cam.RemoveCommandBuffer(CameraEvent.AfterGBuffer, _CMD_Calc);
            _CMD_Calc.Dispose();
            _CMD_Calc = null;
        }
    }

    private void UpdateParams(CommandBuffer cmd)
    {
        var near = _Cam.nearClipPlane;
        var far = _Cam.farClipPlane;
        far = Mathf.Clamp(FogRange, near, far);

        Matrix4x4 projMtx = Matrix4x4.Perspective(_Cam.fieldOfView, _Cam.aspect, near, far);
        Matrix4x4 invProjMtx = projMtx.inverse;
        Matrix4x4 invViewMtx = _Cam.cameraToWorldMatrix;

        var param = new Vector4(near, far, Time.frameCount, DepthExponent);
        BlendMat.SetVector(SID.VF_FogParams, param);
        cmd.SetComputeVectorParam(CalcCS, SID.VF_FogParams, param);
        cmd.SetComputeVectorParam(CalcCS, SID.VF_ReprojectionParams, new Vector4(_ReprojectionBlend, 1.0f, _HasPrevFrame, _ReprojectionBlend > 0.0f ? Time.frameCount : 0.0f));
        cmd.SetComputeMatrixParam(CalcCS, SID.VF_Inv_Proj, invProjMtx);
        cmd.SetComputeMatrixParam(CalcCS, SID.VF_Inv_View, invViewMtx);
        BlendMat.SetMatrix(SID.VF_Inv_Proj, invProjMtx);
        BlendMat.SetMatrix(SID.VF_Inv_View, invViewMtx);
        cmd.SetComputeVectorParam(CalcCS, SID.VF_CameraWorldPos, _Cam.transform.position);

        _PrevInvProjection = invProjMtx;
        _PrevInvView = invViewMtx;
        _HasPrevFrame = 1.0f;
    }
}
