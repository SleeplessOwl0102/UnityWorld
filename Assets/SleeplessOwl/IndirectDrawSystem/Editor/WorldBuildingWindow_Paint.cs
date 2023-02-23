using UnityEditor;
using UnityEngine;

namespace Ren
{

    public partial class WorldBuildingWindow
    {
        private void OnGUI_Paint()
        {
            data = (InstanceObjectDrawConfig)EditorGUILayout.ObjectField(data, typeof(InstanceObjectDrawConfig), true);

            if (data == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(isPainting ? "ON" : "OFF", GUILayout.Width(50)))
                    isPainting = !isPainting;

                targetLayer = EditorGUILayoutUtil.LayerMaskField("Target Layer :", targetLayer);
            }
            EditorGUILayout.EndHorizontal();

            DrawSelectIndexButton(data);
        }

        public void OnSceneView_Paint()
        {
            if (data == null)
                return;

            if (isPainting == false)
                return;

            Event e = Event.current;//检测输入
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            RaycastHit raycastHit;
            Ray terrain = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(terrain, out raycastHit, Mathf.Infinity, targetLayer))
            {
                DrawMouseCursor(raycastHit);
            }

            var t = e.GetTypeForControl(controlID);
            if (t == EventType.MouseMove)
            {
                HandleUtility.Repaint();
            }
            else if (t == EventType.MouseDrag && Event.current.button == 0)
            {
                //HandleUtility.Repaint();
                //data.onMouseDrag(raycastHit);

            }
            else if (t == EventType.MouseDown && Event.current.button == 0)
            {
                //data.onMouseDown(raycastHit);
                //HandleUtility.Repaint();
            }
        }
        private int curIndex = -1;

        public void DrawSelectIndexButton(InstanceObjectDrawConfig data)
        {
            for (int i = 0; i < data.instanceRenderSetting.Count; i++)
            {
                if (GUILayout.Button(data.instanceRenderSetting[i].description))
                {
                    curIndex = i;
                }
            }
        }

        public void DrawMouseCursor(RaycastHit raycastHit)
        {
            Handles.color = new Color(1f, 1f, 0f, 1f);//颜色
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, 1);//根据笔刷大小在鼠标位置显示一个圆
        }
    }
}