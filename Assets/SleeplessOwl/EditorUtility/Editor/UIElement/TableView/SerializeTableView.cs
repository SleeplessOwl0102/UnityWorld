using SleeplessOwl.EditorUtil.EditorUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public class TableCommonData<TItem>
    {
        //单个serialize object内包含list时使用的cache
        public SerializedObject serializeObject;
        
        public ISerializeTableView table;
        public List<TItem> itemList;
    }

    public interface ISerializeTableView
    {
        public void RefreshItemAndTableView();

        public void RemoveElement(int index);
    }

    /// <summary>
    /// 方便大量Unity.Object类型透过Table列表呈现数据的类，另提供选取显示完整内容与预览图的功能
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public abstract class SerializeTableView<TItem> : VisualElement, ISerializeTableView where TItem : UnityEngine.Object
    {
        protected static List<TItem> s_copyItemData;

        protected TableCommonData<TItem> m_commonData;
        protected ScrollView m_previewScroller;
        protected PropertyField m_previewInfoProperty;
        protected SerializedObject m_selectedObjectsSerializeObject;
        protected int m_selectIndex;
        protected MultiColumnListView m_tableView;

        protected Image m_previewImageProperty;
        protected EditorCoroutine m_previewGetCoroutine;

        protected VisualElement m_tableContainer;
        protected VisualElement m_tableContentRoot;

        protected IColumnDefineProvider m_columnDefineProvider;
        private int m_selectItemCount;

        #region Constructor
        public SerializeTableView(IColumnDefineProvider columnDefine)
        {
            Assert.IsNotNull(columnDefine, "未提供Column绘制定义");

            m_commonData = new TableCommonData<TItem>();
            m_commonData.table = this;
            m_columnDefineProvider = columnDefine;

            RegisterCallback<KeyDownEvent>(HandleTableShortcut);


            BuildRootElement();
            BuildTableElement();
            BuildPreviewElement();
        }



        #endregion

        #region Virtual Method
        
        
        protected virtual void BeforeBuildTableView()
        {
        }

        
        protected virtual void HandleTableShortcut(KeyDownEvent evt)
        {
        }

        protected abstract List<TItem> ItemGetter { get; }


        public abstract void RemoveElement(int index);

        #endregion

        #region Build UIElement
        private void BuildPreviewElement()
        {

            if (m_previewInfoProperty != null)
                m_previewScroller.TryRemove(m_previewInfoProperty);

            if (m_previewImageProperty != null)
                m_tableContentRoot.TryRemove(m_previewImageProperty);


            if (m_columnDefineProvider.ShowSelectItemDetail)
            {
                m_previewScroller = new ScrollView(ScrollViewMode.Vertical);
                m_tableContentRoot.Add(m_previewScroller);
            }

            if (m_columnDefineProvider.ShowSelectModelPreiview)
            {
                m_previewImageProperty = new Image();
                m_previewImageProperty.style.width = 128;
                m_previewImageProperty.style.height = 128;
                m_tableContentRoot.Add(m_previewImageProperty);
            }
        }

        protected void BuildTableElement()
        {
            BeforeBuildTableView();

            m_tableContentRoot = new VisualElement();
            m_tableContentRoot.name = "m_tableContentRoot";
            Add(m_tableContentRoot);
            m_tableContentRoot.style.flexDirection = FlexDirection.Row;


            if (m_tableContainer == null)
            {
                m_tableContainer = new VisualElement();
                m_tableContentRoot.Add(m_tableContainer);
            }

            if (m_tableView != null)
                m_tableContainer.TryRemove(m_tableView);

            var tableColumns = m_columnDefineProvider.ColumnList;
            var columns = BuildTableColumnList(tableColumns);
            var table = new MultiColumnListView(columns);
            table.sortingEnabled = true;
            table.columnSortingChanged += SortAndRebuildTable;
            table.onSelectedIndicesChange += OnSelectedIndicesChange;
            table.itemsSource = m_commonData.itemList;
            table.fixedItemHeight = 30;
            table.style.SetBorderColorAll(Color.gray).SetBorderWidthAll(1);

            if (m_columnDefineProvider is IMaxHeight getter)
            {
                table.style.height = getter.Height;
            }

            m_tableView = table;

            table.selectionType = SelectionType.Multiple;

            foreach (var item in tableColumns)
            {
                item.CustomBehaviour?.OnTableBuild();
            }


            m_tableContainer.Add(table);
        }

        public void RefreshItemAndTableView()
        {
            m_commonData.itemList = ItemGetter;
            m_tableView.itemsSource = m_commonData.itemList;

            RefreshTableView();
        }

        public void RefreshTableView()
        {
            var ind = m_tableView.selectedIndex;

            //避免Preview握有被删除的资源产生的error
            BuildPreviewElement();
            if (ind >= m_tableView.itemsSource.Count)
            {
                ind = m_tableView.itemsSource.Count - 1;
            }
            //m_commonData.serializeObject?.Update();
            m_tableView.Rebuild();
            m_tableView.SetSelection(ind);
        }

        private void BuildRootElement()
        {
            this.style.minHeight = 150;
            if (m_columnDefineProvider is IMaxHeight getter)
            {
                this.style.maxHeight = getter.MaxHeight;
            }
        }


        #endregion

        #region Handle Select Change
        public List<TItem> GetSelectedItem()
        {
            var ids = m_tableView.selectedIndices;
            List<TItem> res = new List<TItem>();

            foreach (var index in ids)
            {
                res.Add((TItem)m_tableView.itemsSource[index]);
            }
            return res;
        }

        private void OnSelectedIndicesChange(IEnumerable<int> itemIndexes)
        {
            var m_selectItems = new List<UnityEngine.Object>();
            foreach (var item in itemIndexes)
            {
                if (m_tableView.itemsSource[item] is UnityEngine.Object obj)
                {
                    m_selectItems.Add(obj);
                }
            }

            if (m_selectItems.Count == 0)
            {
                m_selectedObjectsSerializeObject = null;
                return;
            }

            m_selectItemCount = m_selectItems.Count;
            m_selectedObjectsSerializeObject = new SerializedObject(m_selectItems.ToArray());
            m_selectedObjectsSerializeObject.UpdateIfRequiredOrScript();
            UpdatePreviewBinding();
        }

        private void UpdatePreviewBinding()
        {
            if (m_previewGetCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(m_previewGetCoroutine);

            SerializedObject selectSerializeObject;
            SerializedProperty infoProp;
            //"DataRoot.CharacterInfo"
            var path = m_columnDefineProvider.PreviewSerializePath;

            selectSerializeObject = m_selectedObjectsSerializeObject;
            if (selectSerializeObject == null)
                return;
            infoProp = m_selectedObjectsSerializeObject.FindProperty(path);

            if (infoProp == null)
            {
                m_previewInfoProperty?.Unbind();
                return;
            }

            if (m_columnDefineProvider.ShowSelectItemDetail)
            {
                infoProp.isExpanded = true;
                
                m_previewScroller.Clear();
                m_previewInfoProperty = new PropertyField(infoProp);
                m_previewInfoProperty.style.minWidth = 300;
                m_previewInfoProperty.BindProperty(infoProp);
                
                if(selectSerializeObject.isEditingMultipleObjects)
                    m_previewScroller.Add(new Label($"正在修改 {m_selectItemCount} 笔数据"));
                m_previewScroller.Add(m_previewInfoProperty);
            }


            if (m_columnDefineProvider.ShowSelectModelPreiview && selectSerializeObject.targetObject is IPreviewTextureGetter)
            {
                m_previewImageProperty.image = null;
                if (selectSerializeObject.targetObject is IPreviewTextureGetter item)
                {
                    m_previewGetCoroutine = EditorCoroutineUtility.StartCoroutine(tryGetPreviewTexture(), this);
                    IEnumerator tryGetPreviewTexture()
                    {
                        if (m_previewImageProperty.visible == false)
                            yield break;

                        Texture2D texture = null;
                        while (texture == null)
                        {
                            texture = item.GetPreviewTexture();
                            yield return null;
                        }
                        m_previewImageProperty.image = texture;
                    }
                }
            }
        }

        #endregion

        #region Handle column sorting
        private void SortAndRebuildTable()
        {
            var table = m_commonData.table;
            foreach (var column in m_tableView.sortedColumns)
            {
                var col = column.column as ExtendColumn<TItem>;
                if (col == null)
                    continue;

                var items = m_tableView.viewController.itemsSource as List<TItem>;
                if (col.customDrawer is IComparer<TItem> comparer)
                {
                    if (column.direction == SortDirection.Ascending)
                    {
                        items.Sort(comparer);
                    }
                    else//Descending
                    {
                        items.Sort((x, y) =>
                        {
                            return -comparer.Compare(x, y);
                        });
                    }
                }
                var ID = m_tableView.columns.IndexOf(col);
                m_tableView.Rebuild();
                break;
            }
        }

        protected void SetSortingColumnByIndex(int index, SortDirection sortDirection)
        {
            m_tableView.sortColumnDescriptions.Clear();
            m_tableView.sortColumnDescriptions.Add(new SortColumnDescription(index, sortDirection));
        }

        #endregion

        public virtual Columns BuildTableColumnList(IReadOnlyList<TableColumn> list)
        {
            var columns = new Columns();
            foreach (var item in list)
            {
                columns.Add(CreateExtendColumn(item));
            }
            return columns;

            ExtendColumn<TItem> CreateExtendColumn(TableColumn setting)
            {
                var extendColumn = new ExtendColumn<TItem>(setting);
                extendColumn.makeCell = () =>
                {
                    return OnMakeCell(setting);
                };
                extendColumn.bindCell = (ve, Index) =>
                {
                    OnBindCell(ve, Index, setting);
                };
                return extendColumn;
            }
        }

        private void OnBindCell(VisualElement ve, int index, TableColumn setting)
        {
            SerializedObject rootSo = null;
            SerializedProperty targetFieldProp = null;

            if (m_commonData.itemList.Count <= index)
                return;

            if (m_commonData.itemList[index] is UnityEngine.Object obj)
            {
                rootSo = new SerializedObject(obj);
                targetFieldProp = rootSo.FindProperty(setting.fieldPath);
            }

            if (rootSo == null)
            {
                RefreshItemAndTableView();
            }

            if (setting.CustomBehaviour != null &&
                setting.CustomBehaviour is TableColumn.IDrawer<TItem> drawer)
            {
                var label = ve.Q<IMGUIContainer>();
                label.onGUIHandler = () =>
                {
                    if (rootSo.targetObject is TItem item)
                    {
                        drawer.DrawGUI(item, m_commonData, index);
                    }
                };
                return;
            }

            if (targetFieldProp != null)
            {
                if (setting.IsEditable)
                {
                    var propertyField = ve.Q<IMGUIContainer>();
                    propertyField.onGUIHandler = () =>
                    {
                        if (targetFieldProp == null)
                            return;
                        rootSo.UpdateIfRequiredOrScript();
                        EditorGUILayout.PropertyField(targetFieldProp, GUIContent.none);
                        rootSo.ApplyModifiedProperties();
                    };
                }
                else
                {
                    var label = ve.Q<Label>();
                    label.BindProperty(targetFieldProp);
                }
            }
        }
        private VisualElement OnMakeCell(TableColumn setting)
        {
            VisualElement root = new VisualElement();
            root.style.flexGrow = 1;
            root.style.justifyContent = Justify.Center;

            if (setting.CustomBehaviour != null &&
                setting.CustomBehaviour is TableColumn.IDrawer<TItem>)
            {
                var label = new IMGUIContainer();
                root.Add(label);
            }
            else
            {
                if (setting.IsEditable)
                {
                    var label = new IMGUIContainer();
                    root.Add(label);
                }
                else
                {
                    var label = new Label();
                    label.style.fontSize = 12;
                    root.Add(label);
                }
            }
            return root;
        }

    }
}