using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace LegacyFogSystem.Systems.Debugs;
internal sealed class ScreenDebug : MonoBehaviour
{
    public static RenderTexture RT;
    private CommandBuffer _CB;
    private Camera _Cam;
    private CameraEvent _CurrentEvent;
    private BuiltinRenderTextureType Target;

    void Start()
    {
        _Cam = GetComponent<Camera>();
        RT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);
        _CB = new CommandBuffer();
        _CB.name = "Debug CB";
    }

    private void AddBuffer(CameraEvent evt)
    {
        _CB.Clear();
        _CB.Blit(Target, RT);

        _Cam.RemoveCommandBuffer(_CurrentEvent, _CB);
        _Cam.AddCommandBuffer(evt, _CB);
        _CurrentEvent = evt;
    }

    void OnGUI()
    {
        if (RT == null) return;

        GUI.DrawTexture(new Rect(0.0f, 0.0f, 256.0f, 256.0f * (9.0f / 16.0f)), RT, ScaleMode.ScaleToFit);
    }
}
