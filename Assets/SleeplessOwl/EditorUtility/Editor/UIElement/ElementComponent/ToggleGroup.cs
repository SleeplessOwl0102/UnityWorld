namespace SleeplessOwl.EditorUtil.UIElement
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using UnityEngine.UIElements;

    /// <summary>
    /// VisualElement通用扩充类别，
    /// 可透过Enum元素生成按钮，支持falgs模式
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class ToggleGroup<TEnum> : VisualElement where TEnum : Enum, IConvertible
    {
        private List<Toggle> m_toggleList;
        private bool m_isFlagMode;
        private TEnum m_enum;

        public ToggleGroup(Action<TEnum> onClick, bool isFlagMode = false, TEnum defaultValue = default)
        {
            style.flexDirection = FlexDirection.Row;

            m_isFlagMode = isFlagMode;
            m_enum = defaultValue;
            m_toggleList = new List<Toggle>();

            var names = Enum.GetNames(typeof(TEnum));
            for (int i = 0; i < names.Length; i++)
            {
                var index = i;
                var name = names[i];
                var element = new Toggle();

                //try get description attribute
                var type = m_enum.GetType();
                var memInfo = type.GetMember(name);
                var descriptionAttribute = memInfo[0]
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;

                //set toogle name
                if (descriptionAttribute != null)
                {
                    element.text = descriptionAttribute.Description;
                }
                else
                {
                    element.text = names[i];
                }

                //register callback on toggle value changed
                element.RegisterValueChangedCallback(
                    (ChangeEvent<bool> boolcbk) =>
                    {
                        if (m_isFlagMode == false)
                        {
                            if (boolcbk.newValue == false)
                            {
                                m_toggleList[index].SetValueWithoutNotify(true);
                                return;
                            }

                            m_enum = (TEnum)Enum.Parse(typeof(TEnum), name);
                            SetOtherValuefalse(index);
                        }
                        else
                        {
                            TEnum v = (TEnum)Enum.Parse(typeof(TEnum), name);
                            if (boolcbk.newValue == true)
                            {
                                m_enum = (TEnum)Enum.ToObject(
                                    typeof(TEnum),
                                    m_enum.ToInt64(null) | v.ToInt64(null));
                            }
                            else
                            {
                                m_enum = (TEnum)Enum.ToObject(
                                    typeof(TEnum),
                                    m_enum.ToInt64(null) & ~v.ToInt64(null));
                            }
                        }
                        onClick?.Invoke(m_enum);
                    });

                //set toggle initial value
                if (m_isFlagMode)
                {
                    if (m_enum.HasFlag((TEnum)Enum.Parse(typeof(TEnum), name)))
                    {
                        element.SetValueWithoutNotify(true);
                    }
                }
                else
                {
                    if (m_enum.Equals((TEnum)Enum.Parse(typeof(TEnum), name)))
                    {
                        element.SetValueWithoutNotify(true);
                    }
                }

                Add(element);
                m_toggleList.Add(element);
            }
        }

        private void SetOtherValuefalse(int index)
        {
            for (int i = 0; i < m_toggleList.Count; i++)
            {
                Toggle item = m_toggleList[i];
                if (index != i)
                {
                    item.SetValueWithoutNotify(false);
                }
            }
        }
    }
}
