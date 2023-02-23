using SleeplessOwl.EditorUtil.EditorUtil;
using System;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public class PropertyListTableView<TItem> : VisualElement, ISerializeTableView
    {
        private bool m_undoable;
        private TableCommonData<TItem> m_commonData;
        private ScrollView m_previewScroller;
        private PropertyField m_previewInfoProperty;
        private int m_selectIndex;
        private MultiColumnListView m_tableView;

        private Image m_previewImageProperty;
        private EditorCoroutine m_coroutine;
        private VisualElement m_tableContainer;
        public static List<TItem> s_copyItemData;
        private SerializedObject rootSo;
        private IMGUIContainer m_previewgui;
        private SerializedProperty m_InfoProperty;

        public SerializedProperty m_listProp;

        protected IColumnDefineProvider m_columnDefineProvider;

        #region Constructor
        public PropertyListTableView(UnityEngine.Object rootObject,
                    List<TItem> items,
                    IColumnDefineProvider columnDefineProvider, bool undoable = false)
        {
            Assert.IsNotNull(columnDefineProvider, "未提供Column绘制定义");
            m_columnDefineProvider = columnDefineProvider;

            m_undoable = undoable;

            m_commonData = new TableCommonData<TItem>();
            m_commonData.table = this;
            m_commonData.itemList = items;
            m_commonData.serializeObject = new SerializedObject(rootObject);


            m_listProp = m_commonData.serializeObject.FindProperty(m_columnDefineProvider.ListRootSerializePath);

            RegisterCallback<KeyDownEvent>(HandleTableShortcut);

            BuildRoot();
            BuildTable();
            BuildPreview();
        }


        #endregion
        #region Shortcut
        private void HandleTableShortcut(KeyDownEvent evt)
        {
            //property list 要支援undo需要
            if (evt.ctrlKey && evt.keyCode == KeyCode.Z)
            {
                Debug.Log("PerformUndo");
                Undo.PerformUndo();
                RefreshItemAndTableView();
                evt.StopImmediatePropagation();
            }

            if (evt.ctrlKey && evt.keyCode == KeyCode.Y)
            {
                Debug.Log("PerformRedo");
                Undo.PerformRedo();
                RefreshItemAndTableView();
                Debug.Log("RebuildTable");
                evt.StopImmediatePropagation();
            }


            if (evt.keyCode == KeyCode.Delete)
            {
                var items = GetSelectedItem();
                var str = string.Empty;
                foreach (var item in items)
                {
                    str += item.ToString();
                    str += "\n";
                }

                //if (EditorUtility.DisplayDialog("删除对象", str, "ok", "cancel"))
                //{
                var itemindexs = m_tableView.selectedIndex;

                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(m_commonData.serializeObject.targetObject, "");
                m_tableView.viewController.RemoveItem(itemindexs);

                //m_commonData.itemList = m_commonData.GETitemList.Invoke();
                //}

                evt.StopImmediatePropagation();
                RefreshItemAndTableView();
            }
        }

        #endregion

        private void BuildRoot()
        {
            this.style.minHeight = 150;
            if (m_columnDefineProvider is IMaxHeight getter)
            {
                this.style.maxHeight = getter.MaxHeight;
            }
        }


        public void RemoveElement(int index)
        {
            m_commonData.serializeObject.Update();
            m_listProp.DeleteArrayElementAtIndex(index);
            m_commonData.serializeObject.ApplyModifiedProperties();
        }

        public void RefreshItemAndTableView()
        {
            var ind = m_tableView.selectedIndex;

            //避免Preview握有被删除的资源产生的error
            BuildPreview();
            if (ind >= m_tableView.itemsSource.Count)
            {
                ind = m_tableView.itemsSource.Count - 1;
            }
            m_commonData.serializeObject?.Update();
            m_tableView.Rebuild();

            m_tableView.SetSelection(ind);

        }

        private void BuildTable()
        {
            style.flexDirection = FlexDirection.Row;

            if (m_tableContainer == null)
            {
                m_tableContainer = new VisualElement();
                Add(m_tableContainer);
            }

            if (m_tableView != null)
                m_tableContainer.TryRemove(m_tableView);

            var tableColumns = m_columnDefineProvider.ColumnList;
            var columns = BuildTableColumnList(tableColumns);
            var table = new MultiColumnListView(columns);

            table.reorderable = true;
            table.sortingEnabled = true;
            table.columnSortingChanged += OnColumnSortingChanged;
            table.onSelectedIndicesChange += OnSelectedIndicesChange;
            table.itemsSource = m_commonData.itemList;
            table.fixedItemHeight = 30;
            table.style.SetBorderColorAll(Color.gray).SetBorderWidthAll(1);

            if (m_columnDefineProvider is IMaxHeight getter)
            {
                table.style.height = getter.Height;
            }

            m_tableView = table;

            table.selectionType = SelectionType.Single;

            foreach (var item in tableColumns)
            {
                item.CustomBehaviour?.OnTableBuild();
            }


            m_tableContainer.Add(table);
        }

        private void BuildPreview()
        {
            m_previewScroller?.Clear();
            m_previewInfoProperty = null;

            if (m_previewImageProperty != null)
                this.TryRemove(m_previewImageProperty);


            var define = m_columnDefineProvider;
            if (define.ShowSelectItemDetail)
            {
                m_previewScroller = new ScrollView(ScrollViewMode.Vertical);
                Add(m_previewScroller);

                m_previewInfoProperty = new PropertyField();
                m_previewInfoProperty.style.minWidth = 300;
                m_previewInfoProperty.Bind(m_commonData.serializeObject);
                m_previewScroller.Add(m_previewInfoProperty);
            }

            if (define.ShowSelectModelPreiview)
            {
                m_previewImageProperty = new Image();
                m_previewImageProperty.style.width = 128;
                m_previewImageProperty.style.height = 128;
                Add(m_previewImageProperty);
            }
        }

        private void UpdatePreviewBinding()
        {
            if (m_coroutine != null)
                EditorCoroutineUtility.StopCoroutine(m_coroutine);

            SerializedObject selectSerializeObject;
            SerializedProperty infoProp;
            //"DataRoot.CharacterInfo"
            var path = m_columnDefineProvider.PreviewSerializePath;

            selectSerializeObject = m_commonData.serializeObject;

            if (m_listProp.arraySize <= m_selectIndex)
                return;

            infoProp = m_listProp.GetArrayElementAtIndex(m_selectIndex);
            if (string.IsNullOrEmpty(path) == false)
            {
                infoProp = infoProp.FindPropertyRelative(path);
            }

            if (infoProp == null)
                return;

            m_previewScroller?.Clear();
            if (m_columnDefineProvider.ShowSelectItemDetail)
            {
                infoProp.isExpanded = true;
                //m_previewInfoProperty.BindProperty(infoProp);
                m_InfoProperty = infoProp;
                m_previewgui = new IMGUIContainer(DrawDetail);
                m_previewScroller.Add(m_previewgui);
            }

        }

        private void DrawDetail()
        {
            m_InfoProperty.serializedObject.Update();
            EditorGUILayout.PropertyField(m_InfoProperty);
            m_InfoProperty.serializedObject.ApplyModifiedProperties();
        }

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
            foreach (var item in itemIndexes)
            {
                m_selectIndex = item;
                break;
            }

            UpdatePreviewBinding();
        }

        #endregion

        #region Handle column sorting change
        private void OnColumnSortingChanged()
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
                    else
                    {
                        items.Sort((x, y) =>
                        {
                            return -comparer.Compare(x, y);
                        });
                    }
                }
                m_tableView.Rebuild();
                break;
            }
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
            SerializedProperty itemProp = null;
            SerializedProperty targetFieldProp = null;

            var listProp = m_listProp;
            if (listProp.arraySize <= index)
                return;

            rootSo = m_commonData.serializeObject;
            itemProp = listProp.GetArrayElementAtIndex(index);
            targetFieldProp = itemProp.FindPropertyRelative(setting.fieldPath);

            if (setting.CustomBehaviour != null &&
                setting.CustomBehaviour is TableColumn.IDrawer<TItem> drawer)
            {
                var label = ve.Q<IMGUIContainer>();
                label.onGUIHandler = () =>
                {
                    drawer.DrawGUI(m_commonData.itemList[index], m_commonData, index);
                };
                return;
            }
            if (targetFieldProp != null)
            {
                if (setting.IsEditable)
                {
                    //var propertyField = ve.Q<PropertyField>();
                    //propertyField.Unbind();
                    //propertyField.BindProperty(targetFieldProp);
                    var propertyField = ve.Q<IMGUIContainer>();
                    propertyField.onGUIHandler = () =>
                    {
                        if (targetFieldProp == null)
                            return;
                        EditorGUILayout.PropertyField(targetFieldProp, GUIContent.none);

                        //TODO lyx : 
                        //ReconnectCharacterInstanceDataReference 会影响到undo结果
                        if (m_undoable)
                            rootSo.ApplyModifiedProperties();
                        else
                            rootSo.ApplyModifiedPropertiesWithoutUndo();
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
            if (setting.CustomBehaviour != null &&
                setting.CustomBehaviour is TableColumn.IDrawer<TItem>)
            {
                var root = new VisualElement();
                root.style.flexGrow = 1;
                root.style.justifyContent = Justify.Center;
                var label = new IMGUIContainer();
                root.Add(label);
                return root;
            }

            if (setting.IsEditable)
            {
                //var root = new PropertyField();
                //root.label = "";
                //root.style.flexGrow = 1;
                //root.style.justifyContent = Justify.Center;
                //return root;

                var root = new VisualElement();
                root.style.flexGrow = 1;
                root.style.justifyContent = Justify.Center;
                var label = new IMGUIContainer();
                root.Add(label);
                return root;
            }
            else
            {
                var root = new VisualElement();
                root.style.flexGrow = 1;
                root.style.justifyContent = Justify.Center;
                var label = new Label();

                label.style.fontSize = 12;
                root.Add(label);
                return root;
            }
        }

    }
}