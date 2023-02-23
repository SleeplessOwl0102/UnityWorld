using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputString : SubstanceInputBase
    {
        [Multiline]
        public string Data;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.String;
        public override bool IsNumeric => false;

        internal SubstanceInputString(int index, int graphID, DataInternalNumeric data)
        {
            Index = index;
            GraphID = graphID;
            Data = Marshal.PtrToStringAnsi(data.mPtr);
        }

        public override void UpdateNativeHandle(SubstanceNativeHandler handler)
        {
            handler.SetInputString(Data, Index, GraphID);
        }

        internal override void SetNumericDescription(NativeNumericInputDesc desc)
        {
            return;
        }

        internal override void SetEnumOptions(NativeEnumInputDesc[] options)
        {
            return;
        }
    }
}