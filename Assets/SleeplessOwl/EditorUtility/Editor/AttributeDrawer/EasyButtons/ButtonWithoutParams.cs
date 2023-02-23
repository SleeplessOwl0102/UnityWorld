namespace EasyButtons.Editor
{
    using SleeplessOwl.EditorUtil.Mono;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    internal class ButtonWithoutParams : Button
    {
        public ButtonWithoutParams(MethodInfo method, ButtonAttribute buttonAttribute)
            : base(method, buttonAttribute) { }

        protected override void DrawInternal(IEnumerable<object> targets)
        {
            if (!GUILayout.Button(DisplayName))
                return;

            foreach (object obj in targets)
            {
                Method.Invoke(obj, null);
            }
        }
    }
}