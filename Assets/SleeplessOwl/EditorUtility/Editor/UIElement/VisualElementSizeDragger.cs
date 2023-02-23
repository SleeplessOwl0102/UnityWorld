namespace SleeplessOwl.EditorUtil.UIElement
{
    using UnityEngine;
    using UnityEngine.UIElements;
    
    /// <summary>
    /// 提供VisualElement拖动右下角改变大小的功能
    /// </summary>
    public class VisualElementSizeDragger : MouseManipulator
    {
        private bool m_Focus;

        public VisualElementSizeDragger()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            m_Focus = false;

            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        protected void OnMouseDown(MouseDownEvent evt)
        {
            var orect = target.contentRect;
            var nrect = new Rect(orect.max - Vector2.one * 40, Vector2.one * 40);

            if (CanStartManipulation(evt) && nrect.Contains(evt.localMousePosition))
            {
                m_Focus = true;
                target.BringToFront();
                target.CaptureMouse();
            }
        }

        protected void OnMouseUp(MouseUpEvent evt)
        {
            if (CanStopManipulation(evt))
            {
                target.ReleaseMouse();
                m_Focus = false;
            }
        }

        protected void OnMouseCaptureOut(MouseCaptureOutEvent evt)
        {
            m_Focus = false;
        }

        protected void OnMouseMove(MouseMoveEvent evt)
        {
            if (m_Focus == false)
                return;

            target.SetSize(target.GetSize() + evt.mouseDelta);
        }
    }
}
