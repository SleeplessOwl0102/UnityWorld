using System.Collections.Generic;
using System.Linq;
using SleeplessOwl.EditorUtil.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public abstract class TableConfigBase<T> : ScriptableObject, IMaxHeight, IColumnDefineProvider
        where T : TableColumn, new()
    {
        [Header("定义每列显示内容")]
        [FormerlySerializedAs("list")]
        public List<T> m_columnDefineList;

        [Header("List序列化路径")]
        [FormerlySerializedAs("listRootFieldPath")]
        public string m_listRootFieldPath;

        [Header("列表元素被选中时要显示的序列化路径")]
        public string m_previewSerializePath;

        [Header("Table最大高度限制")]
        public int m_tableMaxHeight;

        [Header("Table默认高度")]
        public int m_tableHeight;

        [Header("显示选中元素详细内容")]
        public bool m_showSelectItemDetail;

        [Header("显示预览图")]
        public bool m_showSelectModelPreiview;


        [Header("可被搜索的内容路径列表")]
        public List<string> m_searchablePathList;

        void Reset()
        {
            m_columnDefineList = new List<T>()
            {
                new T()
            };
        }
        [Button]
        public void Save()
        {
            EditorUtility.SetDirty(this);

            AssetDatabase.SaveAssetIfDirty(this);
        }

        IReadOnlyList<TableColumn> IColumnDefineProvider.ColumnList => 
            m_columnDefineList.Cast<TableColumn>().ToList().AsReadOnly();

        string IColumnDefineProvider.ListRootSerializePath => m_listRootFieldPath;

        bool IColumnDefineProvider.ShowSelectItemDetail => m_showSelectItemDetail;

        bool IColumnDefineProvider.ShowSelectModelPreiview => m_showSelectModelPreiview;

        string IColumnDefineProvider.PreviewSerializePath => m_previewSerializePath;

        int IMaxHeight.MaxHeight => m_tableMaxHeight;

        int IMaxHeight.Height => m_tableHeight;

        IReadOnlyList<string> IColumnDefineProvider.SearchablePathList => m_searchablePathList.AsReadOnly();
    }


}