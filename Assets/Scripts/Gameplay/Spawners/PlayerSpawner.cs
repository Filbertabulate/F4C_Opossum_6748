using UnityEngine;
// Old version
// using UnityEngine.InputSystem;
// We need this namespace to talk to TextMeshPro UI elements!

// Move the economy system to another script to better OOP.
//using TMPro;


public class PlayerSpawner : MonoBehaviour
{
    // Create a variable that is able to access all possible player units that we can spawn,
    // from a folder declared in the hierarchy called "PlayerUnits", where we can add all the player 
    // unit prefabs that we want to spawn in the game.
    private GameObject[] playerUnits;
    // Currently the set up needs me to do maunal assignment of the size of the array and the elements in the 
    // Unity editor, which is not ideal. thus I plan to switch to using a Resource folder to store the player 
    // unit prefabs, and then we can load all the prefabs in that folder into the array at startup.

    // We also need a variable to store the spawn point of the player units, which is a transform 
    // that we can set in the Unity editor.
    // Using Transform since it is a postion based variable, where we care mainly about the x, y positions
    // of said spawn point.
    public Transform spawnPoint;

    // Initially did not wanted to get this spawn cooldown, but since there is currently collision if I
    // do spawning of units too fast, a temporary fix is to have a cool down for spawn to prevent units jittering
    // and not being able to move.
    public float spawnCooldown = 0.5f;
    private float spawnCooldownTimer = 0f;

    // Get the player Base HealthBar that can be used later onwards since we want to stop spawnning units
    // if the based is destroyed
    public HealthSystem playerBaseHealth;

    // Now we need a script to access for the economy system.
    public EconomySystem economySystem;

    // Since I need to differentiate which unit comes first, I need a tracker to tag each spawned unit
    private long nextSpawnOrder = 0;

    [Tooltip("Type the cost of each unit here. Index 0 = Red, Index 1 = Yellow, Index 2 = Green")]
    public int[] unitCosts; 

    private void Awake()
    {
        // Load all the player unit prefabs from the Resources/PlayerUnits folder into the playerUnits array.
        // This way we can easily add or remove player unit prefabs from the folder without having to manually 
        // assign them in the Unity editor, which is more convenient and less error-prone.
        // The usage of Resources.LoadAll is based on the Unity documentation, where we can load all assets of a 
        // specific type from a folder in the Resources directory.
        // https://docs.unity3d.com/ScriptReference/Resources.LoadAll.html
        // As such, I had to create a folder called Resources in the Assets directory, and then create a subfolder
        // called PlayerUnits to store all the player unit prefabs that I want to spawn in the game.
        playerUnits = Resources.LoadAll<GameObject>("PlayerUnits");
    }

    // Update is called once per frame
    private void Update()
    {
        // If the based of the player (us in this case has been destroyed), stop spawning any more units
        // This is becuase the referece to the base has been destroyed, thus the refernce no longer points
        // to a base, which means the value becomes null.
        if (playerBaseHealth == null)
        {
            return;
        }
        
        // Continously tick down the cooldown timer in the background so the user can eventually spawn again
        if (spawnCooldownTimer > 0)
        {
            spawnCooldownTimer -= Time.deltaTime;
        }
    }

    // Changed to PUBLIC so UI buttons can access it.
    // Added 'int unitIndex' so the button can specify exactly WHICH unit to spawn, rather than a random one.
    public void SpawnUnitFromUI(int unitIndex)
    {
        // Check 1: Is our base destroyed? If yes, we can't spawn.
        if (playerBaseHealth == null) return;

        // Check 2: Are we still on cooldown?
        if (spawnCooldownTimer > 0f)
        {
            Debug.Log("Spawn is on cooldown!");
            return;
        }

        // Check 3: Do we even have an economy system to check if we can afford the unit? If not, we can't spawn.
         if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return;
        }
        
        // Check 4: Prevent errors if the array is empty or if the button passes a wrong number
        if (playerUnits == null || playerUnits.Length == 0)
        {
            Debug.LogWarning("No player units assigned to the spawner (Resources Folder in Assets)!");
            return;
        }

        // Check 5: Prevent errors if the button passes a wrong number, i.e. out of bounds of the array
        if (unitIndex < 0 || unitIndex >= playerUnits.Length)
        {
            Debug.LogWarning("Invalid unit index requested by the UI button!");
            return;
        }

        // --- NEW: Determine the cost of the requested unit ---
        int cost = GetUnitCost(unitIndex);

        // Now to check if the player can afford the unit requested ---
        // Note that this method if the player can afford the unit, it will automatically deduct the cost from 
        // the player's gold.
        if (!economySystem.TrySpendGold(cost))
        {
            // If the purchase fails, the method TrySpendGold will return false, 
            // and we can log a message to the console indicating that the player cannot afford the unit.
            Debug.Log("Not enough gold! Need: " + cost + ", current: " + economySystem.Gold);
            return;
        }

        // We use the specific index passed by the UI button instead of Random.Range
        GameObject unitToSpawn = playerUnits[unitIndex];

        // Then we can instantiate the selected unit at the spawn point's position and rotation.
        // https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        // Instantiate is a method that creates a copy of the given object, in this case, the unitToSpawn, 
        // at the specified position and rotation.
        // This way I only need to create one of that unit in the hierarchy, and I can spawn as many as I want 
        // by instantiating it.
        // Why I am storing the value of the spawned Unit is becuase I need to updates its spawn Order value
        // in its UnitMove Script so that the is Ally tracking method works as indented.
        GameObject spawnedUnit = Instantiate(unitToSpawn, spawnPoint.position, spawnPoint.rotation);

        // Obtain the UnitMove script form the spawned unit, if there is.
        UnitMove unitMove = spawnedUnit.GetComponent<UnitMove>();

        // If such a script is available in this ally unit, then I will define its spawn Order value as such
        // And increment the next spawnOrder value up by one to keep it unique, where lower spawnOrder number
        // means the unit was spawned first
        if (unitMove != null)
        {
            unitMove.InitialiseSpawnOrder(nextSpawnOrder);
            nextSpawnOrder++;
        }

        // Reset the cooldown timer so they can't instantly spam the button
        spawnCooldownTimer = spawnCooldown;
    }

    // A helper method to obtain the cost of a unit based on its index in the playerUnits array.
    private int GetUnitCost(int unitIndex)
    {
        // If it is a valid unit index, where  the array is not null, then we return that unit cost.
        if (unitCosts != null && unitIndex < unitCosts.Length)
        {
            return unitCosts[unitIndex];
        }

        // If not we get a debug log saying we cannot find that unit cost, and return 0 as the default value.
        Debug.LogWarning("Warning: You forgot to set the cost for unit " + unitIndex + " in the Inspector!");
        return 0;
    }
}