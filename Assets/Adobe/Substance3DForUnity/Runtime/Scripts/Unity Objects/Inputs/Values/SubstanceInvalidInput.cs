using System;
using UnityEngine;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInvalidInput : SubstanceInputBase
    {
        public override bool IsValid => false;
        public override SubstanceValueType ValueType => Description.Type;
        public override bool IsNumeric => false;

        public SubstanceInvalidInput(int index, int graphID)
        {
            Index = index;
            GraphID = graphID;
        }

        public override void UpdateNativeHandle(SubstanceNativeHandler handler)
        {
            return;
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