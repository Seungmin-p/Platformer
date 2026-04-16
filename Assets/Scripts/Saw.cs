using UnityEngine;

public class Saw : Trap
{
    private float rotationSpeed = 150f;
    
    private void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerDeath(collision.gameObject, collision.GetContact(0).point);
        }
    }
}