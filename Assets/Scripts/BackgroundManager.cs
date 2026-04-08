using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] SpriteRenderer bgRenderer;
    [SerializeField] Material[] bgMaterials;

    void Start()
    {
        if (bgMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, bgMaterials.Length);
            
            //랜덤 인덱스값으로 머티리얼 설정
            bgRenderer.material = bgMaterials[randomIndex];
        }
    }
}
