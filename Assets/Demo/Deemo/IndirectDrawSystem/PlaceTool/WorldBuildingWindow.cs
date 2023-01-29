#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Ren
{

    public partial class WorldBuildingWindow : EditorWindow
    {

        public LayerMask targetLayer;
        public InstanceObjectDrawConfig data;
        public bool isPainting = false;

        public WorldBuildingWindow instance;

        public enum ToolType
        {
            Paint,
            Generator,
        }
        string[] toolTypeStrings = { "Paint", "Generator" };
        ToolType toolType = 0;

        [MenuItem("RenTool/Owl-World Builder")]
        static void Init()
        {
            var window = GetWindow(typeof(WorldBuildingWindow));
            window.Show();

        }

        private void OnEnable()
        {
            instance = (WorldBuildingWindow)GetWindow(typeof(WorldBuildingWindow));
            targetLayer = 1 << LayerMask.NameToLayer("Terrain");
            SceneView.duringSceneGui += DrawSceneViewSequence;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneViewSequence;
        }

        private void OnGUI()
        {
            toolType = (ToolType)GUILayout.Toolbar((int)toolType, toolTypeStrings);

            switch (toolType)
            {
                case ToolType.Paint:
                    OnGUI_Paint();
                    break;
                case ToolType.Generator:
                    break;
                default:
                    break;
            }
        }

        private void DrawSceneViewSequence(SceneView obj)
        {
            switch (toolType)
            {
                case ToolType.Paint:
                    OnSceneView_Paint();
                    break;
                case ToolType.Generator:
                    break;
                default:
                    break;
            }
        }
        
    }
}
#endif