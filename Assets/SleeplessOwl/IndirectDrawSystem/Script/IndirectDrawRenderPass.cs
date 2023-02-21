using Ren;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenLai.IndirectDraw
{
    public class ShaderIDs
    {
        public const string IndirectDrawLod = "INDIREDCT_DRAWING_LOD";
        public const string DepthCulling = "DEPTH_CULLING";
    }
    
    public interface IIndirectDrawSystrem
    {
        public void Add(InstanceRenderSetting data);

        public void Remove(string name);


        public int RenderCount { get; }
    }



    public class IndirectDrawRenderPass : ScriptableRenderPass, IIndirectDrawSystrem
    {
        
        #region IIndirectDrawSystrem

        public static IIndirectDrawSystrem Instance;

        void IIndirectDrawSystrem.Add(InstanceRenderSetting data)
        {
            if (cachBufferData.Contains(data))
                return;

            cachBufferData.Add(data);
        }

        void IIndirectDrawSystrem.Remove(string name)
        {
            var target = cachBufferData.Find(x => x.description == name);
            if (target == null)
                return;

            if (BufferCaches.ContainsKey(target))
            {
                var item = BufferCaches[target];
                if (item.allInstancesPosWSBuffer != null)
                    item.allInstancesPosWSBuffer.Release();

                if (item.visibleInstancesOnlyPosWSIDBuffer != null)
                    item.visibleInstancesOnlyPosWSIDBuffer.Release();

                if (item.argsBuffer != null)
                    item.argsBuffer.Release();

                item.allInstancesPosWSBuffer = null;
                item.visibleInstancesOnlyPosWSIDBuffer = null;
                item.argsBuffer = null;

                BufferCaches.Remove(target);
            }

            cachBufferData.Remove(target);
        }

        public int RenderCount => cachBufferData.Count;

        #endregion
        
        
        public static bool ForceRefresh = false;
        public static bool ForceDisable = false;

        private string displayName;

        private ComputeShader cullingComputeShader;

        private IndirectDrawRendererFeature config;
        public List<InstanceRenderSetting> cachBufferData;
        
        public Dictionary<InstanceRenderSetting, BuffersCache> BufferCaches = new Dictionary<InstanceRenderSetting, BuffersCache>();

        
        public IndirectDrawRenderPass(IndirectDrawRendererFeature obj)
        {
            Instance = this;
            cachBufferData = new List<InstanceRenderSetting>();

            config = obj;
            displayName = "IndirectRender";
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            cullingComputeShader = obj.cullingComputeShader;
        }

        internal void OnDispose()
        {

            foreach (var iter in BufferCaches)
            {
                var item = iter.Value;
                if (item.allInstancesPosWSBuffer != null)
                    item.allInstancesPosWSBuffer.Release();

                if (item.visibleInstancesOnlyPosWSIDBuffer != null)
                    item.visibleInstancesOnlyPosWSIDBuffer.Release();

                if (item.argsBuffer != null)
                    item.argsBuffer.Release();

                item.allInstancesPosWSBuffer = null;
                item.visibleInstancesOnlyPosWSIDBuffer = null;
                item.argsBuffer = null;

                //lod
                if (item.visibleInstancesOnlyPosWSIDBuffer_Lod != null)
                    item.visibleInstancesOnlyPosWSIDBuffer_Lod.Release();

                if (item.argsBuffer_Lod != null)
                    item.argsBuffer_Lod.Release();

                item.visibleInstancesOnlyPosWSIDBuffer_Lod = null;
                item.argsBuffer_Lod = null;
            }

            BufferCaches.Clear();
        }

        public class BuffersCache
        {
            public int preCount;
            public ComputeBuffer allInstancesPosWSBuffer;
            public ComputeBuffer visibleInstancesOnlyPosWSIDBuffer;
            public ComputeBuffer visibleInstancesOnlyPosWSIDBuffer_Lod;
            public ComputeBuffer argsBuffer;
            public ComputeBuffer argsBuffer_Lod;
        }

        public void CreatAllPosBuffer(InstanceRenderSetting data, int allObjCount)
        {
            Debug.Log("CreatAllPosBuffer");
            var bar = new BuffersCache();
            if (BufferCaches.ContainsKey(data) == false)
                BufferCaches.Add(data, bar);
            bar = BufferCaches[data];
            var collection = data.quadCellPos;
            var allPosCount = collection.flatenArrayPosSortByCell.Length;

            // allInstancesPosWS Buffer
            if (bar.allInstancesPosWSBuffer != null)
                bar.allInstancesPosWSBuffer.Release();


            bar.allInstancesPosWSBuffer = new ComputeBuffer(allPosCount, sizeof(float) * 3); //float3 posWS only, per grass
            bar.allInstancesPosWSBuffer.SetData(collection.flatenArrayPosSortByCell);

            var instanceMaterial = data.material;
            instanceMaterial.SetBuffer("_AllInstancesTransformBuffer", bar.allInstancesPosWSBuffer);


            // AllPos buffer
            if (bar.visibleInstancesOnlyPosWSIDBuffer != null)
                bar.visibleInstancesOnlyPosWSIDBuffer.Release();
            bar.visibleInstancesOnlyPosWSIDBuffer = new ComputeBuffer(allPosCount, sizeof(uint), ComputeBufferType.Append);
            instanceMaterial.SetBuffer("_VisibleInstancesIDBuffer", bar.visibleInstancesOnlyPosWSIDBuffer);

            if (data.EnableLOD)
            {
                data.lod_Material.SetBuffer("_AllInstancesTransformBuffer", bar.allInstancesPosWSBuffer);

                if (bar.visibleInstancesOnlyPosWSIDBuffer_Lod != null)
                    bar.visibleInstancesOnlyPosWSIDBuffer_Lod.Release();
                bar.visibleInstancesOnlyPosWSIDBuffer_Lod = new ComputeBuffer(allPosCount, sizeof(uint), ComputeBufferType.Append);
                data.lod_Material.SetBuffer("_VisibleInstancesIDBuffer", bar.visibleInstancesOnlyPosWSIDBuffer_Lod);

                if (bar.argsBuffer_Lod != null)
                    bar.argsBuffer_Lod.Release();

                var mesh2 = data.lod_Mesh;
                uint[] args2 = new uint[5] { 0, 0, 0, 0, 0 };

                bar.argsBuffer_Lod = new ComputeBuffer(1, args2.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

                args2[0] = (uint)mesh2.GetIndexCount(0);
                args2[1] = (uint)allObjCount;
                args2[2] = (uint)mesh2.GetIndexStart(0);
                args2[3] = (uint)mesh2.GetBaseVertex(0);
                args2[4] = 0;

                bar.argsBuffer_Lod.SetData(args2);
            }

            //args buffer
            if (bar.argsBuffer != null)
                bar.argsBuffer.Release();

            var mesh = data.mesh;
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

            bar.argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)allObjCount;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            args[4] = 0;

            bar.argsBuffer.SetData(args);
        }


        private Plane[] planes = new Plane[6];

        private void RenderPerIndirectData(CommandBuffer cb, InstanceRenderSetting data, ref RenderingData renderingData)
        {
            Camera cam = Camera.main;
            ref ScriptableRenderer renderer = ref renderingData.cameraData.renderer;

            var quadCell = data.quadCellPos;
            var allPosCount = quadCell.flatenArrayPosSortByCell.Length;

            if (allPosCount == 0)
                return;

            /////////////////
            var far = cam.farClipPlane;
            cam.farClipPlane = data.visibleDistance;
            GeometryUtility.CalculateFrustumPlanes(cam, planes);
            if (GeometryUtility.TestPlanesAABB(planes, quadCell.renderBound) == false)
                return;
            cam.farClipPlane = far;

            //////////////////////////
            //Create Buffer
            bool needReCreatBuffer = ForceRefresh
                || BufferCaches.ContainsKey(data) == false
                || BufferCaches[data].preCount != allPosCount
                || (data.EnableLOD && BufferCaches[data].argsBuffer_Lod == null);
            if (needReCreatBuffer)
            {
                if (cachBufferData.Contains(data) == false)
                    cachBufferData.Add(data);

                CreatAllPosBuffer(data, allPosCount);
                BufferCaches[data].preCount = allPosCount;
            }

            bool isSceneViewCamera = renderingData.cameraData.isSceneViewCamera; 

            var bar = BufferCaches[data];
            if (isSceneViewCamera == false)
            {
                cb.SetComputeBufferParam(cullingComputeShader, 0, "_AllInstancesPosWSBuffer", bar.allInstancesPosWSBuffer);
                cb.SetComputeBufferParam(cullingComputeShader, 0, "_VisibleInstancesIDBuffer", bar.visibleInstancesOnlyPosWSIDBuffer);
                cb.SetBufferCounterValue(bar.visibleInstancesOnlyPosWSIDBuffer, 0);

                Matrix4x4 v = cam.worldToCameraMatrix;
                Matrix4x4 p = cam.projectionMatrix;
                Matrix4x4 vp = p * v;
                cb.SetComputeMatrixParam(cullingComputeShader, "_VPMatrix", vp);
                cb.SetComputeFloatParam(cullingComputeShader, "_MaxDrawDistance", data.visibleDistance);
                cb.SetComputeFloatParam(cullingComputeShader, "_FadeRange1", data.fadeRange);

                //LOD
                if (data.EnableLOD)
                {
                    cb.EnableShaderKeyword(ShaderIDs.IndirectDrawLod);
                    cb.SetComputeBufferParam(cullingComputeShader, 0, "_VisibleInstancesIDBuffer_Lod", bar.visibleInstancesOnlyPosWSIDBuffer_Lod);
                    cb.SetBufferCounterValue(bar.visibleInstancesOnlyPosWSIDBuffer_Lod, 0);
                    cb.SetComputeFloatParam(cullingComputeShader, "_LodThreshhold", data.lod_Threshold);
                    cb.SetComputeFloatParam(cullingComputeShader, "_FadeRange2", data.lod_FadeRange);
                }
                else
                {
                    cb.DisableShaderKeyword(ShaderIDs.IndirectDrawLod);
                }
                //////////////////////////

                //dispatch per visible cell
                int dispatchCount = 0;

                List<int> visibleCellIDList = data.quadCellPos.GetVisibleCellByCamera(cam, data.visibleDistance);


                for (int i = 0; i < visibleCellIDList.Count; i++)
                {
                    int targetCellFlattenID = visibleCellIDList[i];
                    int memoryOffset = 0;
                    for (int j = 0; j < targetCellFlattenID; j++)
                    {
                        memoryOffset += data.quadCellPos.cellPosArrayCount[j];
                    }
                    cb.SetComputeIntParam(cullingComputeShader, "_StartOffset", memoryOffset);//culling read data started at offseted pos, will start from cell's total offset in memory
                    int jobLength = data.quadCellPos.cellPosArrayCount[targetCellFlattenID];

                    //============================================================================================
                    //batch n dispatchs into 1 dispatch, if memory is continuous in allInstancesPosWSBuffer
                    if (true)
                    {
                        while ((i < visibleCellIDList.Count - 1) && //test this first to avoid out of bound access to visibleCellIDList
                                (visibleCellIDList[i + 1] == visibleCellIDList[i] + 1))
                        {
                            int newJobLength = jobLength + data.quadCellPos.cellPosArrayCount[visibleCellIDList[i + 1]];
                            if (newJobLength / 64 > 65535)
                                break;


                            //if memory is continuous, append them together into the same dispatch call
                            jobLength += data.quadCellPos.cellPosArrayCount[visibleCellIDList[i + 1]];
                            i++;
                        }
                    }
                    //============================================================================================
                    if (jobLength == 0)
                        continue;

                    //Depth Culling
                    if (data.enableDepthCulling)
                    {
                        
                        cb.EnableShaderKeyword(ShaderIDs.DepthCulling);
                        cb.SetComputeTextureParam(cullingComputeShader, 0, "_DepthTexture", renderer.cameraDepthTargetHandle);
                    }
                    else
                    {
                        cb.DisableShaderKeyword(ShaderIDs.DepthCulling);
                    }

                    cb.SetComputeIntParam(cullingComputeShader, "_MaxIndex", memoryOffset + jobLength);
                    //disaptch.X division number must match numthreads.x in compute shader (e.g. 64)
                    cb.DispatchCompute(cullingComputeShader, 0, Mathf.CeilToInt(jobLength / 64f), 1, 1);

                    dispatchCount++;
                }
            }
            
            cb.SetRenderTarget(renderer.cameraColorTargetHandle,renderer.cameraDepthTargetHandle);
            cb.CopyCounterValue(bar.visibleInstancesOnlyPosWSIDBuffer, bar.argsBuffer, 4);
            cb.DrawMeshInstancedIndirect(data.mesh, 0, data.material, 0, bar.argsBuffer);

            if (data.EnableLOD)
            {
                cb.CopyCounterValue(bar.visibleInstancesOnlyPosWSIDBuffer_Lod, bar.argsBuffer_Lod, 4);
                cb.DrawMeshInstancedIndirect(data.lod_Mesh, 0, data.lod_Material, 0, bar.argsBuffer_Lod);
            }


            if (data.enableZWrite)
            {
                CoreUtils.SetRenderTarget(cb, renderer.cameraDepthTargetHandle, renderer.cameraDepthTargetHandle, ClearFlag.None, Color.white);
                cb.DrawMeshInstancedIndirect(data.mesh, 0, data.material, 1, bar.argsBuffer);

                if (data.EnableLOD)
                {
                    cb.DrawMeshInstancedIndirect(data.lod_Mesh, 0, data.lod_Material, 1, bar.argsBuffer_Lod);
                }
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            bool isSceneView = renderingData.cameraData.isSceneViewCamera;
            bool isMainCamera = renderingData.cameraData.camera == Camera.main;


            if (isSceneView == false && isMainCamera == false)
                return;
            
            if (ForceDisable)
                return;
            
            if (cachBufferData == null)
                return;

            CommandBuffer cb = CommandBufferPool.Get();
            foreach (var renderData in cachBufferData)
            {
                if (renderData is null)
                {
                    Debug.Log($"renderData 为空");
                    continue;
                }

                if (renderData.CheckData() == false)
                {
                    Debug.Log($"renderData {renderData.description} 数据不完整");
                    continue;
                }

                if (renderData.enable == false)
                {
                    continue;
                }

                cb.name = displayName;
                RenderPerIndirectData(cb, renderData, ref renderingData);
            }
            ForceRefresh = false;
            cb.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            context.ExecuteCommandBuffer(cb);
            cb.Clear();

            CommandBufferPool.Release(cb);
        }

    }
}