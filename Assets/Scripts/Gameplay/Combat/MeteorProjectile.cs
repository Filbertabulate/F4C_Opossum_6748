using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("Meteor Settings")]
    public float speed = 10f;
    public int damage = 50;
    public float explosionRadius = 2.5f;
    
    [Header("Targeting")]
    // 
    public LayerMask enemyLayer;

    private void Update()
    {
        // Move the meteor diagonally down and to the right
        Vector3 fallDirection = new Vector3(1f, -1.5f, 0f).normalized;
        
        // Destroy if it falls way below the map
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Explode if it hits an enemy or the ground
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Find all colliders within the explosion radius on the enemy layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        // Deal damage to everything caught in the blast
        foreach (Collider2D hit in hits)
        {
            HealthSystem enemyHealth = hit.GetComponentInParent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Destroy the meteor
        Destroy(gameObject);
    }

    //
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}