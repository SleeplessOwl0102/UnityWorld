using System.Collections.Generic;
using UnityEditor;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public interface IColumnDefineProvider
    {
        public bool ShowSelectItemDetail { get; }

        public bool ShowSelectModelPreiview { get; }

        public IReadOnlyList<TableColumn> ColumnList { get; }

        public string ListRootSerializePath { get; }

        public string PreviewSerializePath { get; }

        public IReadOnlyList<string> SearchablePathList { get; }
    }

    public interface IMaxHeight
    {
        public int MaxHeight { get; }
        public int Height { get; }
    }
}