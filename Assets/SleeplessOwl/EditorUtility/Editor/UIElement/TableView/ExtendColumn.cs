using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public class ExtendColumn<TItem> : Column
    {
        public string fieldPath;
        public string listRootFieldPath;
        public bool isEditable;


        public TableColumn.ExtendBehaviour customDrawer;

        public ExtendColumn()
        {

        }

        public ExtendColumn(TableColumn tableColumn)
        {
            title = string.IsNullOrEmpty(tableColumn.title) ? tableColumn.fieldPath : tableColumn.title;
            resizable = true;
            optional = true;
            stretchable = false;

            width = tableColumn.width;
            visible = tableColumn.visible;
            fieldPath = tableColumn.fieldPath;
            isEditable = tableColumn.IsEditable;

            customDrawer = tableColumn.CustomBehaviour;
            sortable = customDrawer is IComparer<TItem>;
        }

    }


}