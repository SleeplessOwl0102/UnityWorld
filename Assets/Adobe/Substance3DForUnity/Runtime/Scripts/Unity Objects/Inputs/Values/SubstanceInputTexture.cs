using System;
using UnityEngine;
using Adobe.Substance.Input.Description;
using System.Runtime.InteropServices;

namespace Adobe.Substance.Input
{
    [System.Serializable]
    public class SubstanceInputTexture : SubstanceInputBase
    {
        [SerializeField]
        private Texture2D Data;

        public override bool IsValid => true;
        public override SubstanceValueType ValueType => SubstanceValueType.Image;
        public override bool IsNumeric => false;

        internal SubstanceInputTexture(int index, int graphID, DataInternalNumeric data)
        {
            Index = index;
            GraphID = graphID;
            Data = null;
        }

        public override void UpdateNativeHandle(SubstanceNativeHandler handler)
        {
            if (Data == null)
                return;

            if (!Data.isReadable)
            {
                Debug.LogWarning($"Input textures must be set as readable. Texture assigned to {Description.Identifier} will have no effect.");
                return;
            }

            var pixels = Data.GetPixels32();
            handler.SetInputTexture2D(pixels, Data.width, Data.height, Index, GraphID);
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