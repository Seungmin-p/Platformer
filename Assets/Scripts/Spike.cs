using UnityEngine;

public class Spike : Trap
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //부모클래스 메소드 이용
            TriggerDeath(collision.gameObject, collision.GetContact(0).point);
        }
    }
}
