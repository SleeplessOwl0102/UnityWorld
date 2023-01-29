using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RL.EditorCore
{
    public class MenuTreeView : TreeView
    {
        private Dictionary<string, int> m_pathToId;
        private int m_id;

        public MenuTreeView(List<TreeViewItemData<MenuItemData>> sourceData)
        {
            m_id = 0;
            m_pathToId = new Dictionary<string, int>();

            if (sourceData == null)
            {
                sourceData = new List<TreeViewItemData<MenuItemData>>();
            }

            SetRootItems(sourceData);

            makeItem += HandleMakeItem;
            bindItem += handleBindItem;
        }


        public void AddMenuItem(MenuItemData itemData)
        {
            Debug.Log(itemData.GetMenuPath);
            var pathSplits = itemData.GetMenuPath.Split('/');
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
                    AddMenuItem(new MenuItemData(tempPath));
                }

                if (m_pathToId.ContainsKey(tempPath))
                {
                    parentId = m_pathToId[tempPath];
                }
            }

            m_pathToId.Add(itemData.GetMenuPath, m_id);
            TreeViewItemData<MenuItemData> item = new TreeViewItemData<MenuItemData>(m_id++, itemData);
            AddItem(item, parentId: parentId, rebuildTree: false);
        }

        protected virtual VisualElement HandleMakeItem()
        {
            var ve = new Label();
            ve.style.unityTextAlign = TextAnchor.MiddleLeft;
            ve.style.flexGrow = 1;
            return ve;
        }

        protected virtual void handleBindItem(VisualElement ve, int index)
        {
            if (ve is Label lb)
            {
                var itemRoot = lb.parent.parent;
                itemRoot.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, .25f);
                itemRoot.style.borderBottomWidth = 1f;
                lb.text = GetItemDataForIndex<MenuItemData>(index).GetItemLabelText;
                Debug.Log(lb.text);
            }
        }
    }

    public interface IMenuItem
    {
        string GetMenuPath { get; }

        string GetItemLabelText { get; }

        VisualElement GetVisualElement { get; }

    }


    public class MenuItemData : IMenuItem
    {
        public MenuItemData(string menuPath)
        {
            this.menuPath = menuPath;
            int index = menuPath.LastIndexOf('/');
            if (index > -1)
            {
                itemLabelText = menuPath.Substring(index + 1);
            }
            else
            {
                itemLabelText = menuPath;
            }
        }

        public MenuItemData(string menuPath, string itemName)
        {
            this.menuPath = menuPath;
            itemLabelText = itemName;
        }

        private string menuPath;

        private string itemLabelText;

        public virtual string GetMenuPath => menuPath;

        public virtual string GetItemLabelText => itemLabelText;

        public virtual VisualElement GetVisualElement => new Label($"{menuPath} : No implment visualelement");
    }
}