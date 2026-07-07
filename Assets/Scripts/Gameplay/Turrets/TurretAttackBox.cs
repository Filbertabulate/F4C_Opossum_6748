using UnityEngine;

// This script controls the turret's AI.
// It continuously searches for enemies within its attack box,
// selects the front-most enemy (lowest spawn order),
// plays the attack animation,
// and spawns a projectile when the animation reaches its firing frame.
public class TurretAttackBox : MonoBehaviour
{
    // For the BoxCast Detection Settings
    [Header("Box Range")]
    // Horizontal distance the turret can detect enemies.
    public float attackRange = 8f;

    // Turrent combat settings
    [Header("Attack Settings")]
    // Time between consecutive attacks.
    public float attackCooldown = 1.5f;
    // Damage dealt by each projectile.
    public int damage = 10;

    // References for to help control how the turret is displayed
    [Header("References")]
    // Obtaining the Animator controlling the idle and attack animations of the turret
    public Animator animator;
    // Obtaining the Location where projectiles are spawned.
    public Transform firePoint;
    // Obtaining the Projectile (arrow) fired by the turret.
    public GameObject projectilePrefab;
    
    // Private Variables

    // The tag of units the turret can attack.
    // Will be dynamically assigned based on current turrent tag
    private string targetTag;
    // Physics layer containing the units the current turret can attack.
    // I will make it dynamically assigned based on the current turrent layer and tag
    private LayerMask targetLayer;
    // Direction the turret fires, which will be dynamically assigned based on the current tage
    // Note that for Player turret, the direction will be 1, while enemy turret the direction will be
    // -1
    private int direction;

    // Attack cooldown timer
    private float attackTimer = 0f;

    // Stores the enemy that we have locked on to attack
    private Transform currentTarget;

    // Reference to the GroundGrid (or an empty object placed at the top of the ground)
    private Transform groundReference;

    // On start of the creation of the turret, make sure that we fill up the correct tagging
    // where if our turret is player, our target is enemy and projection movement direction is 1
    // likewise the opposite is true as well for the enemy tag
    private void Start()
    {
        Debug.Log($"{gameObject.name} projectilePrefab = {projectilePrefab}");
        
        // To Find the group top value to start
        GameObject ground = GameObject.Find("GroundTop");

        if (ground != null)
        {
            groundReference = ground.transform;
        }
        else
        {
            Debug.LogError("GroundTop not found!");
        }
        
        // Player turret attacks Enemy units and shoots right.
        if (CompareTag("Player"))
        {
            targetTag = "Enemy";
            targetLayer = LayerMask.GetMask("EnemyUnits");
            direction = 1;
        }
        // Enemy turret attacks Player units and shoots left.
        else
        {
            targetTag = "Player";
            targetLayer = LayerMask.GetMask("PlayerUnits");
            direction = -1;
        }
    }

    private void Update()
    {
        // Reduce attack cooldown timer every frame (based on real time via the system)
        attackTimer -= Time.deltaTime;

        // Search for the front-most enemy currently inside the attack box.
        UnitMove target = FindOldestEnemyInBox();

        // If no enemy is found, clear the current target and stop attacking.
        if (target == null)
        {
            currentTarget = null;
            return;
        }

        // Else, if we find an enemy, lock on to that target till it is destoryed, 
        // making sure that the currentTarget remembers the correct target we should be hitting.
        currentTarget = target.transform;

        // If the turret has finished reloading (i.e. the cooldown is over), trigger the attack animation.
        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Fire");

            // Reset attack cooldown.
            attackTimer = attackCooldown;
        }
    }

    // Searches every enemy inside the turret's attack box, and returns the one with the lowest spawnOrder.
    private UnitMove FindOldestEnemyInBox()
    {
        // Detect every collider inside the attack box.
        // Similar principle for the ally detection method
        // GetBoxCenter() -> finding out the true center of the box range we are lookng at
        // GetBoxSize() -> Give the true dimension of the box size for length and height
        // 0f -> Enusre that the box is not rotated, stay in the correct position
        // targetLayer -> It a filter so we only pick up units from that layer only as "hittable" targets
        Collider2D[] hits = Physics2D.OverlapBoxAll(GetBoxCenter(), GetBoxSize(), 0f, targetLayer);

        // Assume no enemy has been found yet.
        UnitMove oldestEnemy = null;

        // Initialise the value first with the largest possible value.
        // if there is a unit will confirm override this value
        long lowestSpawnOrder = long.MaxValue;

        // Loop through every detected collider.
        foreach (Collider2D hit in hits)
        {
            // Ignore anything that isn't an enemy.
            // Since enemy layer can have like other things, not just enemy units, so a failsafe
            if (!hit.CompareTag(targetTag))
            {
                continue;
            }

            // Obtain the UnitMove component of the current unit we are checking, which is a valid
            // target
            UnitMove unit = hit.GetComponentInParent<UnitMove>();

            // If the unit found somehow does not have a unitMove, script, we ignore it since we need
            // it for getting the earliest spawn unit tracking.
            // Also since each unit has to have this unitmove script, it is impossible for a valid unit
            // to hit this portion
            if (unit == null)
            {
                continue;
            }

            // Keep track of the enemy that has the lowest spawnOrder, which should be the front-most enemy.
            if (unit.GetSpawnOrder() < lowestSpawnOrder)
            {
                lowestSpawnOrder = unit.GetSpawnOrder();
                oldestEnemy = unit;
            }
        }

        // keep track of the oldest enemy and reutrn that unit
        return oldestEnemy;
    }

     // Box extends forward from turret and vertically from turret height to ground.
    private Vector2 GetBoxCenter()
    {
        // Obtain the x coordiate of the box range centre
        // i.e. suppose:
        // Turret x = 10
        // Attack Range  = 8
        // this means the box width should be 8,
        // so the position of the center should be 10 + (8/2) = 14
        // We add direction cuz the box might need to be inverted backwards if it is for enemy
        // Likewise for the ground
        // Turret Y = 6
        // Ground Y = 0
        // Middle is just (6 + 0) / 2 = 3

        // This if method is just for the Gizmos tracking, where the turrent is not created yet, but we
        // want to see the range in the scene
        if (direction == 0)
        {
            // Fallback if Start() hasn't initialized it yet.
            direction = GetDirection();
        }

        float centerX;
        // The condition is differnt based on the direction it is facing
        if (direction == 1)
        {
            // Player turret
            centerX = transform.position.x + attackRange / 2f;
        }
        else
        {
            // Enemy turret
            centerX = transform.position.x - attackRange / 2f;
        }

        // Use GroundTop if it exists, otherwise just use the turret's own Y.
        // Should be tempoarary until we have a better way to find the ground position.
        float groundY = transform.position.y;

        // To prevent the groundReference from being null, we will try to find it if it is null, 
        // and assign it to the groundReference variable
        if (groundReference != null)
        {
            groundY = groundReference.position.y;
        }
        else
        {
            GameObject ground = GameObject.Find("GroundTop");

            if (ground != null)
            {
                groundReference = ground.transform;
                groundY = groundReference.position.y;
            }
        }

        float centerY = (transform.position.y + groundY) / 2f;

        // Update the x and y coords of the box ceneter
        return new Vector2(centerX, centerY);
    }

    private Vector2 GetBoxSize()
    {
        // Use GroundTop if it exists, otherwise just use the turret's own Y.
        // Should be tempoarary until we have a better way to find the ground position.
        float groundY = transform.position.y;

        // To prevent the groundReference from being null, we will try to find it if it is null, 
        // and assign it to the groundReference variable
        if (groundReference != null)
        {
            groundY = groundReference.position.y;
        }
        else
        {
            GameObject ground = GameObject.Find("GroundTop");

            if (ground != null)
            {
                groundReference = ground.transform;
                groundY = groundReference.position.y;
            }
        }
        
        // How baig the rectange, the width is just the attack range, while the height
        // has to be from the ground position up to the turret position.
        // Mathf.Abs is to ensure we account in case we have negative height, which is not physically
        // possible
        float height = Mathf.Abs(transform.position.y - groundY);

        // return the x and y coords of the box size
        return new Vector2(attackRange, height);
    }

    // Called by the Animation Event when the bow releases.
    public void FireProjectile()
    {
        // For Debugging purposes, since the arrow is spawning three times for each fire when i set it to one only
        Debug.Log($"FireProjectile START {Time.frameCount}");

        // Stop if target no longer exists.
        if (currentTarget == null)
        {
            Debug.LogWarning("No current target");
            return;
        }

        // Stop if there is no project object we can fire
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab missing");
            return;
        }

        // Stop if there is no spawn point for the projectile to come out from
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint missing");
            return;
        }
        
        // Spawn the projectile at the FirePoint.
        // initialise the project that we want to shoot out, at the defined firepoint position, and
        // Quaternion.identity is to ensure the arrow is spawn in with zero rotation at the start
        GameObject arrow = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // For Debug Purposes
        /*
        arrow.name = "Spawned_Arrow";
        Debug.Log("Spawned ONE arrow: " + arrow.GetInstanceID());
        Debug.Log($"Instantiate finished {arrow.GetInstanceID()}");
        */

        // Obtain the projectile script.
        TurretArcProjectile projectile = arrow.GetComponent<TurretArcProjectile>();

        // Initialise the projectile, this creates the arrow movement animation
        if (projectile != null)
        {
            projectile.Initialize(currentTarget, damage);
        }
    }

    // Draws the attack box inside the Scene View.
    // This helps visualise and debug the turret range.
    private void OnDrawGizmosSelected()
    {
        // the box colour should be yellow in colour
        Gizmos.color = Color.yellow;

        // To draw out the turret range box
        Gizmos.DrawWireCube(GetBoxCenter(), GetBoxSize());
    }

    // For Gizmos tracking
    private int GetDirection()
    {
        if (CompareTag("Player"))
        {
            return 1;
        }

        return -1;
    }
}