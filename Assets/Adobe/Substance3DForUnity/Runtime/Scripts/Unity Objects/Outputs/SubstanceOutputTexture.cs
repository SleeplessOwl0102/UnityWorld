using UnityEngine;

namespace Adobe.Substance
{
    [System.Serializable]
    public class SubstanceOutputTexture
    {
        [SerializeField]
        public int Index;

        [SerializeField]
        public int VirtualOutputIndex;

        [SerializeField]
        public int GraphIndex;

        [SerializeField]
        public SubstanceOutputDescription Description;

        [SerializeField]
        public Texture2D OutputTexture;

        [SerializeField]
        public bool sRGB;

        [SerializeField]
        public bool IsVirtual;

        [SerializeField]
        public bool IsStandardOutput;

        [SerializeField]
        public bool IsAlphaAssignable;

        [SerializeField]
        public string AlphaChannel;

        [SerializeField]
        public bool InvertAssignedAlpha;

        [SerializeField]
        public uint Flags = 0;

        public SubstanceOutputTexture(SubstanceOutputDescription description, int graphIndex, bool isStandard)
        {
            Index = description.Index;
            GraphIndex = graphIndex;
            Description = description;
            IsStandardOutput = isStandard;
            IsAlphaAssignable = !string.Equals(description.Channel, "normal", System.StringComparison.OrdinalIgnoreCase);
            IsVirtual = false;
            sRGB = false;
            OutputTexture = null;
            AlphaChannel = string.Empty;
        }
    }
}