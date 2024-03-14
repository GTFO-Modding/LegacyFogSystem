using UnityEngine;

namespace LegacyFogSystem.Systems;
internal sealed partial class LegacyLitVolume : MonoBehaviour
{
    public Color FogColor;
    public float FogAmbience;
    public float FogRange = 40.0f;
    public float LightAttenuation = 0.035f;
    public float DepthExponent = 2.0f;

    public float Density;
    public float DensityHeightMaxBoost;
    public float DensityHeight;
    public float DensityHeightRange;

    public Texture3D DensityNoise;
    public Vector3 DensityNoiseDirection;
    public float DensityNoiseSpeed;
    public float DensityNoiseScale;

    public ComputeShader CalcCS;
    public ComputeShader BlurCS;
    public Material BlendMat;

    private int _Kernel_Calc;
    private int _Kernel_Accumulate;

    private int _Kernel_Passthrough;
    private int _Kernel_GaussianBlur;

    private Camera _Cam;

    private void Start()
    {
        _Kernel_Calc = CalcCS.FindKernel("Calculate");
        _Kernel_Accumulate = CalcCS.FindKernel("Accumulate");

        _Kernel_Passthrough = BlurCS.FindKernel("Passthrough");
        _Kernel_GaussianBlur = BlurCS.FindKernel("GaussianBlur");

        _Cam = GetComponent<Camera>();

        Setup_ComputeBuffer();
        Setup_CalcBuffer();
        Setup_BlendBuffer();
    }

    private void Update()
    {
        Update_CheckRes();
        Update_ShaderProperties();
    }

    private void OnPreCull()
    {
        Update_LinkPreLitVolume();
        Update_CalcBuffer();
        Update_BlendBuffer();
    }

    private void OnPreRender()
    {
        Update_CollectLight();
    }

    private void OnDestroy()
    {
        Clear_ComputeBuffer();
        Clear_RenderTextures();
        Clear_CalcBuffer();
        Clear_BlendBuffer();
    }
}
