using System;
using System.Collections.Generic;
using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("Meteor Movement")]

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private Vector2 fallDirection = new Vector2(1f, -1.5f);

    [Header("Meteor Damage")]
    [SerializeField]
    private int damage = 50;
    [SerializeField]
    private float explosionRadius = 2.5f;
    
    [Header("Targeting")]
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private LayerMask impactLayer;

    // Ro keep track if the explosion has taken placed or not.
    private bool hasExploded;

    // Public read-only states.
    public float Speed => speed;
    public int Damage => damage;
    public float ExplosionRadius => explosionRadius;
    public bool HasExploded => hasExploded;

    private void Update()
    {
        // For this fall direction, I would first want to check which direction the meteor should travel.
        // Normally I want it to fall diagonally towards the ground, i.e. downwards and slightly to the right.
        // However, if someone accidentally sets the direction to (0,0) in the Inspector, then normalising (0,0) 
        // still gives (0,0), meaning the meteor would never move. Therefore I perform a safety check to prevent
        // such cases from happening.
        // 
        // As such, if the direction is valid, I will normalise it so that only the direction remains,
        // while the movement speed comes entirely from "speed".
        //
        // Otherwise simply make the meteor fall straight down.
        Vector2 fallingdirection =  fallDirection.sqrMagnitude > 0.001f ? fallDirection.normalized : Vector2.down;

        // Now I will need to move the meteor every frame, whereby:
        // - speed          -> units travelled every second.
        // Time.deltaTime   -> converts it into movement this frame.
        // Multiplying them together makes the movement frame-rate independent.
        // Finally we convert the Vector2 into a Vector3 because transform.position is a Vector3.
        transform.position += (Vector3)(fallingdirection * speed * Time.deltaTime);
        
        // Destroy if it falls way below the map
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If this meteor has already exploded, ignore every future collision.
        // This helsp to prevent multiple explosions in the same frame.
        if (hasExploded)
        {
            return;
        }
        
        bool hitEnemy = IsOnLayer(collision.gameObject, enemyLayer);
        bool hitGround = IsOnLayer(collision.gameObject, impactLayer);

        // Note that while the meteor may still explode beside the enemy base,
        // the explosion will not damage the base because the base has no UnitMove component.
        // i.e the meteor should only affect enemy units.

        if (hitEnemy || hitGround)
        {
            Explode();
        }
    }

    // To check if the object we are looking at is on that specific layerMask we are looking for (enemy/ground)
    private bool IsOnLayer(GameObject target, LayerMask layerMask)
    {
        return (layerMask.value & (1 << target.layer)) != 0;
    }

    private void Explode()
    {
        // To prevent the same meteor from exploding twice.
        if (hasExploded)
        {
            return;
        }

        // Set the explosion track to be true
        hasExploded = true;
        
        // Find all colliders within the explosion radius on the enemy layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        // As one enemy may contain several colliders, the big boss, without setting up a HashSet, to uniquely
        // identify duplicate colliders of the same object without wasting time, one explosion could damage the 
        // same enemy multiple times.
        // As such, by using a hashset  here, the HashSet will automatically ignores duplicates.
        HashSet<HealthSystem> damagedUnits = new HashSet<HealthSystem>();

        // Deal damage to everything caught in the blast
        foreach (Collider2D hit in hits)
        {
            HealthSystem enemyHealth = hit.GetComponentInParent<HealthSystem>();
            // This is important as I want the special ability to only damage troops, units, not 
            // enemy base, where enemy base does not have this script.
            UnitMove enemyUnit = hit.GetComponentInParent<UnitMove>();

            // As such, we will ignore anything that is
            // - not damageable
            // OR
            // - not an actual unit.
            if (enemyHealth == null || enemyUnit == null)
            {
                continue;
            }

            // If not, by utilising the HashSet.Add(...)
            // - returns true means that this is the first time seeing this unit.
            // - returns false means that the unit has already been damaged earlier.
            //
            // Therefore each enemy can only be damaged once per explosion.
            if (damagedUnits.Add(enemyHealth))
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