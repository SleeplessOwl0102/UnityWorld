
    using UnityEngine;
    using UnityEngine.UIElements;

    public interface IPreviewTextureGetter
    {
        public Texture2D GetPreviewTexture();
    }


    public static class StyleExtension
    {
        /// <summary>
        /// 设置4面的Margin(外扩)
        /// 
        /// Flex Tip：
        /// Mergin => BorderLine => Pending => Content
        /// </summary>
        /// <param name="style"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IStyle SetMarginAll(this IStyle style, float length = 5)
        {
            style.marginLeft = length;
            style.marginRight = length;
            style.marginTop = length;
            style.marginBottom = length;
            return style;
        }

        public static IStyle SetBorderWidthAll(this IStyle style, float length = 5)
        {
            style.borderTopWidth = length;
            style.borderLeftWidth = length;
            style.borderBottomWidth = length;
            style.borderRightWidth = length;
            return style;
        }

        public static IStyle SetPaddingWidthAll(this IStyle style, float length = 5)
        {
            style.paddingBottom = length;
            style.paddingLeft = length;
            style.paddingRight = length;
            style.paddingTop = length;
            return style;
        }

        public static IStyle SetBorderColorAll(this IStyle style, Color color)
        {
            style.borderTopColor = color;
            style.borderLeftColor = color;
            style.borderBottomColor = color;
            style.borderRightColor = color;
            return style;
        }

        public static IStyle SetHeightSafe(this IStyle style, StyleLength height)
        {
            return SetHeightSafe(style, height.value.value); ;
        }

        public static IStyle SetHeightSafe(this IStyle style, float height)
        {
            if (height > style.minHeight.value.value && height < style.maxHeight.value.value)
                style.height = height;
            else if (height <= style.minHeight.value.value)
                style.height = style.minHeight;
            else if (height >= style.maxHeight.value.value)
                style.height = style.maxHeight;

            return style;
        }

        public static IStyle SetWidthSafe(this IStyle style, StyleLength width)
        {
            return SetWidthSafe(style, width.value.value);
        }

        public static IStyle SetWidthSafe(this IStyle style, float width)
        {
            if (width > style.minWidth.value.value && width < style.maxWidth.value.value)
                style.width = width;
            else if(width <= style.minWidth.value.value)
                style.width = style.minWidth;
            else if(width >= style.maxWidth.value.value)
                style.width = style.maxWidth;

            return style;
        }

        public static IStyle SetFlexGrow(this IStyle style, float value = 1)
        {
            style.flexGrow = value;
            return style;
        }


        public static float GetWidth(this VisualElement visualElemen)
        {
            return visualElemen.style.width.value.value;
        }

        public static float GetHeight(this VisualElement visualElemen)
        {
            return visualElemen.style.height.value.value;
        }

        public static Vector2 GetSize(this VisualElement visualElemen)
        {
            return new Vector2(visualElemen.GetWidth(), visualElemen.GetHeight());
        }

        public static VisualElement SetSize(this VisualElement visualElemen, Vector2 size)
        {
            visualElemen.style.SetWidthSafe(size.x).SetHeightSafe(size.y);
            return visualElemen;
        }

        public static VisualElement SetWidth(this VisualElement visualElemen, float width)
        {
            visualElemen.style.SetWidthSafe(width);
            return visualElemen;
        }

        public static VisualElement SetHeight(this VisualElement visualElemen, float height)
        {
            visualElemen.style.SetHeightSafe(height);
            return visualElemen;
        }
    }