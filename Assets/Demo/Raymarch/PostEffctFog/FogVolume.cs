
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SleeplessOwl.URPPostProcessing.Indev
{
    [Serializable, VolumeComponentMenu("Custom Post-processing/PostFog")]
    public sealed class FogVolume : PostProcessVolumeComponent
    {
        public BoolParameter Active = new BoolParameter(false);

        Material material;
        Vector3[] frustumCorners;

        public override InjectionPoint InjectionPoint => InjectionPoint.BeforePostProcess;
        public override bool IsActive() => Active.value;

        public override void Initialize()
        {
            material = CoreUtils.CreateEngineMaterial("Raymarch/PostFog");
            frustumCorners = new Vector3[4];
        }

        public override void Render(CommandBuffer cb, Camera camera, RenderTargetIdentifier source, RenderTargetIdentifier destination)
        {
            cb.SetGlobalMatrix("_WorldToCam", camera.worldToCameraMatrix);
            cb.SetGlobalMatrix("_InverseView", camera.cameraToWorldMatrix);
            
            cb.SetGlobalVector("_CamWorldSpace", camera.transform.position);

            cb.SetGlobalTexture("_PostSource", source);
            //cb.DrawMeshFullScreen(camera, material);
            cb.DrawFullScreenTriangle(material, destination);
        }

    }

}