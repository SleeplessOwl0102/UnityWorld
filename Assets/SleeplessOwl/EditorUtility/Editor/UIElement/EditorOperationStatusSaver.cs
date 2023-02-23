namespace SleeplessOwl.EditorUtil.UIElement
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// 定义操作状态储存的方式
    /// </summary>
    public enum StateLifePeriod
    {
        /// <summary>
        /// 只储存在Unity开启期间
        /// </summary>
        AcrossCompile,
        
        /// <summary>
        /// 持久储存在注册表
        /// </summary>
        AcrossApplication,
    }

    public static class EditorContentStateHelper
    {
        /// <summary>
        /// 用来方便记录基于VisualElement实现的编辑器显示内容状态的扩展方法
        /// </summary>
        /// <param name="visualElement"></param>
        /// <param name="object"></param>
        /// <typeparam name="TStateData"></typeparam>
        /// <returns></returns>
        public static EditorOperationStatusSaver<TStateData> CreateOperationStatusSaverFromObject<TStateData>(
            this VisualElement visualElement, UnityEngine.Object @object) 
            where TStateData : new()
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(@object, out string guid, out long localid);
            return new EditorOperationStatusSaver<TStateData>(visualElement, key: guid + localid, serializationLifePeriod: StateLifePeriod.AcrossCompile);
        }
    }

    /// <summary>
    /// 会在VisualElement被关闭时储存当前状态
    /// </summary>
    /// <typeparam name="TStateData"></typeparam>
    public class EditorOperationStatusSaver<TStateData> where TStateData : new()
    {
        protected string m_key;
        protected StateLifePeriod m_serializationLifePeriod;
        private Action m_beforeDetatch;
        private TStateData m_state;

        public TStateData State
        {
            get => m_state; 
            set
            {
                if (value == null)
                    return;
                m_state = value;
            }
        }

        public ref TStateData RefState
        {
            get => ref m_state;
        }

        public EditorOperationStatusSaver(
            VisualElement visualElement,
            StateLifePeriod serializationLifePeriod = StateLifePeriod.AcrossApplication,
            string key = "", 
            Action beforeDetatch = null)
        {
            m_serializationLifePeriod = serializationLifePeriod;
            m_beforeDetatch = beforeDetatch;
            if (string.IsNullOrEmpty(key))
            {
                //todo lyx: find better way to generate key
                m_key = visualElement.GetType().Name + GetType().GetGenericArguments()[0].Name;
            }
            else
            {
                m_key = key;
            }
            
            visualElement.RegisterCallback<DetachFromPanelEvent>(SaveState);
            string str = LoadState();

            if (string.IsNullOrEmpty(str))
            {
                State = new TStateData();
            }
            else
            {
                State = JsonUtility.FromJson<TStateData>(str);
            }
        }

        private string LoadState()
        {
            switch (m_serializationLifePeriod)
            {
                case StateLifePeriod.AcrossCompile:
                    return SessionState.GetString(this.m_key, string.Empty);
                case StateLifePeriod.AcrossApplication:
                    return EditorPrefs.GetString(this.m_key, string.Empty);
                default:
                    return null;
            }
        }

        private void SaveState(DetachFromPanelEvent evt)
        {
            m_beforeDetatch?.Invoke();

            //Debug.Log(m_state.ToString());
            switch (m_serializationLifePeriod)
            {
                case StateLifePeriod.AcrossCompile:
                    SessionState.SetString(m_key, JsonUtility.ToJson(State));
                    break;
                case StateLifePeriod.AcrossApplication:
                    EditorPrefs.SetString(m_key, JsonUtility.ToJson(State));
                    break;
                default:
                    return;
            }
        }
    }
}
