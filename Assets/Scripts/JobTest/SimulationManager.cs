using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject prefab;
    public int objectCount = 10000;
    
    private Transform[] transforms;
    private Vector3[] startPositions;
    
    void Start()
    {
        transforms = new Transform[objectCount];
        startPositions = new Vector3[objectCount];

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
        float time = Time.time;

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 pos = startPositions[i];
            pos.y += Mathf.Sin(time + i) * 2f;
            
            transforms[i].position = pos;
        }
    }
}
