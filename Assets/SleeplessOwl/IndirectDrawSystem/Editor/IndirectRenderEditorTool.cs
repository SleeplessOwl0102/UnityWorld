using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace RenLai.IndirectDraw
{
    public class IndirectRenderEditorTool
    {

        //版本更新後好像不需要了
        [Shortcut("World Creator/Re Create Buffer", KeyCode.W, ShortcutModifiers.Shift)]
        private static void ForceRefreshRenderPass()
        {
            IndirectDrawRenderPass.ForceRefresh = true;
        }
        [Shortcut("World Creator/ForceDisable", KeyCode.E, ShortcutModifiers.Shift)]
        private static void ForceDisable()
        {
            IndirectDrawRenderPass.ForceDisable = !IndirectDrawRenderPass.ForceDisable;
            if(IndirectDrawRenderPass.ForceDisable == false)
            {
                IndirectDrawRenderPass.ForceRefresh = true;
            }
        }
    }
}
