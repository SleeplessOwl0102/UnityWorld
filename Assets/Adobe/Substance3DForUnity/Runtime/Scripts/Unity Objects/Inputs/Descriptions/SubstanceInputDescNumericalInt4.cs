using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance.Input.Description
{
    public class SubstanceInputDescNumericalInt4 : ISubstanceInputDescNumerical
    {
        public int[] DefaultValue;

        public int[] MinValue;

        public int[] MaxValue;

        public int[] SliderStep;

        public bool SliderClamp;

        public int EnumValueCount;

        public SubstanceInt4EnumOption[] EnumValues;
    }

    [System.Serializable]
    public class SubstanceInt4EnumOption
    {
        public int[] Value;

        public string Label;
    }
}