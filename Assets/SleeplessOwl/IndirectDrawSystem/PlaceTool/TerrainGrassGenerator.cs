using EasyButtons;
using Ren;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TerrainGrassGenerator : MonoBehaviour
{
    public Terrain terrain;
    public BoxCollider boxBound;

    public int terrainControlMapIndex;

    public LayerMask targetLayer;
    public LayerMask ignoreLayer;

    public Texture2D noiseFilterTexture;

    public InstanceObjectDrawConfig config;
    public int renderDataIndex;
    public int count = 10000;

    [Button]
    public void Clear()
    {
        var data = config.instanceRenderSetting[renderDataIndex];
        data.quadCellPos.Clear();
    }

    [Button]
    public void GenerateParallel()
    {
        Debug.Log("Start Parallel Generate ");

        var textures = terrain.terrainData.alphamapTextures;


        var texture = textures[0];
        var vec3s = new NativeArray<Vector3>(count, Allocator.TempJob);
        var pass = new NativeArray<bool>(count, Allocator.TempJob);

        //raycast
        var raycastCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
        var raycastHits = new NativeArray<RaycastHit>(count, Allocator.TempJob);

        var jobWall = new RandomPos
        {
            position = vec3s,
            bound = boxBound.bounds,
            baseSeed = Random.Range(1, 999),
            command = raycastCommands,
            targetLayer = targetLayer,
            ignoreLayer = ignoreLayer
        };




        var handlerWall = jobWall.Schedule(count, 0);
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 1, handlerWall);
        handle.Complete();


        var poslist = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            var hit = raycastHits[i];
            vec3s[i] = hit.point;

            if (Vector3.Dot(hit.normal, Vector3.up) < .9f)
                continue;

            if (hit.collider.gameObject.layer == 0)
                continue;

            var coord = hit.textureCoord;
            var alpha = texture.GetPixelBilinear(coord.x, coord.y);

            if (Mathf.Pow(alpha.r,2) < Random.Range(0.3f, 1))
                continue;

            if(noiseFilterTexture != null)
            {
                coord = new Vector2(hit.point.x, hit.point.z) / 150;
                alpha = noiseFilterTexture.GetPixelBilinear(coord.x, coord.y);

                if (Mathf.Pow(alpha.r, 1) < Random.Range(0.4f, 1))
                    continue;

            }

            poslist.Add(vec3s[i]);
        }

        var data = config.instanceRenderSetting[renderDataIndex];
        data.quadCellPos.Assign(poslist);
        data.quadCellPos.RecalculateData();


        Debug.Log("Complete!!");

        vec3s.Dispose();
        pass.Dispose();
        raycastCommands.Dispose();
        raycastHits.Dispose();
    }


    struct RandomPos : IJobParallelFor
    {
        public NativeArray<Vector3> position;
        public NativeArray<RaycastCommand> command;

        public Bounds bound;
        public int baseSeed;

        public LayerMask targetLayer;
        public LayerMask ignoreLayer;

        public void Execute(int id)
        {
            var seed = (uint)(baseSeed + id);
            var rnd = new Unity.Mathematics.Random(seed);

            float randomX = rnd.NextFloat(bound.min.x, bound.max.x);
            float randomZ = rnd.NextFloat(bound.min.z, bound.max.z);
            position[id] = new Vector3(randomX, 300, randomZ);
            command[id] = new RaycastCommand(position[id], Vector3.down, layerMask: targetLayer | ignoreLayer);
        }
    }



}
