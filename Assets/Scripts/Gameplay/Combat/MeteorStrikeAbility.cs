using System.Collections;
using UnityEngine;

public class MeteorStrikeAbility : MonoBehaviour
{
    [Header("Ability Settings")]
    
    [SerializeField] 
    private GameObject meteorPrefab;
    [SerializeField]
    private int currentMeteorExpCost = 250;
    // Total meteors to drop
    [SerializeField] 
    private int currentNumberOfMeteors = 50;
    
    [Header("Meteor Shower Settings")]
    // Time delay between each meteor
    [SerializeField] 
    private float delayBetweenSpawns = 0.1f;
    // How high above the battlefield the meteors spawn (Y-axis).
    [SerializeField] 
    private float spawnHeight = 12f;
    // Keeps meteors slightly away from both base collider spaces.
    [SerializeField] 
    private float horizontalPadding = 0.5f;

    [Range(0f, 0.5f)]
    // 0 means the full battlefield.
    // 0.5 means only the enemy half.
    // Set the metor Strike Area to be 0
    [Tooltip("0 means the full battlefield, while 0.5 means only the enemy half.")]

    [SerializeField]
    private float metorStrikeArea = 0f;

    [Header("Battlefield Boundaries")]
    // Collider_Space under New_Player_Base.
    [SerializeField] private Collider2D playerBaseColliderSpace;

    // Collider_Space_Enemy under New_Enemy_Base.
    [SerializeField] private Collider2D enemyBaseColliderSpace;
    
    [Header("Dependencies")]
    public EconomySystem economySystem; 

    // Just a tracker to see if the special ability is currently activated.
    private bool isCasting;

    // Public read-only states.
    public int CurrentMeteorExpCost => currentMeteorExpCost;
    public int CurrentNumberOfMeteors => currentNumberOfMeteors;
    public bool IsCasting => isCasting;

    public void CastMeteorStrike()
    {
        // Dont recast if the spell us currently being casted
        if (isCasting)
        {
            return;
        }

        // If any of the references in this script is not correct / undefined, early return.
        if (!ReferencesAreValid())
        {
            return;
        }

        // Try spending exp to cast the metor shower
        if (!economySystem.TrySpendExp(currentMeteorExpCost))
        {
            Debug.Log("Meteor Strike failed: Not enough Exp.");
            return;
        }

        StartCoroutine(SpawnMeteorShower());
        
        Debug.Log("Meteor Shower Cast! " + currentMeteorExpCost + " exp spent.");

    }

    // This method is to help receive and update (if needed)the current era's meteor settings from PlayerSpawner.
    public void ConfigureForEra(int meteorExpCost, int numberOfMeteors)
    {
        currentMeteorExpCost = Mathf.Max(0, meteorExpCost);

        currentNumberOfMeteors = Mathf.Max(1, numberOfMeteors);

        Debug.Log($"Meteor ability configured: " + $"{currentMeteorExpCost} EXP, " + $"{currentNumberOfMeteors} meteors.");
    }

    // A Coroutine allows us to pause code execution (yield) to create a delay
    private IEnumerator SpawnMeteorShower()
    {
        isCasting = true;

        // Defined the border of where the unitd will be. Note that for the player units, they move towards the
        // left side of the enemy collider, therefore:
        // - The right edge of the player collider is the battlefield start.
        // - The left edge of the enemy collider is the battlefield end.
        // Use min here to compensate for the diagonal metor shower
        float fullBattlefieldMinX = playerBaseColliderSpace.bounds.min.x + horizontalPadding;

        float battlefieldMaxX = enemyBaseColliderSpace.bounds.min.x - horizontalPadding;

        // If I wish to limit the area of the metors to strike on the battlefield.
        // Set default percentage to be 0, meaning we let it by default cover the whole battlefield.
        float battlefieldMinX = Mathf.Lerp(fullBattlefieldMinX, battlefieldMaxX, metorStrikeArea);


        // If the boundaries does not make sense, where left is greater than right, cannot cast any metor
        // and throw log error for debuggin
        if (battlefieldMinX >= battlefieldMaxX)
        {
            Debug.LogError("Meteor Strike failed: Battlefield collider boundaries overlap.");

            isCasting = false;
            yield break;
        }

        // If not for every metor rock, spawn them at a random poin this this boundary
        // and let it drop downwards.
        for (int i = 0; i < currentNumberOfMeteors; i++)
        {
            float randomX = Random.Range(battlefieldMinX, battlefieldMaxX);

            // Set the spawn position high in the air
            Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0f);

            // Spawn one meteor
            Instantiate(meteorPrefab, spawnPosition, meteorPrefab.transform.rotation);

            // Wait for a fraction of a second before looping to spawn the next one
            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        // After all the metors have finished falling, can set the isCasting to be false.
        isCasting = false;
    }

    // Method to help track all the necessary resoucres are defined for this script to work.
    private bool ReferencesAreValid()
    {
        if (meteorPrefab == null)
        {
            Debug.LogError("Meteor Strike failed: Meteor Prefab is not assigned.");
            return false;
        }

        if (economySystem == null)
        {
            Debug.LogError("Meteor Strike failed: Economy System is not assigned.");
            return false;
        }

        if (playerBaseColliderSpace == null)
        {
            Debug.LogError("Meteor Strike failed: Player Base Collider Space is not assigned.");
            return false;
        }

        if (enemyBaseColliderSpace == null)
        {
            Debug.LogError("Meteor Strike failed: Enemy Base Collider Space is not assigned.");
            return false;
        }

        return true;
    }

    // Method to visualise the metor boundary in Unity
    private void OnDrawGizmosSelected()
    {
        if (playerBaseColliderSpace == null || enemyBaseColliderSpace == null)
        {
            return;
        }

        float minimumX = playerBaseColliderSpace.bounds.max.x + horizontalPadding;

        float maximumX = enemyBaseColliderSpace.bounds.min.x - horizontalPadding;

        float centreX = (minimumX + maximumX) / 2f;
        float width = maximumX - minimumX;

        // Shows the full horizontal meteor range in the Scene view.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(new Vector3(centreX, spawnHeight, 0f), new Vector3(width, 0.5f, 0f));
    }

    // ===========================================
    // Unit-Testing Helpers
    // ===========================================
    public void SetEconomySystemForTesting(EconomySystem testEconomySystem)
    {
        economySystem = testEconomySystem;
    }

    public void SetMeteorPrefabForTesting(GameObject testMeteorPrefab)
    {
        meteorPrefab = testMeteorPrefab;
    }

    public void SetBattlefieldCollidersForTesting(Collider2D testPlayerCollider, Collider2D testEnemyCollider)
    {
        playerBaseColliderSpace = testPlayerCollider;
        enemyBaseColliderSpace = testEnemyCollider;
    }
}