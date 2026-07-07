using UnityEngine;

// This script is created to control the arrow/bolt fired by the turret.
// To make the arrow curve, this script manually moves from the turret FirePoint to the target using a curved arc.
public class TurretArcProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    // How long the projectile takes to reach the target.
    // higher time means faster arrow speed
    public float projectileSpeed = 12f;
    // How high the projectile arcs upward before falling.
    // larger value mean the arrow creates a higher curve
    public float arcHeight = 1.5f;

    // Damage dealt when the arrow reaches the target.
    private int damage;
    // Starting position of the arrow when spawned.
    private Vector3 startPos;
    // Target enemy that this projectile is flying toward.
    private Transform target;
    // Time needed to reach target
    private float travelTime;
    // Tracks how long the projectile has been flying.
    private float timer = 0f;

    // The turret will call this method when the projectile is created.
    // this is to "create" the arrow animation
    public void Initialize(Transform targetTransform, int projectileDamage)
    {
        // Store the enemy target.
        target = targetTransform;
        // Store the damage value from the turret.
        damage = projectileDamage;
        // Store the position of where the projectile starts from.
        startPos = transform.position;

        // Travel time depends on distance, so far targets take longer to hit
        float distance = Vector3.Distance(startPos, target.position);
        // update the proposed travel time
        travelTime = distance / projectileSpeed;

        // Safety check so travelTime is never 0, i.e. make sure there is no instant hit kind of scenario
        travelTime = Mathf.Max(travelTime, 0.1f);

        // In case if any of the previous arrow was not destroyed, we will destory, it,
        // so that no buggy exta arrow will be seen
        // we make sure that if by the time the arrow alr travelled finish, giving it a 1 second
        // buffer before destorying it if is still being shown
        Destroy(gameObject, travelTime + 1f);
    }

    void Update()
    {
        // If the enemy has been destroyed before the arrow reachs, we despawn the arrow, so it dont hit
        // another unit, since the arrow is lock on to that enemy unit only
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Update the time based on real time, not by frame count
        timer += Time.deltaTime;

        // To convert the current time into progress from 0 to 1, so we know that
        // the time travelled for the arrow at any point of time, the progress of the 
        // arrow travelled is based on t
        float t = timer / travelTime;

        // If the arrow reach the target, deal damage
        if (t >= 1f)
        {
            HitTarget();
            return;
        }

        // As the target position updates every fram, the arrow needs to keep tracking the moving enemy
        Vector3 targetPos = target.position;

        // Move normally from start to the target
        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

        // Adding the arc value for the arrow projectile
        // Why we choose to use sin is because sin wave enforces a smooth curve up and down, so
        // that by normalising the flight progress, i.e. for (t: 0 -> 1) to the upper half of a unit circle 
        // (0 -> pi radians), the smooth curve can be obtained.
        // Note that Mathf.Sin automatically handles our boundary conditions, perfectly zeroing out at both 
        // the start (t=0) and end (t=1).
        // As a result, it helps the projectile to smoothly decelerate at its apex for a natural, weighted 
        // look—all while keeping the trajectory 100% predictable, and does not look out of place in terms of
        // game physics.
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        // updating the position based on the arc we want to get, based of the height
        // if we reach the apex, the arc vlaue should be negative, indication the arrow position
        // should be falling downwards
        currentPos.y += arc;

        // Updagte the arrow position
        transform.position = currentPos;

        // This is to roate the arrow to face the correct movement of direction, i.e. if going up arrow tip
        // point upwards, if going down arrow tip point downwards.
        Vector3 direction = targetPos - transform.position;

        // Credit for this portion, the if statemet, via ChatGPT, as IDK how the math was suppose to work for 
        // this scenario, as I was asking it what if the pos was 0 would my arc not break, so this the code they 
        // gave.

        // Ensure the direction is not a zero vector. This helps to prevent errors when calculating the angle 
        // if the projectile is exactly on top of the target.
        if (direction != Vector3.zero)
        {
            // Calculate the angle (in degrees) between the positive x-axis and the
            // direction vector pointing towards the enemy.
            // Mathf.Atan2 returns the angle in radians, so we convert it into degrees.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Rotate the projectile so that its sprite always faces the direction
            // it is currently travelling towards, making the flight animation
            // appear more realistic.
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Method to deal damge to the target when the projection has reached its destination
    private void HitTarget()
    {
        // Only do damage if enemy still exist
        if (target != null)
        {
            // Obtain the enemy current HP value
            HealthSystem health = target.GetComponentInParent<HealthSystem>();

            // If the unit has a health system, which there should be, then
            // we can do the take damage method, this just for safety checking
            // since if health is null, we can use a takedamge method on a null
            // preventing errors from occuring
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // After dealing the damage, we will destroy this projectile "arrow"
        Destroy(gameObject);
    }
}