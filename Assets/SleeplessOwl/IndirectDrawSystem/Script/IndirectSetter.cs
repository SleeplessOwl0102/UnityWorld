using System;
using RenLai.IndirectDraw;
using Ren;
using UnityEngine;

[ExecuteAlways]
public class IndirectSetter : MonoBehaviour
{
    public InstanceObjectDrawConfig data;

    private void Update()
    {
        var system = IndirectDrawRenderPass.Instance;

        if (system.RenderCount == 0)
        {
            foreach (var item in data.instanceRenderSetting)
            {
                system.Add(item);
            }
        }
    }

    private void OnDisable()
    {
        var system = IndirectDrawRenderPass.Instance;
        foreach (var item in data.instanceRenderSetting)
        {
            system.Remove(item.description);
        }
    }
}