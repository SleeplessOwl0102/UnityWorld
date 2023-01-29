using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RL.EditorCore
{
    public static class UIElement_Util
    {

    }


    public static class HelperMethod
    {
        #region IStyle

        /// <summary>
        /// 设置4面的Margin(外扩)
        /// 
        /// Flex Tip：
        /// Mergin => BorderLine => Pending => Content
        /// </summary>
        /// <param name="style"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IStyle SetMarginAll(this IStyle style, float length)
        {
            style.marginLeft = length;
            style.marginRight = length;
            style.marginTop = length;
            style.marginBottom = length;
            return style;
        }

        #endregion
    }

}
