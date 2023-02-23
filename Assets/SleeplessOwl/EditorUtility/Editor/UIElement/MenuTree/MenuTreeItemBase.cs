using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    /// <summary>
    /// MenuTreeView中每个选项按钮的基础类别，可继承后实现客制化内容
    /// </summary>
    public class MenuTreeItemBase : IMenuItem
    {

        /// <summary>
        /// 此选项在MenuTreeView上显示的层级路径
        /// </summary>
        private string m_menuPath;

        /// <summary>
        /// 此选项在MenuTreeView上显示的名称
        /// </summary>
        private string m_labelText;
        protected MenuTreeView m_treeView;
        protected int m_itemIndex;

        public MenuTreeItemBase(string menuPath)
        {
            this.m_menuPath = menuPath;
            int index = menuPath.LastIndexOf('/');
            if (index > -1)
            {
                m_labelText = menuPath.Substring(index + 1);
            }
            else
            {
                m_labelText = menuPath;
            }
        }

        public MenuTreeItemBase(string menuPath, string name)
        {
            this.m_menuPath = menuPath;
            m_labelText = name;
        }

        /// <summary>
        /// 获取此选项关联的资源路径
        /// </summary>
        public virtual string GetMenuPath => m_menuPath;

        /// <summary>
        /// 此选项在MenuTreeView上显示的名称
        /// </summary>
        public virtual string GetItemLabelText => m_labelText;

        
        public void Init(int id, MenuTreeView treeView)
        {
            m_treeView = treeView;
            m_itemIndex = id;
        }

        /// <summary>
        /// 覆盖此方法实现客制化编辑界面
        /// </summary>
        public virtual VisualElement GetVisualElement
        {
            get
            {
                return new Label($"{m_menuPath} : No implment visualelement");
            }
        }

        public virtual void OnDeselect()
        {

        }
    }
}