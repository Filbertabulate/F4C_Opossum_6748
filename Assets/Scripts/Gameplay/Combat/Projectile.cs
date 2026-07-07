using UnityEngine;
public class Projectile : MonoBehaviour
{
    public float speed = 5f; // How fast the projectile flies
    private int damage;      // Damage passed from the unit
    private string targetTag; // Tag of the enemy to hit
    private int moveDirection; // 1 for right, -1 for left

    // This method is called by the unit when it fires the projectile
    public void Initialize(int unitDamage, string tagToHit, int direction)
    {
        damage = unitDamage;
        targetTag = tagToHit;
        moveDirection = direction;

        // Destroy this projectile after 5 seconds automatically so they don't fly forever and cause lag
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        // Move the projectile forward every frame
        transform.position += Vector3.right * moveDirection * speed * Time.deltaTime;
    }

    // This built-in Unity method detects when colliders touch
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the thing we hit has the tag we want to destroy (e.g., "Enemy" or "Player")
        if (collision.CompareTag(targetTag))
        {
            // Try to find the health system of the target
            HealthSystem targetHealth = collision.GetComponentInParent<HealthSystem>();

            if (targetHealth != null)
            {
                // Deal damage and destroy the projectile
                targetHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}