using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RL.EditorCore
{
    public class MenuTreeWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private VisualElement leftView;
        private VisualElement rightView;
        private MenuTreeView treeView;
        private ToolbarSearchField searchField;

        [MenuItem("Window/UI Toolkit/MenuTreeWindow")]
        public static void ShowExample()
        {
            MenuTreeWindow wnd = CreateWindow<MenuTreeWindow>("MenuTreeWindow");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            rootVisualElement.style.flexDirection = FlexDirection.Row;

            leftView = new VisualElement();
            leftView.style.maxWidth = 200;
            leftView.style.width = 200;
            leftView.style.borderRightColor = Color.gray;
            leftView.style.borderRightWidth = 1;

            root.Add(leftView);
            searchField = new ToolbarSearchField();
            searchField.style.width = StyleKeyword.Auto;
            leftView.Add(searchField);

            rightView = new VisualElement();
            rightView.style.flexGrow = 1;
            root.Add(rightView);

            treeView = new MenuTreeView(null);
            leftView.Add(treeView);

            treeView.onSelectionChange += HandleSelectionChange;
            InitializeTreeViewData();

            treeView.Rebuild();
        }

        private void HandleSelectionChange(IEnumerable<object> obj)
        {
            foreach (var item in obj)
            {
                if (item is MenuItemData data)
                {
                    for (int i = 0; i < rightView.childCount; i++)
                    {
                        rightView.RemoveAt(i);

                    }

                    rightView.Add(data.GetVisualElement);
                }
            }
        }

        public virtual void InitializeTreeViewData()
        {
            string label = "关卡编辑";
            treeView.AddMenuItem(new MenuItemData(label));
            for (int i = 0; i < 7; i++)
            {
                var temp2 = $"{label}/章节 {i}";
                for (int j = 0; j < 7; j++)
                {
                    var temp3 = $"{temp2}/关卡 {j}";
                    treeView.AddMenuItem(new MenuItemData(temp3));
                    for (int k = 0; k < 5; k++)
                    {
                        var temp4 = $"{temp3}/事件 {k}";
                        treeView.AddMenuItem(new MenuItemData(temp4));
                    }
                }
            }

            label = "角色编辑";
            treeView.AddMenuItem(new MenuItemData(label));
            for (int i = 0; i < 7; i++)
            {
                var temp2 = $"{label}/角色 {i}";
                treeView.AddMenuItem(new MenuItemData(temp2));
            }

            label = "Buff编辑";
            treeView.AddMenuItem(new MenuItemData(label));
            for (int i = 0; i < 7; i++)
            {
                var temp2 = $"{label}/Buff {i}";
                treeView.AddMenuItem(new MenuItemData(temp2));
            }
        }
    }
}