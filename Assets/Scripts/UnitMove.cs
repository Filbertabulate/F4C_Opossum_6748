using UnityEngine;
using UnityEngine.UIElements;

public class UnitMove : MonoBehaviour
{
    // Set up the speed of the unit/ball to be moving automatically, value obtain via trial and error.
    public float speed = 1f;

    // Set the damage the unit can deal to an enemy base or enemy unit
    public int unitDamage = 10;

    // Set up the attack cooldown for the unit, which is the time between each attack,
    // which in this case is 1 second
    public float attackCooldown = 1f;

    // Set the unit attack range, in this case 1f is for melee unit, which means it can only attack when 
    // it is colliding with the enemy unit or base.
    public float attackRange = 1f;

    // Setting up the value for ally collision detection, so we stop behind our allies and wait for them to be 
    // destroyed or move forward, to move.
    // Value obtain via trial and error
    public float allyCollisionRange = 0.5f;

    // Set up the ground layer, which will be used to check if the unit has landed on the ground or not,
    // for deployment purposes.
    public LayerMask groundLayer;

    // In case there are scenarios where there is unit stacking, which is allowed only at the base
    // I need to have a tagging called spawn Order so that I know which unit should move first
    // [SerializeField] attribute in Unity forces the engine to serialize a private or protected field, 
    // making it visible and editable within the Unity Inspector while keeping it inaccessible to other scripts.
    // For tracking purposes
    [SerializeField] private long spawnOrder;

    // Set the unit initial state to be not deployed, where it will only change
    // once we know that the unit has landed on the ground.
    private bool hasDeployed = false;
    // Tracker to know if the unit is currently attacking or not
    private bool isAttacking = false;

    [Header("Targeting & Movement Settings")]
    [Tooltip("The tag this unit will attack. (e.g., 'Enemy' for player units, 'Player' for enemy units)")]
    public string targetTag;

    [Tooltip("The direction this unit moves. (1 for right/Player, -1 for left/Enemy)")]
    public int moveDirection = 1;

    // Now I need to get the layer number of the current unit layer its on, that way
    // all ally units in that layers will be detected as allies, and we can stop behind them and wait for 
    // them to be destroyed or move forward, to move.
    private int allyLayer;

    // This unitCollider bound is defined here because I want to know the unit "edge" position
    // so instead of random guessing the start position for the RayCast start position, I can
    // just take refence from the edge of the unitColliderBound value
    private Collider2D unitColliderBound;

    // Since we have an attack cooldown, we need a timer to track the time between each attack,
    // so we know when the unit can attack again after the cooldown time has passed, where the attackTimer
    // hits <= 0
    private float attackTimer = 0f;

    // 
    private HealthSystem targetHealth;
    
    //private Rigidbody2D rb;

    [Header("References")]
    public Transform groundCheck;

    [Header("Ranged Combat Settings")]
    public bool isRanged = false; 
    public GameObject projectilePrefab; 
    public Transform firePoint; // The spot where the projectile spawns 

    private Animator animator;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();

        // Get the layer number of the current unit layer its on, that way all ally units in that layers 
        // will be detected as allies,
        allyLayer = gameObject.layer;

        // This is to get the unit boundary for the RayCast later on.
        // Why i use Collider2D instead of specific values is becuase I want to be inclusive
        // for other shape, unit types that may have different collider boundaries.
        unitColliderBound = GetComponent<Collider2D>();

        // Grab the Animator component attached to this unit so we can trigger animations
        animator = GetComponent<Animator>();

        // Hardcoded tag assignments removed. targetTag and moveDirection are now directly
        // controlled via the Unity Inspector to allow both units to use this script dynamically.
    }

    private void Update()
    {
        // Since I want to showcase a unit being deployed as it is dropped from the sky, 
        // I will make the unit fall down first before it starts moving forward.
        if (!hasDeployed)
        {
            hasDeployed = IsGrounded();
            // the return is here to pre-stop the update loop till we know the troop
            // has been successfully deployed, only which the rest of the code will be executed.
            return;
        }

        // If the unit has been deployed, it will start moving forward automatically.
        // It will only stop when it reaches the target, which is either the enemy base or the enemy unit.   
        if (isAttacking)
        {
            // Animation Logic: Stop walking, wait for attack trigger
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            // If target has been killed or destroyed, we should stop attacking and start moving forward again.
            if (targetHealth == null)
            {
                isAttacking = false;
                
                // Wipe the memory of the attack trigger so they don't swing at ghosts
                if (animator != null)
                {
                    animator.ResetTrigger("AttackTrigger");
                }
                
                return;
            }

            // Else we first check are we still attacking a base, cuz if we are, and we found that
            // there is a new unit enemy that has been spawned by the enemy, we need to lock onto that
            // unit instead.
            if (IsCurrentTargetBase())
            {
                HealthSystem oldestEnemyUnit = FindOldestEnemyUnitInRange();

                if (oldestEnemyUnit != null)
                {
                    targetHealth = oldestEnemyUnit;

                    // For debugging and logging purposes.
                    Debug.Log(gameObject.name + " switched from base to enemy troop!");
                }
            }

            // If the unit is currently attacking, 
            // we should reduce the attack timer by the time that has passed since the last frame.
            // Since we initiall start with 0f, it means the unit is ready to attack as soon as it collides 
            // with the enemy unit or base, and then we reset the attack timer to the attack cooldown, 
            // which means the unit will attack again after the cooldown time has passed.
            attackTimer -= Time.deltaTime;

            // To ensure we trigger the attack only when the attack has finished its cooldown.
            if (attackTimer <= 0f)
            {
                // Trigger animation if it has one, otherwise attack instantly (for basic circles) 
                if (animator != null)
                {
                    animator.SetTrigger("AttackTrigger");
                }
                else
                {
                    ExecuteAttack();
                }

                // Start the attack cooldown...
                attackTimer = attackCooldown;
            }

            // To ensure we dont run other codes below since when we are attacking, we should not be moving forward, 
            // we should only be attacking the target until it is destroyed or killed.
            return;
        }

        // Else if the unit is not attacking currently, we should be finding if there is any enemy unit or enemy base
        //  within the attack range of the unit.
        HealthSystem enemyInRange = FindEnemyInRange();

        // If enemy found, start attacking
        if (enemyInRange != null)
        {
            isAttacking = true;
            // To store the enemy target we found, so that the update loop still has memory of the target we are attacking, 
            // since the FindEnemyInRange method is only used to find if there is any enemy unit or base within the attack range
            targetHealth = enemyInRange;

            // Just for logging purposes to see if the unit has found an enemy in range.
            Debug.Log("Enemy found in range!");

            // Same thing, since we know we found an enemy in range, we should start attacking it immediately, 
            // so we should return and not move forward anymore.
            return;
        }

        // Now if there is an ally unit in front of us within the ally collision range, 
        // we should stop and wait for them to be destroyed or move forward, to move.
        if (isAllyInFront())
        {
            // Just for logging purposes to see if there is an ally unit in front of us within the ally 
            // collision range.
            // This debug current overload the console terminal, thus commented out for now
            // Debug.Log("Ally unit in front! Stopping to wait.");

            // Blocked by ally, switch to Idle 
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            // We should return and not move forward anymore.
            return;
        }

        // Coast is clear, switch to Walk
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }

        // If the unit has been deployed, it will start moving forward automatically.
        // It will only stop when it reaches the target, which is either the enemy base or the enemy unit.
        // In this case it would be when any of the methods above has been triggered. 
        // Making sure that the update of the unit is relative with the user
        // laptop/PC ram using delta time
        // Now I need to add the move direction to the movement, since for player units, 
        // they should be moving towards the right, which is the positive x direction, 
        // and for enemy units, they should be moving towards the left, which is the negative x direction.
        transform.position += Vector3.right * moveDirection * speed * Time.deltaTime;
    }
    
    // Method to check if the unit is grounded, which will be used to determine if the unit can move forward
    // or not
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    // Instead I will create a method called FindEnemyInRange, which checks if there is any
    // enemy unit or enemy base within the attack range of the unit. If there is, it will return the
    // HealthSystem component of the enemy unit or base, which will be used to deal damage to it. 
    private HealthSystem FindEnemyInRange()
    {
        // Since now we included the usage of SpawnOrder, we want to get the HP of the unit that
        // is Spawned first
        HealthSystem oldestEnemyUnit = FindOldestEnemyUnitInRange();

        // If we are able to find an Enemy Unit, we return that enemy Unity Health to be
        // the target we want to reduce the value on.
        if(oldestEnemyUnit != null)
        {
            return oldestEnemyUnit;
        }

        // If there is no Enemy Unit found, then we need to see if there is a base that
        // is in range that we can attack, cuz if there is, we attack that base instead.
        if (!AnyEnemyUnitsAlive())
        {
            return FindEnemyBaseInRange();
        }

        // If there is no enemy unit or base within the attack range, we return null.
        return null;
    }

    // To ensure the unit we are targeting is alawys the unit that has been spawned first, I
    // I create a method to Find the oldest enemy unit in range
    private HealthSystem FindOldestEnemyUnitInRange()
    {
        
        // I have used the unitColliderBound to find the distnace from the unit center
        // to its front edge, in this case, I only care able its x axis, i.e. its left to right value
        // where this frontOffSet value is equal to half the width of the collider bound.
        float frontOffSet = unitColliderBound.bounds.extents.x;

        // To get the box collider range true center, we need to take the 
        // 1) the collider's mathematical center, and then add it with 
        // 2) The direction we should be pointing towrds, if it is a player, point right
        // else if it is not point left, thus the usage of Vector2.right
        // 3) We multiple the direction we are facing with the distance of the edge of the unit 
        // with the attack range to get the max reach of the unit.
        // 4) However, since vectors in Unity assumes we are talking about the centre of the box we
        // are trying to create, I need to shift the x value by half to hit the true centre of the attack range
        Vector2 boxCenter = (Vector2)unitColliderBound.bounds.center 
                            + Vector2.right * moveDirection * (frontOffSet + attackRange) / 2f;

        
        // Now we define the dimensions of the detection box, i.e. our target area.
        // The width is exactly the assigned attackRange value.
        // The height matches the unit's collider height.
        Vector2 boxSize = new Vector2(attackRange, unitColliderBound.bounds.size.y);

        // Now we push the attack box created into the physics engine and collect all overlapping 2D colliders.
        // It uses our calculated forward center point, the dynamic size vectors, and a 0-degree rotation angle.
        // We keep it at 0 cuz we are doing a 2D game, so the "Z" value is not as importnant.
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        // Temporay place holders so that we can compare for all enemy units observed, which units
        // is the one that was spawned first, and from there, what is its health points.
        UnitMove oldestEnemyUnit = null;
        HealthSystem oldestEnemyHealth = null;

        // Going through every single collider detected inside the overlapping attack zone area.
        foreach (Collider2D hit in hits)
        {
            // If the collider value inside is empty, skip it
            if (hit == null) continue;

            // Skip this object if its tag does not match our target.
            if (!hit.CompareTag(targetTag)) continue;

            // Else if we found our traget type, we seach its value
            // to get its Unit Move script to get the spawn order later on.
            UnitMove enemyUnit = hit.GetComponentInParent<UnitMove>();
            
            // We also want to get the health script from the enemy to get its enemy health value so we 
            // can do damge to our target
            HealthSystem enemyHealth = hit.GetComponentInParent<HealthSystem>();

            // Just a fail safe as if this object if it lacks either of the core required component scripts,
            // we skip it since the code will break later onewards.
            if (enemyUnit == null || enemyHealth == null) continue;

            // If haven't selected a target yet, or if this newly found enemy unit was spawned earlier 
            // in the game than our current selection, then we take that as our unit to target.
            if (oldestEnemyUnit == null || enemyUnit.spawnOrder < oldestEnemyUnit.spawnOrder)
            {
                // Save this enemy setup as our new current best target choice.
                oldestEnemyUnit = enemyUnit;
                oldestEnemyHealth = enemyHealth;
            }
        }

        // Provide the health system component of the optimal target found (returns null if empty).
        return oldestEnemyHealth;
    }

    // This time is the same idea as above, but our target this time is the enemy base, and since
    // this code only run when we know all the enemy above is dead, we can just find the target with 
    // the enemy tag and it will be the enemy base
    private HealthSystem FindEnemyBaseInRange()
    {
        // I have used the unitColliderBound to find the distnace from the unit center
        // to its front edge, in this case, I only care able its x axis, i.e. its left to right value
        // where this frontOffSet value is equal to half the width of the collider bound.
        float frontOffSet = unitColliderBound.bounds.extents.x;

        // To get the box collider range true center, we need to take the 
        // 1) the collider's mathematical center, and then add it with 
        // 2) The direction we should be pointing towrds, if it is a player, point right
        // else if it is not point left, thus the usage of Vector2.right
        // 3) We multiple the direction we are facing with the distance of the edge of the unit 
        // with the attack range to get the max reach of the unit.
        // 4) However, since vectors in Unity assumes we are talking about the centre of the box we
        // are trying to create, I need to shift the x value by half to hit the true centre of the attack range
        Vector2 boxCenter = (Vector2)unitColliderBound.bounds.center 
                            + Vector2.right * moveDirection * (frontOffSet + attackRange) / 2f;

        
        // Now we define the dimensions of the detection box, i.e. our target area.
        // The width is exactly the assigned attackRange value.
        // The height matches the unit's collider height.
        Vector2 boxSize = new Vector2(attackRange, unitColliderBound.bounds.size.y);

        // Now we push the attack box created into the physics engine and collect all overlapping 2D colliders.
        // It uses our calculated forward center point, the dynamic size vectors, and a 0-degree rotation angle.
        // We keep it at 0 cuz we are doing a 2D game, so the "Z" value is not as importnant.
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        // Going through every single collider detected inside the overlapping attack zone area.
        foreach (Collider2D hit in hits)
        {
            // If the collider value inside is empty, skip it
            if (hit == null) continue;
            
            // Skip this object if its tag does not match our target.
            if (!hit.CompareTag(targetTag)) continue;

            // Else if we found our traget type, we seach its value
            // to get its Unit Move script to get the spawn order later on.
            UnitMove enemyUnit = hit.GetComponentInParent<UnitMove>();
            
            // We also want to get the health script from the enemy to get its enemy health value so we 
            // can do damge to our target
            HealthSystem enemyHealth = hit.GetComponentInParent<HealthSystem>();
            
            // If there is No UnitMove, but there is a HealthSystem, we will treat that target as a base.
            if (enemyUnit == null && enemyHealth != null)
            {
                return enemyHealth;
            }
        }

        // Else if there is no base as well, we reutrn null
        return null;
    }

    // Method to check if any target unit that were deployable is still alive.
    private bool AnyEnemyUnitsAlive()
    {
        // From the who map right now, we see all possible units/sprites that are on the map, all
        // which match our target tag
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        // Going through the array of targets, if there is any
        foreach (GameObject target in targets)
        {
            // If any of the targettag units/sprites contains a UnitMove script, it means
            // that the unit is a character, and not a base, so there is still enemy units alive.
            UnitMove unit = target.GetComponentInParent<UnitMove>();

            if (unit != null)
            {
                return true;
            }
        }

        // Else it means there is no target unit/characters alive, and it might only be left with the base.
        return false;
    }

    // A method flag just to check if the current target the unit is lock into is a base unit or not,
    // i.e. it has health values, but no unitMove script.
    private bool IsCurrentTargetBase()
    {
        return targetHealth != null &&  targetHealth.GetComponentInParent<UnitMove>() == null;
    }

    private bool isAllyInFront()
    {
        // This is to check if there is an ally unit in front of us within the ally collision range,
        // where we use vector2.right * moveDirection to check in the direction of the unit movement, 
        // since for player units, they should be moving towards the right, which is the positive x direction, 
        // and for enemy units, they should be moving towards the left, which is the negative x direction.
        Vector2 direction = Vector2.right * moveDirection;

        // I have used the unitColliderBound to find the distnace from the unit center
        // to its front edge, in this case, I only care able its x axis, i.e. its left to right value
        // where this frontOffSet value is equal to half the width of the collider bound.
        float frontOffSet = unitColliderBound.bounds.extents.x;

        // This is to tell where the ray starting position should be, as well as the direction it should
        // be pointing to, where a negative rayStart means that means that the object is moving left and 
        // the ray will cast from its left edge, while a positive offset casts it from the right edge.
        // Why I had cast the transform.position to a vector2 datatype is because since we are working
        // with a 2D game, the z axis is not important, where we mainly care able the x axis for this method
        // thus keeping it as Vector2 will suffice.
        Vector2 rayStart = (Vector2) transform.position + direction * frontOffSet;

        // For debugging purposes, where I want to see where the raw line starts from to ensure
        // we dont point/tigger wrong ally finding causing unwanted scenarios to happen.
        Debug.DrawRay(rayStart, direction * allyCollisionRange, Color.red);
        
        // Now I will see from the ray casting, what are all possible objects/items being detected, placing
        // them all into a list first
        RaycastHit2D[] allyHit = Physics2D.RaycastAll(rayStart, direction, allyCollisionRange, 
                                                      1 << allyLayer);

        // Going through all the objects that was picked up by the raycast
        foreach (RaycastHit2D hit in allyHit)
        {
            // If there is no object found in the hit.collider from the rayCast, we do nothing and
            // go to the next object
            if (hit.collider == null) continue;

            // If what we are hitting is our own unit, or items/parts of the current unit, i.e. the child
            // components, we ignore it as well.
            // transform.root refers to the top-most parent object in the hierarchy, not the main parent
            // but taking this current unit with the script as the top most parent.
            // If both roots are the same, it means the detected collider belongs to this current unit and 
            // should not be treated as an ally in front.
            if (hit.collider.transform.root == transform.root) continue;

            // This is for us to debug at this point after see both if statements, if we still encouter and
            // items that is not part of the current unit, we will flag in on the map during debugging
            // of the current postion where we saw it in blue.
            Debug.DrawRay(hit.point, Vector2.up * 0.5f, Color.blue, 1f);

            // Now I want to get the info of the current unit that is being flag, which has to be
            // an ally unit since the ray cast only picks up units on the same layer, which are all 
            // ally units
            UnitMove allyUnit = hit.collider.GetComponentInParent<UnitMove>();

            // This is just a safety catch in case we found an ally unit, mabye the base, but
            // since it does not have a unit move script attached to it, since it is a base
            // we skip that unit flagged by the ray cast.
            if (allyUnit == null) continue;            

            // In this case, we only want to move forward if our current unit has been 
            // spawn earlier than the unit we are comparing, as if that is the case,
            // we want this unit to move forwrd, so we skip this iteration to go to the next
            // item/unit flagged by the ray.
            if (allyUnit.spawnOrder >= spawnOrder) continue;

            // Comment this out first cuz it is overloading the logs
            // Debug.Log("Ally in front: " + hit.collider.name);
            // Or else since the item in front of us is our ally and was spawn earlier, we return true.
            // as there is an ally in front of us.
            return true;
        }

        // Else return false, meaning no ally unit in front of us within the ally collision range.
        return false;
    }

    // Setter method for spawnOrder value cuz we dont want other classes to directly access the value
    // easily
    public void InitialiseSpawnOrder(long order)
    {
        spawnOrder = order;
    }

    // This method is called by the Animation Event to deal damage exactly when the sword swings
    public void ExecuteAttack()
    {
        // Safety check in case the target died while the sword was swinging
        if (targetHealth == null) return; 

        if (isRanged && projectilePrefab != null && firePoint != null)
        {
            // Ranged Attack: Spawn the projectile
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projScript = proj.GetComponent<Projectile>();
            
            if (projScript != null)
            {
                // Pass the unit's stats to the projectile so it knows who to hit and how hard
                projScript.Initialize(unitDamage, targetTag, moveDirection);
            }
            Debug.Log(gameObject.name + " fired a projectile!");
        }
        else
        {
            // Melee Attack: Deal direct damage
            targetHealth.TakeDamage(unitDamage);
            Debug.Log(gameObject.name + " melee attacked " + targetHealth.gameObject.name);
        }
    }
}