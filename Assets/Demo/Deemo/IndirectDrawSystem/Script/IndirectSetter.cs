using RenLai.IndirectDraw;
using Ren;
using UnityEngine;

[ExecuteAlways]
public class IndirectSetter : MonoBehaviour
{
    public InstanceObjectDrawConfig data;

    private void Update()
    {
        if (IndirectDrawRenderPass.renderDatas == null)
            IndirectDrawRenderPass.renderDatas = data.instanceRenderSetting;
    }

    private void OnDisable()
    {
        IndirectDrawRenderPass.renderDatas = null;
    }


}
