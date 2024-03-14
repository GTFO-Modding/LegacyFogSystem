using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RootMotion.FinalIK.AimPoser;

namespace LegacyFogSystem.Systems.Buffers;
internal unsafe class LitBuffer<T> where T : struct
{
    public readonly int MaxSize;
    public readonly int Stride;

    public int Size { get; private set; }
    public ComputeBuffer CB { get; private set; }
    public T[] TempBuffer { get; private set; }

    private readonly Il2CppSystem.Array _Buffer;
    private readonly IntPtr _ClassPtr;

    public LitBuffer(int size)
    {
        _Buffer = Il2CppSystem.Array.CreateInstance(Il2CppType.Of<T>(), size);
        _ClassPtr = Il2CppClassPointerStore<T>.NativeClassPtr;

        var stride = Marshal.SizeOf<T>();
        MaxSize = size;
        Size = 0;
        Stride = stride;

        CB = new ComputeBuffer(size, stride);
        TempBuffer = new T[size];
    }

    public void ClearBuffer()
    {
        for (int i = 0; i<MaxSize; i++)
        {
            var defaultT = new T();
            var ptr = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref defaultT);
            var obj = new Il2CppSystem.Object(IL2CPP.il2cpp_value_box(_ClassPtr, ptr));
            _Buffer.SetValue(obj, i);
            TempBuffer[i] = defaultT;
        }
    }

    public void PushTempBuffer(int size = -1)
    {
        Push(TempBuffer, size);
    }

    public void Push(T[] buffers, int size = -1)
    {
        if (CB == null)
            return;

        int length = Mathf.Min(Mathf.Min(buffers.Length, size > 0 ? size : int.MaxValue), MaxSize);
        Size = length;
        for (int i = 0; i<length; i++)
        {
            var ptr = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref buffers[i]);
            var obj = new Il2CppSystem.Object(IL2CPP.il2cpp_value_box(_ClassPtr, ptr));
            _Buffer.SetValue(obj, i);
        }
        
        CB.SetData(_Buffer, 0, 0, MaxSize);
    }

    public void Dispose()
    {
        if (CB != null)
        {
            CB.Dispose();
            CB = null;
        }
    }
}
