using SleeplessOwl.EditorUtil.RuntimeEditor;
using System;
using System.Collections.Generic;
using SleeplessOwl.EditorUtil.Mono;
using UnityEngine;

namespace SleeplessOwl.EditorUtil.UIElement
{
    
    /// <summary>
    /// 定义SerializeTableView列表Column的绘制方式
    /// </summary>
    [Serializable]
    public class TableColumn
    {
        /// <summary>
        /// Column的标题
        /// </summary>
        public string title;
        
        /// <summary>
        /// 要显示字段的序列化路径
        /// </summary>
        public string fieldPath;
        
        /// <summary>
        /// 此Column预设宽度
        /// </summary>
        public float width;
        
        /// <summary>
        /// 此Column是否可见
        /// </summary>
        public bool visible;
        
        /// <summary>
        /// 此Column内容可否在TableView上直接编辑
        /// </summary>
        public bool IsEditable;

        /// <summary>
        /// 用来自定义绘制内容（继承 TableColumn.IDrawer）
        /// 或提供排序功能（继承IComparer）
        /// </summary>
        [ReferenceTypeSelector(typeof(ExtendBehaviour), ReferenceTypeSelectorAttribute.SelectTypeMethod.BelongSerializeObjectNamespace)]
        [SerializeReference]
        public ExtendBehaviour CustomBehaviour;

        public TableColumn()
        {
            title = "";
            width = 100;
            visible = true;
        }

        public abstract class ExtendBehaviour
        {
            public virtual void OnTableBuild()
            {

            }
        }

        /// <summary>
        /// 用来客制化显示内容，或按钮等....
        /// </summary>
        public interface IDrawer<T>
        {
            public abstract void DrawGUI(T itemData, TableCommonData<T> commonData, int indexInSourceList);
        }

    }


}