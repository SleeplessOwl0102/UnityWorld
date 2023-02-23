using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    /// <summary>
    /// 附加树状选单的编辑器基类
    /// </summary>
    public class MenuTreeWindow: EditorWindow
    {
        private VisualElement m_leftView;
        private VisualElement m_rightView;
        private ToolbarSearchField m_searchField;

        protected MenuTreeView m_treeView;
        private List<MenuTreeItemBase> m_itemBaseList;
        private ScrollView m_rightViewSelection;

        /// <summary>
        /// 建构窗口内容
        /// </summary>
        private void CreateGUI()
        {
            m_itemBaseList = new List<MenuTreeItemBase>();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            var twoPan = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(twoPan);

            m_leftView = new VisualElement() { name = "左半Menu列表Pannel" };
            m_leftView.style.borderRightColor = Color.gray;
            m_leftView.style.borderRightWidth = 1;

            twoPan.Add(m_leftView);
            m_searchField = new ToolbarSearchField() { value = "", name = "搜索窗口" };
            m_searchField.style.flexGrow = 1;
            m_searchField.style.width = 50;
            m_searchField.Q<TextField>().isDelayed = true;
            m_searchField.Q<TextField>().RegisterCallback<ChangeEvent<string>>(OnSearchFilterChange);

            var toolbarHorizontal = new HorizontalElement() {name = "Menu列表工具栏"};
            m_leftView.Add(toolbarHorizontal);
            toolbarHorizontal.Add(m_searchField);
            var reloadBtn = new Button(() => { BuildTreeView(); });
            reloadBtn.text = "reload";
            reloadBtn.style.width = 50;
            toolbarHorizontal.Add(reloadBtn);

            m_rightView = new VisualElement() { name = "右半选取内容显示Pannel" };
            m_rightView.style.flexDirection = FlexDirection.Row;
            m_rightView.style.SetFlexGrow(1);
            twoPan.Add(m_rightView);

            m_rightViewSelection = new ScrollView();
            m_rightViewSelection.style.SetPaddingWidthAll().SetFlexGrow(1);
            m_rightView.Add(m_rightViewSelection);

            BuildTreeView();
        }

        private void OnSearchFilterChange(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                BuildTreeView();
                return;
            }

            for (int i = m_treeView.itemsSource.Count - 1; i >= 0; i--)
            {
                var menuItem = m_treeView.GetItemDataForIndex<IMenuItem>(i);
                if (menuItem.GetMenuPath.ToUpper().Contains(evt.newValue.ToUpper()) == false)
                {
                    m_treeView.itemsSource.RemoveAt(i);
                }
            }

            m_treeView.Rebuild();
            m_treeView.SetSelection(m_treeView.selectedIndex);
        }

        private void BuildTreeView()
        {
            if (m_treeView != null)
                m_leftView.Remove(m_treeView);

            m_treeView = new MenuTreeView(null, AfterBind);
            m_treeView.selectionType = SelectionType.Multiple;
            m_treeView.unbindItem += HandleUnbind;
            m_leftView.Add(m_treeView);

            m_treeView.onSelectionChange += HandleSelectionChange;

            //InitializePineContent();
            InitializeTreeViewContent();
            m_treeView.Rebuild();
        }

        public void ReBuild()
        {
            BuildTreeView();
        }

        private void AfterBind(VisualElement ve, IMenuItem item)
        {
            ve.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                if (item is ObjectAssetItem soItem)
                {
                    evt.menu.AppendAction($"档名 {soItem.GetObject.name}", null, DropdownMenuAction.Status.Disabled);

                    evt.menu.AppendAction("在新窗口打开", (x) =>
                    {
                        var window = PopUpAssetWindow.Create(soItem.GetObject, this);
                    });

                    evt.menu.AppendAction("复制一份", (x) =>
                    {
                        var config = soItem.GetObject;
                        AssetDatabase.CopyAsset(soItem.GetObjectPath, AssetDatabase.GetAssetPath(config).Replace($".asset", "_New.asset"));
                        AssetDatabase.Refresh();
                        ReBuild();
                    });

                    evt.menu.AppendAction("删除", (x) =>
                    {
                        var config = soItem.GetObject;
                        AssetDatabase.DeleteAsset(soItem.GetObjectPath);
                        AssetDatabase.Refresh();
                        var index = m_treeView.selectedIndex;
                        ReBuild();
                        m_treeView.SetSelection(index);
                    });

                    evt.menu.AppendAction("重命名", (x) =>
                    {
                        var config = soItem.GetObject;
                        var content = new PopupRename(config, () => { ReBuild(); });
                        UnityEditor.PopupWindow.Show(ve.worldBound, content);
                    });
                }
            }));
        }

        internal sealed class PopupRename: PopupWindowContent
        {
            private string m_name;
            private UnityEngine.Object m_asset;
            private Action m_renameFinishCbk;

            public PopupRename(UnityEngine.Object asset, Action onRenameFinish)
            {
                m_asset = asset;
                m_renameFinishCbk = onRenameFinish;
            }

            public override void OnGUI(Rect rect)
            {
                m_name = GUILayout.TextField(m_name);
                if (GUILayout.Button("Rename"))
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(m_asset), m_name);
                    AssetDatabase.Refresh();
                    if (editorWindow)
                        editorWindow.Close();

                    m_renameFinishCbk?.Invoke();
                }
            }
        }

        private void HandleUnbind(VisualElement arg1, int arg2)
        {
            for (int i = 0; i < m_rightViewSelection.childCount; i++)
            {
                m_rightViewSelection.RemoveAt(i);
            }
        }

        private void HandleSelectionChange(IEnumerable<object> obj)
        {
            for (int i = m_itemBaseList.Count - 1; i >= 0; i--)
            {
                MenuTreeItemBase item = m_itemBaseList[i];
                item?.OnDeselect();

                m_itemBaseList.RemoveAt(i);
            }

            for (int i = m_rightViewSelection.childCount - 1; i >= 0; i--)
            {
                m_rightViewSelection.RemoveAt(i);
            }

            int count = 0;
            foreach (var item in obj)
            {
                count++;
                if (item is MenuTreeItemBase data)
                {
                    m_rightViewSelection.Add(data.GetVisualElement);
                    m_itemBaseList.Add(data);
                }
            }
        }

        public List<string> AddAllScriptableObjectData<T>(string root, string path) where T : ScriptableObject
        {
            if (AssetDatabase.IsValidFolder(path) == false)
            {
                Debug.LogError("要搜索的根路径不存在！");
                return null;
            }

            var guids = AssetDatabase.FindAssets($"t:{typeof (T).Name}", new[] { path });

            foreach (var item in guids)
            {
                string fullPath = AssetDatabase.GUIDToAssetPath(item);

                string menuPath = fullPath.Replace(path, root).Replace(".asset", "");
                m_treeView.AddMenuItem(new ObjectAssetItem(menuPath, fullPath));
            }

            return null;
        }

        public void AddMenuItem(MenuTreeItemBase item)
        {
            m_treeView.AddMenuItem(item);
        }

        protected virtual void InitializeTreeViewContent()
        {
            //example
            string label = "关卡编辑/Ch 1/Stage 1";
            m_treeView.AddMenuItem(new MenuTreeItemBase(label));
        }
    }
}