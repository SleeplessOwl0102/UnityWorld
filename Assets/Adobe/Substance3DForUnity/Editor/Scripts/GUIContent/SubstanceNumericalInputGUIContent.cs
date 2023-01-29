using Adobe.Substance;
using Adobe.Substance.Input.Description;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Custome GUIContent class that provides extra information for drawing numeric input parameters.
    /// </summary>
    internal class SubstanceNumericalInputGUIContent : SubstanceInputGUIContent
    {
        /// <summary>
        /// Numerical input description for the target SerializedProperty.
        /// </summary>
        public ISubstanceInputDescNumerical NumericalDescription;

        public SubstanceNumericalInputGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, ISubstanceInputDescNumerical numDescription) : base(description, dataProp)
        {
            NumericalDescription = numDescription;
        }

        public SubstanceNumericalInputGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, ISubstanceInputDescNumerical numDescription, string text) : base(description, dataProp, text)
        {
            NumericalDescription = numDescription;
        }
    }

    internal class SubstanceIntComboBoxGUIContent : SubstanceNumericalInputGUIContent
    {
        public GUIContent[] EnumValuesGUI { get; }

        public int[] EnumValues { get; }

        public SubstanceIntComboBoxGUIContent(SubstanceInputDescription description, SubstanceInputDescNumericalInt intDescription, SerializedProperty dataProp, ISubstanceInputDescNumerical numDescription) : base(description, dataProp, numDescription)
        {
            var enumValues = intDescription.EnumValues;

            EnumValuesGUI = new GUIContent[enumValues.Length];
            EnumValues = new int[enumValues.Length];

            for (int i = 0; i < EnumValuesGUI.Length; i++)
            {
                var enumElement = enumValues[i];
                EnumValuesGUI[i] = new GUIContent(enumElement.Label);
                EnumValues[i] = enumElement.Value;
            }
        }
    }
}