using UnityEngine;
using UnityEngine.UIElements;

public class UnitMove : MonoBehaviour
{
    [Header("Character Stats")]
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
    // This is for tracking the range of the ally unit in front of us, so that we can move at the same speed as them, to prevent animation issues.
    public float speedTrackingRange = 2f;

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
    // For tracking of the unit current movement speed, so that we can use it for referecing to ensure all units if in a row moves at the same speed
    private float currentMoveSpeed = 0f;

    // For memory cashing of the ally unit in front of us, so that we can move at the same speed as them, to prevent animation issues.
    private UnitMove trackedFrontAlly = null;

    // This is for allowing the script to be used for both player and enemy units, since
    // I will have this field to differentiate the target of this current unit, based on the
    // tag that has been placed on them.
    // i.e. when the unit is a player unit, the target tag will be "Enemy", 
    // which means it will be looking for any game object with the tag "Enemy" to attack, 
    // and when the unit is an enemy unit, the target tag will be "Player", 
    // which means it will be looking for any game object with the tag "Player" to attack.
    private string targetTag;

    // I will also have another field to store the direction of the target, as for player units,
    // they should be moving towards the right, which is the positive x direction, and for enemy units, 
    // they should be moving towards the left, which is the negative x direction, so we can use this to 
    // determine the direction of the unit movement.
    private int moveDirection;

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

    // Before we even start up the game, I should define the tag of the target that this unit will be attacking, 
    // which is based on the tag of the unit itself.
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

        // Set the target tag based on the tag of the unit itself, if the unit is a player unit, 
        // then the target tag will be "Enemy", and if the unit is an enemy unit, then the target tag 
        // will be "Player".
        if (gameObject.CompareTag("Player"))
        {
            // Our target is enemy units, with a positive move direction since we want to move towards the right
            targetTag = "Enemy";
            moveDirection = 1;
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Our target is player units, with a negative move direction since we want to move towards the left
            targetTag = "Player";
            moveDirection = -1;
        }
    }

    // Now we set up a target for the unit to hit, which would be either the enemy base or the enemy unit, 
    // which will be used to determine if the unit has reached its destination or not.
    // public Transform target;

    private void Update()
    {
        // Since I want to showcase a unit being deployed as it is dropped from the sky, 
        // I will make the unit fall down first before it starts moving forward.
        if (!hasDeployed)
        {
            hasDeployed = IsGrounded();
            // Update the currentMoveSpeed to 0f since the unit is not moving yet, as it is still falling down from the sky.
            currentMoveSpeed = 0f;
            // the return is here to pre-stop the update loop till we know the troop
            // has been successfully deployed, only which the rest of the code will be executed.
            return;
        }

        // If the unit has been deployed, it will start moving forward automatically.
        // It will only stop when it reaches the target, which is either the enemy base or the enemy unit.   
        if (isAttacking)
        {
            // Update the currentMoveSpeed to 0f since the unit is not moving forward anymore, as it is currently attacking the target.
            currentMoveSpeed = 0f;
            
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

                    // Fire the emergency stop trigger
                    animator.SetTrigger("CancelAttack");
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

                // Start the attack cooldown
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

            // Update the currentMoveSpeed to 0f since the unit is not moving forward anymore, as it is currently attacking the target.
            currentMoveSpeed = 0f;

            // To store the enemy target we found, so that the update loop still has memory of the target we are attacking, 
            // since the FindEnemyInRange method is only used to find if there is any enemy unit or base within the attack range
            targetHealth = enemyInRange;

            // Just for logging purposes to see if the unit has found an enemy in range.
            Debug.Log("Enemy found in range!");

            // Same thing, since we know we found an enemy in range, we should start attacking it immediately, 
            // so we should return and not move forward anymore.
            return;
        }

        // Now we change the ally collision detection to be more dynamic, so we need to get the proposed speed, we can use the GetAdjustedSpeed method to get the 
        // speed of the ally unit in front of us, if there is one, and then we can use that speed to move forward, so that we can move at the same speed as the ally unit 
        // in front of us, to prevent animation issues.
        
        // First we try an obtain the ally unit in front of us, if there is one, we will use its speed to move forward, otherwise we will use our own speed to move forward.
        UnitMove blockingAlly = GetAllyInFront();

        // If there is an ally unit infront of us
        if (blockingAlly != null)
        {
            // Update the private field to remember the ally unit in front of us, so that we can move at the same speed as them, to prevent animation issues.
            trackedFrontAlly = blockingAlly;
            // Since there is an ally unit in front of us, we should stop, thus set currentMoveSpeed to 0f, and not move forward anymore,
            // until the ally unit in front of us is destroyed or moves forward.
            currentMoveSpeed = 0f;

            // This is to change the current unit animation to idle, since we are not moving forward anymore.
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            // We do not want the remainder code below to execute since we are not moving forward anymore.
            return;
        }

        // If we cannot see the unit in front of us anymore, firstly let see if there was originally an ally unit in front of us,
        // such that if there was one, we will check if it is still alive, if it is not, we will set the trackedFrontAlly to null, and move forward again.
        // This is so that we move at the speed that is at most ther max speed, or lesser to ensure there is no animation issues
        // In essence this if statement is to track that both the memory unit and the current ally unit we see is still in front of us,
        // in which we change the movement speed accordingly
        if (trackedFrontAlly != null && GetTrackedAllyInFront() != null)
        {
            currentMoveSpeed = Mathf.Min(speed, trackedFrontAlly.GetCurrentMoveSpeed());
        }
        // Also need to have the condition if there is no trackFrontAlly being seen, the currentMoveSpped has to be the original unit speed value
        // And we forget the original tracked unit
        else
        {
            trackedFrontAlly = null;
            currentMoveSpeed = speed;
        }

        // Coast is clear, switch to Walk, also to prevent inconsistent animation issues, i.e. to say when we only start the movement animation
        // if the current unit move speed is greater than 0.01f, which is a small value to prevent floating point errors, and not when it is 0f, which is the default value.
        if (animator != null)
        {
            animator.SetBool("IsMoving", currentMoveSpeed > 0.01f);
        }

        // If the unit has been deployed, it will start moving forward automatically.
        // It will only stop when it reaches the target, which is either the enemy base or the enemy unit.
        // In this case it would be when any of the methods above has been triggered. 
        // Making sure that the update of the unit is relative with the user
        // laptop/PC ram using delta time
        // Now I need to add the move direction to the movement, since for player units, 
        // they should be moving towards the right, which is the positive x direction, 
        // and for enemy units, they should be moving towards the left, which is the negative x direction.
        transform.position += Vector3.right * moveDirection * currentMoveSpeed * Time.deltaTime;
    }

    // Getter method for currentMoveSpeed value as we dont want other classes to access the value directly
    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
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
        // picture it like
        /*
                      |       |
            player -> |   x   |   where x is the box centre
                      |       |
        */
        Vector2 boxCenter = (Vector2)unitColliderBound.bounds.center 
                            + Vector2.right * moveDirection * (frontOffSet + attackRange / 2f);

        
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
        // picture it like
        /*
                      |       |
            player -> |   x   |   where x is the box centre
                      |       |
        */
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

    /*
    * Now we need to have a tracker as well to know if the unit in front of us is an
    * ally, as if it is, then we go up as close as it is to them, then we stop and wait for them to be 
    * destroyed or move forward, to move.
    *
    * Since Age of War is a one line attck game, if we collide with our allies, we need to
    * stop behind them and wait for them to be destroyed or move forward, to move.
    * Also later one we need to add it such that the unit is able to attack the enemy if it is a range unit, 
    * so it attacks the enemy unit or base when it is within the attack range.
    */
    // NOTE: DID Changes from isAllyInFront to GetAllyInFront, since I want to know if there is a unit infront of us
    // what is their speed, so we move according to the slower unit in front of us, thus we need to return the UnitMove script of the ally in front of us
    // Of cuz, if there is no unit in front of us, we return null, and we can move at our own speed.
    private UnitMove GetAllyInFront()
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
        
        /* 
        * We can use a raycast to check if there is an ally unit in front of us within a certain distance, 
        * which is the ally collision range in this case, since if there is an ally unit in front of us within 
        * the ally collision range, it means we should stop and wait for them to be destroyed or move forward, 
        * to move.
        * 
        * The 1 << allyLayer is to only check for colliders in the ally layer, which means we will only detect
        * ally units and ignore enemy units and bases, since they are on a different layer.
        * documentation for layer mask: https://docs.unity3d.com/ScriptReference/LayerMask.html
        *
        * Looking at the documention, since we only care able unit directly in front of us, we can use raycast, 
        * which is like a laser beam that shoots out from the unit in the direction of movement,
        * and it will only detect the first collider it hits, which is more efficient than using overlap circle,
        * which checks for all colliders within a certain radius, and then we check if any of them is an ally unit.
        * https://docs.unity3d.com/ScriptReference/Physics2D.Raycast.html
        */
        // Now I will see from the ray casting, what are all possible objects/items being detected, placing
        // them all into a list first
        RaycastHit2D[] allyHit = Physics2D.RaycastAll(rayStart, direction, allyCollisionRange, 
                                                      1 << allyLayer);

        // Now I need to track the closest ally unit in front of us, so that we can move at the same speed as them, to prevent animation issues.
        // Since raycast only detects allys in a range, where it might not be the shortest distance unit correctly, there could be buggy animations
        // since we are not detecting the unit that is truly in front of us, thus we need to track the closest ally unit in front of us, 
        // so that we can move at the same speed as them, to prevent animation issues.
        UnitMove closestAlly = null;
        // Set the max distance first, cuz I know the map size makes it impossible for any unit to be further than infinity distance.
        float closestDistance = Mathf.Infinity;

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

            // Else if the spawn order is less that our current unit, we update the distance if needed.
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestAlly = allyUnit;
            }

            // Comment this out first cuz it is overloading the logs
            // Debug.Log("Ally in front: " + hit.collider.name);
            // Or else since the item in front of us is our ally and was spawn earlier, we return true.
            // as there is an ally in front of us.
            // We need to access the allyUnit speed so we can change the current unit speed to match the ally unit in front of us, so we can move at the same speed as them.
            // return allyUnit;
        }

        // return the closest ally unit in front of us, if there is one, otherwise return null.
        return closestAlly;
    }

    private UnitMove GetTrackedAllyInFront()
    {
        // I only wnat the speed of the current unit to change onliy if it has noted/tracked there is an ally in front, which only
        // happens when it has to stop the first time.
        if (trackedFrontAlly == null) 
        {
            return null;
        }
        
        // Same configurations as the GetAllyInFront method, but this time we are only checking if the trackedFrontAlly is still in front of us, 
        // and if it is, we return it, otherwise we return null.
        Vector2 direction = Vector2.right * moveDirection;
        float frontOffSet = unitColliderBound.bounds.extents.x;
        Vector2 rayStart = (Vector2)transform.position + direction * frontOffSet;

        // Instead of using the allyCollisionRange, we will use a larger range to check if the trackedFrontAlly is still in front of us,
        // thay way we can ensure that we are not too close to the trackedFrontAlly, and we can move at the same speed as them, to prevent animation issues.
        Debug.DrawRay(rayStart, direction * speedTrackingRange, Color.yellow);

        RaycastHit2D[] allyHit = Physics2D.RaycastAll(rayStart, direction, speedTrackingRange,
                                                      1 << allyLayer);


        // Same raycast scanning idea as above.
        foreach (RaycastHit2D hit in allyHit)
        {
            if (hit.collider == null) continue;
            if (hit.collider.transform.root == transform.root) continue;

            UnitMove allyUnit = hit.collider.GetComponentInParent<UnitMove>();

            if (allyUnit == null) continue;

            // Only reutrns the unit if the unit we remember is still in front of us, otherwise we return null.
            if (allyUnit == trackedFrontAlly)
            {
                return allyUnit;
            }
        }

         // Else return null, meaning no ally unit in front of us within the ally detection range.
        return null;
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

            // Using the HealthSystem component of the target to call the TakeDamage method, 
            // which will reduce the hp of the target by the unit damage amount.
            targetHealth.TakeDamage(unitDamage);

            // For logging purposes, to see the attack has been done/executed in the console.
            // We should see unit attacked enemy base or unit, and then we should see the damage taken and 
            // the hp left of the target in the console.
            Debug.Log(gameObject.name + " melee attacked " + targetHealth.gameObject.name);
        }
    }

    // Need to obtain the spawn order so that the turrent target knows which unit it should target first
    public long GetSpawnOrder()
    {
        return spawnOrder;
    }
}