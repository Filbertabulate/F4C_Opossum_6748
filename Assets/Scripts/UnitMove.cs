using UnityEngine;

public class UnitMove : MonoBehaviour
{
    // Set up the speed of the unit/ball to be moving automatically
    public float speed = 2f;
    // Set the damage the unit can deal to an enemy base or enemy unit
    public int unitDamage = 10;
    // Set up the attack cooldown for the unit, which is the time between each attack,
    // which in this case is 1 second
    public float attackCooldown = 1f;
    // Set the unit attack range, in this case 1f is for melee unit, which means it can only attack when 
    // it is colliding with the enemy unit or base.
    public float attackRange = 1f;
    public LayerMask groundLayer;

    // Set the unit initial state to be not deployed, where it will only change
    // once we know that the unit has landed on the ground.
    private bool hasDeployed = false;
    // Tracker to know if the unit is currently attacking or not
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private BaseHealth targetHealth;
    
    private Rigidbody2D rb;

    [Header("References")]
    public Transform groundCheck;
    // Now we set up a target for the unit to hit, which would be either the enemy base or the enemy unit, 
    // which will be used to determine if the unit has reached its destination or not.
    // public Transform target;

    void Update()
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
            // If target has been killed or destroyed, we should stop attacking and start moving forward again.
            if (targetHealth == null)
            {
                isAttacking = false;
                return;
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
                // Using the BaseHealth component of the target to call the TakeDamage method, 
                // which will reduce the hp of the target by the unit damage amount.
                targetHealth.TakeDamage(unitDamage);

                // For logging purposes, to see the attack has been done/executed in the console.
                // We should see unit attacked enemy base or unit, and then we should see the damage taken and 
                // the hp left of the target in the console.
                Debug.Log(gameObject.name + " attacked " + targetHealth.gameObject.name);

                // Start the attack cooldown since we just started attacking, in this case, it will be a
                // 1 second cooldown before the unit can attack again.
                attackTimer = attackCooldown;
            }

            // To ensure we dont run other codes below since when we are attacking, we should not be moving forward, 
            // we should only be attacking the target until it is destroyed or killed.
            return;
        }

        // Else if the unit is not attacking currently, we should be finding if there is any enemy unit or enemy base
        //  within the attack range of the unit.
        BaseHealth enemyInRange = FindEnemyInRange();

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

        // If the unit has been deployed, it will start moving forward automatically.
        // It will only stop when it reaches the target, which is either the enemy base or the enemy unit.
        // In this case it would be when any of the methods above has been triggered. 
        // Making sure that the update of the unit is relative with the user
        // laptop/PC ram using delta time
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
    
    // Method to check if the unit is grounded, which will be used to determine if the unit can move forward
    // or not
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    // Instead I will create a method called FindEnemyInRange, which checks if there is any
    // enemy unit or enemy base within the attack range of the unit. If there is, it will return the
    // BaseHealth component of the enemy unit or base, which will be used to deal damage to it. 
    private BaseHealth FindEnemyInRange()
    {
        // We use OverlapCircleAll to check for all colliders within the attack range, 
        // and then we check if any of them is an enemy.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // Looping though all possible colliders that are within the attack range, and check if any of them 
        // is an enemy unit or base.
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                return hit.GetComponent<BaseHealth>();
            }
        }

        // If there is no enemy unit or base within the attack range, we return null.
        return null;
    }

    // Method to handle collision with other objects, in this case, 
    // we want to check if the unit has collided with an enemy unit or the enemy base.
    // We scrape this since we wnat it to attack not just when colliding, but when in range.
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we are colliding with an enemy then we should start attacking.
        // Or else since Age of War is a one line attck game, if we collide with our allies, we need to
        // stop behind them and wait for them to be destoryed or move forward, to move.
        // Also later one we need to add it such that the unit is able to attack the enemy if it is a range
        // unit, so it attacks the enermy unit or base when it is within the attack range.
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isAttacking = true;
            Debug.Log("Attacking enemy!");

            BaseHealth targetHealth = collision.gameObject.GetComponent<BaseHealth>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(unitDamage);
            }

            Debug.Log(gameObject.name + " hit " + collision.gameObject.name);
        }
    }
    */
}
