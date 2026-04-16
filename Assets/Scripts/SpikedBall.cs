using UnityEngine;

public class MaceTrap : Trap 
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerDeath(collision.gameObject, collision.GetContact(0).point);
        }
    }
}