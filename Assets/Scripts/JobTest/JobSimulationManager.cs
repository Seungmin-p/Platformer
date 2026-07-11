using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

//Job 시스템은 배열을 만들때 NativeArray 사용
//얘는 왜 GC Alloc이 안생기지? -> C++에서 네이티브 어레이는 스택에 할당

public class JobSimulationManager : MonoBehaviour
{
    public GameObject prefab;
    public int objectCount = 10000;
    
    private Transform[] transforms;
    private NativeArray<Vector3> startPositions;
    private NativeArray<Vector3> targetPositions;
    
    void Start()
    {
        transforms = new Transform[objectCount];
        startPositions = new NativeArray<Vector3>(objectCount, Allocator.Persistent);
        targetPositions = new NativeArray<Vector3>(objectCount, Allocator.Persistent);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-100.0f, 100.0f), 0, Random.Range(-100.0f, 100.0f));
            GameObject go = GameObject.Instantiate(prefab, randomPos, Quaternion.identity);
            
            transforms[i] = go.transform;
            startPositions[i] = randomPos;
        }
    }

    void Update()
    {
        MovementJob job = new MovementJob
        {
            startPositions = this.startPositions,
            targetPositions = this.targetPositions,
            time = Time.time
        };
        
        JobHandle jobHandle = job.Schedule(objectCount, new JobHandle());
        
        jobHandle.Complete();
        
        for (int i = 0; i < objectCount; i++)
        {
            transforms[i].position = targetPositions[i];
        }
    }
}

public struct MovementJob : IJobFor
{
    [ReadOnly] public NativeArray<Vector3> startPositions;
    public NativeArray<Vector3> targetPositions;
    public float time;
    
    public void Execute(int index)
    {
        Vector3 pos = startPositions[index];
        
        pos.y += Mathf.Sin(time + index) * 2f;
        targetPositions[index] = pos;
    }
}
