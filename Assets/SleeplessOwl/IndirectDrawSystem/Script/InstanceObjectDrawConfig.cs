using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ren
{
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "InstanceObjectDrawConfig", menuName = "Ren Rendering/InstanceObjectDrawConfig")]
    public class InstanceObjectDrawConfig : ScriptableObject
    {
        public string description;
        [FormerlySerializedAs("col")]
        public List<InstanceRenderSetting> instanceRenderSetting;
    }

    [Serializable]
    public class InstanceRenderSetting
    {
        public string description;

        public bool enable = true;
        public Mesh mesh;
        public Material material;
        public float visibleDistance = 150;
        public QuadCellColllection quadCellPos;
        public float fadeRange = 50;
        public bool enableDepthCulling = true;
        public bool enableZWrite = false;

        public bool EnableLOD;
        //[FormerlySerializedAs("LodThreshhold")]
        public float lod_Threshold = 0.5f;
        public float lod_FadeRange = 50;
        public Material lod_Material;
        public Mesh lod_Mesh;
        
        public bool CheckData()
        {
            if (mesh == null)
                return false;

            if (material == null)
                return false;

            if (visibleDistance <= 0)
                return false;

            if (quadCellPos == null)
                return false;

            return true;
        }

    }

}