using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    /// <summary>
    /// 方便透过string路径建立TreeView列表的封装类
    /// </summary>
    public class MenuTreeView : TreeView
    {
        private Dictionary<string, int> m_pathToId;
        private int m_id;

        private Action<VisualElement, IMenuItem> m_afterBindItem;

        public MenuTreeView(List<TreeViewItemData<IMenuItem>> sourceData,
            Action<VisualElement, IMenuItem> afterBindItem = null)
        {
            m_id = 0;
            m_pathToId = new Dictionary<string, int>();
            if (sourceData == null)
            {
                sourceData = new List<TreeViewItemData<IMenuItem>>();
            }

            SetRootItems(sourceData);
            m_afterBindItem = afterBindItem;
            makeItem += HandleMakeItem;
            bindItem += HandleBindItem;
        }

        public void AddMenuItem(IMenuItem itemData, bool rebuild = false, string overrideMenuPath = null)
        {
            var menuPath = itemData.GetMenuPath;
            if (string.IsNullOrEmpty(overrideMenuPath) == false)
                menuPath = overrideMenuPath;

            var pathSplits = menuPath.Split('/');
            var pathSplitsCount = pathSplits.Length;

            //if parent node not exist, create one
            int parentId = -1;
            if (pathSplits.Length > 1)
            {
                string tempPath = string.Empty;
                for (int i = 0; i < pathSplitsCount - 1; i++)
                {
                    tempPath += pathSplits[i];

                    if (i < pathSplits.Length - 2)
                        tempPath += "/";
                }

                if (m_pathToId.ContainsKey(tempPath) == false)
                {
                    AddMenuItem(new MenuTreeItemBase(tempPath), rebuild);
                }

                if (m_pathToId.ContainsKey(tempPath))
                {
                    parentId = m_pathToId[tempPath];
                }
            }

            if (m_pathToId.ContainsKey(menuPath) == false)
            {
                m_pathToId.Add(menuPath, m_id);
                TreeViewItemData<IMenuItem> item = new TreeViewItemData<IMenuItem>(m_id++, itemData);
                AddItem(item, parentId: parentId, rebuildTree: rebuild);
            }
        }

        protected virtual VisualElement HandleMakeItem()
        {
            var ve = new Label();
            ve.style.unityTextAlign = TextAnchor.MiddleLeft;
            ve.style.flexGrow = 1;
            return ve;
        }

        protected virtual void HandleBindItem(VisualElement ve, int index)
        {
            var menuItem = GetItemDataForIndex<IMenuItem>(index);
            if (ve is Label lb)
            {
                var itemRoot = lb.parent.parent;
                itemRoot.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, .25f);
                itemRoot.style.borderBottomWidth = 1f;

                lb.text = menuItem.GetItemLabelText;
                menuItem.Init(index, this);
            }

            m_afterBindItem?.Invoke(ve, menuItem);
        }
    }
}