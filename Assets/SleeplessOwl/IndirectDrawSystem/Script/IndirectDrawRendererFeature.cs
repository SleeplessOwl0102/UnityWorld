using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenLai.IndirectDraw
{
    public class IndirectDrawRendererFeature : ScriptableRendererFeature
    {
        public ComputeShader cullingComputeShader;
        private IndirectDrawRenderPass afterSkyboxPass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(afterSkyboxPass);
        }

        public override void Create()
        {
            afterSkyboxPass = new IndirectDrawRenderPass(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            afterSkyboxPass.OnDispose();
        }
    }
}