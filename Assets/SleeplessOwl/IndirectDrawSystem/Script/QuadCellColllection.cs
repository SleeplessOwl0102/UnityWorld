using System;
using System.Collections.Generic;
using EasyButtons;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Ren
{
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "QuadCellColllection", menuName = "Ren Rendering/QuadCellColllection")]
    public class QuadCellColllection : ScriptableObject
    {
        //smaller the number, CPU needs more time, but GPU is faster
        public float cellSize = 10; //unity unit (m)
        public Bounds renderBound = new Bounds();
        public List<int> cellPosArrayCount; //每個區塊中物件數量
        public List<Bounds> cellPosArrayBound; //每個區塊的範圍
        public Vector3[] flatenArrayPosSortByCell; //按cell排序後，給compute shader使用的array

        //temp data
        [NonSerialized] public int cellCountX = -1;
        [NonSerialized] private int cellCountZ = -1;
        [NonSerialized] private List<int> visibleCellIds = new List<int>();
        private Plane[] cameraFrustumPlanes = new Plane[6];
        private List<Vector3> tempAllPos = new List<Vector3>();


        public void Assign(List<Vector3> pos)
        {
            tempAllPos = pos;
        }

        public void Add(Vector3 pos)
        {
            tempAllPos.Add(pos);
        }

        [Button]
        public void Clear()
        {
            tempAllPos.Clear();
            cellPosArrayBound.Clear();
            cellPosArrayCount.Clear();
            flatenArrayPosSortByCell = new Vector3[0];
        }

        [Button]
        public void RecalculateData()
        {
            //recalculate bound
            float3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < tempAllPos.Count; i++)
            {
                Vector3 target = tempAllPos[i];
                min = Vector3.Min(min, target);
                max = Vector3.Max(max, target);
            }

            //計算區塊數量
            cellCountX = Mathf.Max(1, Mathf.CeilToInt((max.x - min.x) / cellSize));
            cellCountZ = Mathf.Max(1, Mathf.CeilToInt((max.z - min.z) / cellSize));
            var cellPosArray = new List<Vector3>[cellCountX * cellCountZ]; //flatten 2D array
            cellPosArrayCount = new List<int>(cellCountX * cellCountZ);
            for (int i = 0; i < cellPosArray.Length; i++)
            {
                cellPosArray[i] = new List<Vector3>();
            }

            //binning, put each posWS into the correct cell
            for (int i = 0; i < tempAllPos.Count; i++)
            {
                Vector3 pos = tempAllPos[i];

                //find cellID
                //use min to force within 0~[cellCount-1]  
                int xID = Mathf.Min(cellCountX - 1, Mathf.FloorToInt(Mathf.InverseLerp(min.x, max.x, pos.x) * cellCountX));
                int zID = Mathf.Min(cellCountZ - 1, Mathf.FloorToInt(Mathf.InverseLerp(min.z, max.z, pos.z) * cellCountZ));

                cellPosArray[xID + zID * cellCountX].Add(pos);
            }

            cellPosArrayBound.Clear();
            for (int i = 0; i < cellPosArray.Length; i++)
            {
                cellPosArrayCount.Add(cellPosArray[i].Count);


                //計算每個Cell的範圍
                Vector3 cellMin, cellMax;
                cellMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                cellMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                for (int j = 0; j < cellPosArray[i].Count; j++)
                {
                    cellMin = Vector3.Min(cellMin, cellPosArray[i][j]);
                    cellMax = Vector3.Max(cellMax, cellPosArray[i][j]);

                }
                Bounds bound = new Bounds();
                bound.SetMinMax(cellMin, cellMax);
                cellPosArrayBound.Add(bound);
            }

            //combine to a flatten array for compute buffer
            int offset = 0;
            flatenArrayPosSortByCell = new Vector3[tempAllPos.Count];
            for (int i = 0; i < cellPosArray.Length; i++)
            {
                for (int j = 0; j < cellPosArray[i].Count; j++)
                {
                    flatenArrayPosSortByCell[offset] = cellPosArray[i][j];
                    offset++;
                }
            }

            renderBound.SetMinMax(min, max);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public List<int> GetVisibleCellByCamera(Camera camera, float maxDistance)
        {
            visibleCellIds.Clear();
            float oriFarClipPlan = camera.farClipPlane;
            camera.farClipPlane = maxDistance;
            GeometryUtility.CalculateFrustumPlanes(camera, cameraFrustumPlanes);
            camera.farClipPlane = oriFarClipPlan;//revert far plane edit

            for (int i = 0; i < cellPosArrayBound.Count; i++)
            {
                if (GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, cellPosArrayBound[i]))
                {
                    visibleCellIds.Add(i);
                }
            }
            return visibleCellIds;
        }
    }
}