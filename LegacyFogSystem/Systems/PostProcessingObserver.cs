using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

namespace LegacyFogSystem.Systems;
internal sealed class PostProcessingObserver : MonoBehaviour
{
    public static PostProcessingPipeline Pipeline;

    private PostProcessLayer _Layer;
    private PostProcessingBehaviour _Behaviour;

    void Start()
    {
        if (Pipeline == PostProcessingPipeline.Unchanged)
        {
            Destroy(this);
            return;
        }

        _Layer = GetComponent<PostProcessLayer>();
        _Behaviour = GetComponent<PostProcessingBehaviour>();

        if (Pipeline == PostProcessingPipeline.Legacy)
        {
            if (_Layer != null) _Layer.enabled = false;
            if (_Behaviour != null) _Behaviour.profile.vignette.enabled = false;
        }
        else if (Pipeline == PostProcessingPipeline.Mixed)
        {
            if (_Layer != null) _Layer.enabled = false;
            if (_Behaviour != null) _Behaviour.enabled = true;
        }
    }

    void OnPreCull()
    {
        if (_Layer == null) return;
        if (_Behaviour == null) return;


        if (Pipeline == PostProcessingPipeline.Legacy)
        {
            _Behaviour.enabled = true;
            _Layer.enabled = true;
        }
        else if (Pipeline == PostProcessingPipeline.Mixed)
        {
            _Behaviour.enabled = true;
            _Layer.enabled = false;

            var settings = _Behaviour.m_EyeAdaptation.model.settings;
            settings.maxLuminance = -9.75f;
            _Behaviour.m_EyeAdaptation.model.settings = settings;
        }
    }
}
