Shader "Hidden/LegacyLitVolume/BlendFog"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "white" {}
        _VF_Volume ("Volume", 3D) = "white" {}
    }
    SubShader
    {
        Cull Off Blend Off ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "CGINC/VolumetricFog.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _FPSMask;

            sampler3D _VF_Volume;
            float4 _VF_Volume_TexelSize;
            float4 _FogColor;
            
            sampler2D _CameraDepthTexture;

            float4 frag (v2f i) : SV_Target 
            {
                float planeDistance = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
                float2 uv = i.uv; 
                float2 inv = float2(1.0 / _VF_VoxelDimension.x, 1.0 / _VF_VoxelDimension.y);
                
                float4 fog = SampleFog(_VF_Volume, i.uv, planeDistance); 
                float4 grab = tex2D(_MainTex, i.uv);
                return (grab * fog.a) + fog;
            }

            ENDCG
        }
    }
}
