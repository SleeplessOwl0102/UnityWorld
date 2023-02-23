using System.Linq;

namespace SleeplessOwl.EditorUtil.UIElement
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.UIElements;


    public class SerializeObjectSearchView<TItem> : VisualElement where TItem : UnityEngine.Object
    {
        private SerializedObject m_virtualSourceSO;
        private List<SerializedProperty> m_searchValuePropList;
        private List<Toggle> m_toggleList;
        private Button m_filterBtn;
        private Button m_cancelBtn;
        private Func<IEnumerable<TItem>> m_sourceListGetter;
        private Action<List<TItem>> m_searchCbk;


        public SerializeObjectSearchView(IReadOnlyList<string> searchablePropertyPathes, SerializedObject virtualSourceSerializeObject, Func<IEnumerable<TItem>> sourceListGetter, Action<List<TItem>> searchCbk)
        {
            m_searchCbk = searchCbk;
            m_sourceListGetter = sourceListGetter;
            m_virtualSourceSO = virtualSourceSerializeObject;
            m_searchValuePropList = new List<SerializedProperty>(searchablePropertyPathes.Count);
            
            m_toggleList = new List<Toggle>();

            this.SetHeight((searchablePropertyPathes.Count + 1) * 18);

            foreach (var path in searchablePropertyPathes)
            {
                var searchValueProp = m_virtualSourceSO.FindProperty(path);
                if (searchValueProp == null)
                {
                    continue;
                }

                var horizontal = new HorizontalElement();
                m_searchValuePropList.Add(searchValueProp);

                var propField = new PropertyField(searchValueProp, searchValueProp.name);
                propField.style.flexGrow = 1;
                propField.SetWidth(200);
                propField.Bind(m_virtualSourceSO);

                var toggle = new Toggle() { value = true };
                toggle.RegisterCallback<ChangeEvent<bool>>((evt) => { propField.SetEnabled(evt.newValue); });
                m_toggleList.Add(toggle);

                Add(horizontal);
                horizontal.Add(toggle);
                horizontal.Add(propField);
            }

            var btnGroup = new HorizontalElement();
            Add(btnGroup);
            {
                m_filterBtn = new Button(StartFilter);
                m_filterBtn.text = "Search";
                m_filterBtn.SetWidth(200);
                btnGroup.Add(m_filterBtn);

                m_cancelBtn = new Button(CancelFilter);
                m_cancelBtn.text = "重置";
                m_cancelBtn.SetWidth(200);
                btnGroup.Add(m_cancelBtn);
            }
            
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        protected void OnDetach(DetachFromPanelEvent evt)
        {
            foreach (var item in m_searchValuePropList)
            {
                item.Dispose();
            }
        }

        private void CancelFilter()
        {
            m_searchCbk?.Invoke(m_sourceListGetter?.Invoke().ToList());
        }

        private void StartFilter()
        {
            var res = FilterList(m_sourceListGetter?.Invoke());
            m_searchCbk?.Invoke(res);
        }

        public List<TItem> FilterList(IEnumerable<UnityEngine.Object> sourceList)
        {
            var resultList = new List<TItem>();
            foreach (var item in sourceList)
            {
                bool isPass = true;

                using (var itemSo = new SerializedObject(item))
                {
                    for (int i = 0; i < m_searchValuePropList.Count; i++)
                    {
                        if (m_toggleList[i].value == false)
                            continue;

                        SerializedProperty searchValueProp = m_searchValuePropList[i];
                        using (var compareProp = itemSo.FindProperty(searchValueProp.propertyPath))
                        {
                            Assert.IsNotNull(compareProp);

                            if (searchValueProp.CompareSerializedPropertyValue(compareProp) == false)
                            {
                                isPass = false;
                                break;
                            }
                        }
                    }
                }

                if (isPass == false)
                {
                    continue;
                }

                resultList.Add((TItem)item);
            }

            return resultList;
        }
    }

    

}
