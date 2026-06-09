using UnityEngine;

public class BoomEffect : MonoBehaviour
{
    [SerializeField] Animator anim;
        
    private void Start()
    {
        if (anim != null)
        {
            //애니메이션 클립의 길이 가져오기
            float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
            
            //해당 길이만큼 시간이 지난 후 자기 자신 삭제
            Destroy(gameObject, animLength);
        }
    }
}
