using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    public class SubstanceFileSO : ScriptableObject
    {
        [SerializeField]
        public string AssetPath = default;

        [SerializeField]
        public List<SubstanceGraphSO> Instances;

        public SubstanceGraphSO GetDefaultGraph()
        {
            if (Instances == null || Instances.Count == 0)
                return null;

            return Instances[0];
        }
    }
}