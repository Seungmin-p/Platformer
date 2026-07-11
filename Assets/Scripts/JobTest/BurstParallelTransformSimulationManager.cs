using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class BurstParallelTransformSimulationManager : MonoBehaviour
{
    public GameObject prefab;
    public int objectCount = 10000;
    
    private TransformAccessArray transformsAccessArray;
    private NativeArray<Vector3> startPositions;
    
    void Start()
    {
        transformsAccessArray = new TransformAccessArray(objectCount);
        
        startPositions = new NativeArray<Vector3>(objectCount, Allocator.Persistent);

        Transform[] tempTransforms = new Transform[objectCount];
        
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-100.0f, 100.0f), 0, Random.Range(-100.0f, 100.0f));
            GameObject go = GameObject.Instantiate(prefab, randomPos, Quaternion.identity);
            
            tempTransforms[i] = go.transform;
            startPositions[i] = randomPos;
        }
        
        transformsAccessArray.SetTransforms(tempTransforms);
    }

    void Update()
    {
        BurstParallelTransformJob job = new BurstParallelTransformJob
        {
            startPositions = this.startPositions,
            time = Time.time
        };
        
        JobHandle jobHandle = job.Schedule(transformsAccessArray);
        
        jobHandle.Complete();
    }
}

[BurstCompile]
public struct BurstParallelTransformJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<Vector3> startPositions;
    public float time;

    public void Execute(int index, TransformAccess transform)
    {
        Vector3 pos = startPositions[index];
        
        pos.y += Mathf.Sin(time + index) * 2f;
        transform.position = pos;
    }
}