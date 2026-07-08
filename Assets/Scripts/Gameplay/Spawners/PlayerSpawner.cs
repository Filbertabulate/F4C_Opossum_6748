using System;
using NUnit.Framework;
using UnityEngine;
// Old version
// using UnityEngine.InputSystem;
// We need this namespace to talk to TextMeshPro UI elements!

// Move the economy system to another script to better OOP.
//using TMPro;

// Lets create a data class in the spawner script which just helps store info for every single playerable unit
// their name, thier prefab object, their goldcost and thier training cost
[System.Serializable]
public class PlayerUnitData
{
    public string unitName;
    public GameObject prefab;
    public int goldCost;
    public float trainTime;
}

// Creating another class to track the current era we are in so that when the era change, the PlayerUnitData
// does not need to be change, and likewise for the button as well, only the unit picture and cost text needs to
// change
[System.Serializable]
public class PlayerEraData
{
    public string eraName;
    public PlayerUnitData[] units;
}


public class PlayerSpawner : MonoBehaviour
{
    // Create a variable that is able to access all possible player units that we can spawn,
    // from a folder declared in the hierarchy called "PlayerUnits", where we can add all the player 
    // unit prefabs that we want to spawn in the game.
    // private GameObject[] playerUnits;

    // For Era tracking when we advance through different eras
    [Header("Era Unit Data")]
    [SerializeField] private PlayerEraData[] playerEras;
    [SerializeField] private int currentEraIndex = 0;

    /*
    Note that for the PlayerUnitData which is in the PlayerEraData class, I added the PlayerUnitData class
    as right now, I should no longer just scan the file, but instead look at all possible values found in the
    array of the PlayerUnitData class I just created. All while still following OOP practices
    
    Side Note, for the PlayerUnitData, I am still keeping it to be as such:
    - Index 0 = Cheap melee unit
    - Index 1 = range unit
    - Index 2 = tanky/beefy unit

    Currently the set up needs me to do maunal assignment of the size of the array and the elements in the 
    Unity editor, which is not ideal. thus I plan to switch to using a Resource folder to store the player 
    unit prefabs, and then we can load all the prefabs in that folder into the array at startup.
    */

    [Header("Spawn Dependencies")]
    // We also need a variable to store the spawn point of the player units, which is a transform 
    // that we can set in the Unity editor.
    // Using Transform since it is a postion based variable, where we care mainly about the x, y positions
    // of said spawn point.
    // For better OOP practices, I will make the fields that are public to private, but still make sure they are
    // serialized to ensure that this can still be seen in the Unity Inspector.
    [SerializeField]
    private Transform spawnPoint;

    // Likewise same idea changing public float values to private values for those that should still be seen
    // in the Unity inspector.

    // Initially did not wanted to get this spawn cooldown, but since there is currently collision if I
    // do spawning of units too fast, a temporary fix is to have a cool down for spawn to prevent units jittering
    // and not being able to move.
    // [SerializeField]
    // private float spawnCooldown = 0.5f;

    // Get the player Base HealthBar that can be used later onwards since we want to stop spawnning units
    // if the based is destroyed
    [SerializeField]
    private HealthSystem playerBaseHealth;

    // Now we need a script to access for the economy system.
    [SerializeField]
    private EconomySystem economySystem;

    // For each unit time tracking till they have finished training
    private float spawnCooldownTimer = 0f;

    // Since I need to differentiate which unit comes first, I need a tracker to tag each spawned unit
    private long nextSpawnOrder = 0;

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

    // Refractoring this entire code so that the spawning of the player unit if successfully would return True
    // False otherwise. This is so that I can do Asset testing of true and false later one.

    // Changed to PUBLIC so UI buttons can access it.
    // Added 'int unitIndex' so the button can specify exactly WHICH unit to spawn, rather than a random one.
    public void SpawnUnitFromUI(int unitIndex)
    {
        // Try out the refrector method
        TrySpawnUnit(unitIndex);
    }

    // Refrector method to spawn the unit desired
    // Set to public so that test script can test unit spawnning if it works.
    public bool TrySpawnUnit(int unitIndex)
    {
        // Check 1: Is our base destroyed? If yes, we can't spawn.
        if (playerBaseHealth == null) 
        {
            Debug.LogWarning("Based has been destroyed!");
            return false;
        }

        // Check 2: Are we still on cooldown?
        if (spawnCooldownTimer > 0f)
        {
            Debug.Log("Spawn is on cooldown!");
            return false;
        }

        // Check 3: Do we even have an economy system to check if we can afford the unit? If not, we can't spawn.
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return false;
        }

        // Check 4: If spawnPoint is not assigned, return false since we dont have a place for the unit
        // to come out from
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point not assigned!");
            return false;
        }

        // Now we want to extract the unit selected by the player from the GetCurrentEraUnit class, so that
        // based on the era, the unitIndex given from the Unit select toolbar matches the unit selected of that
        // specific era
        PlayerUnitData selectedUnit = GetCurrentEraUnit(unitIndex);

        // Check 9:
        // Lets check if the unit is available / been defined correctly. We do not want out of index units.
        if (selectedUnit == null || selectedUnit.prefab == null)
        {
            Debug.LogWarning("Selected unit data or prefab is missing!");
            return false;
        }

        // Check 10:
        // Now to check if the player can afford the unit requested ---
        // Note that this method if the player can afford the unit, it will automatically deduct the cost from 
        // the player's gold.
        if (!economySystem.TrySpendGold(selectedUnit.goldCost))
        {
            // If the purchase fails, the method TrySpendGold will return false, 
            // and we can log a message to the console indicating that the player cannot afford the unit.
            Debug.Log("Not enough gold! Need: " + selectedUnit.goldCost + ", current: " + economySystem.Gold);
            return false;
        }

        // Then we can instantiate the selected unit at the spawn point's position and rotation.
        // https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        // Instantiate is a method that creates a copy of the given object, in this case, the unitToSpawn, 
        // at the specified position and rotation.
        // This way I only need to create one of that unit in the hierarchy, and I can spawn as many as I want 
        // by instantiating it.
        // Why I am storing the value of the spawned Unit is becuase I need to updates its spawn Order value
        // in its UnitMove Script so that the is Ally tracking method works as indented.
        GameObject spawnedUnit = Instantiate(selectedUnit.prefab, spawnPoint.position, spawnPoint.rotation);

        // Obtain the UnitMove script form the spawned unit, if there is.
        UnitMove unitMove = spawnedUnit.GetComponent<UnitMove>();

        // Check 11:
        // If such a script is available in this ally unit, then I will define its spawn Order value as such
        // And increment the next spawnOrder value up by one to keep it unique, where lower spawnOrder number
        // means the unit was spawned first
        if (unitMove != null)
        {
            unitMove.InitialiseSpawnOrder(nextSpawnOrder);
            nextSpawnOrder++;
        }

        // Reset the cooldown timer so they can't instantly spam the button
        spawnCooldownTimer = selectedUnit.trainTime;

        // If we reach this point in the code, it means the unit has been spawned, thus it means that
        // we should return true since the unit was successfully spawned
        return true;
    }

    // Helper method to get the specific PlayerUnitData based on era the player is currently on
    private PlayerUnitData GetCurrentEraUnit(int unitIndex)
    {
        // Check 5: (following the trySpawnUnit check counter)
        // Firstly if the current era class is null or empty, we cannot do anything thus we only can return null
        // with waring logs.
        if (playerEras == null || playerEras.Length == 0)
        {
            Debug.LogWarning("No player eras assigned!");
            return null;
        }

        // Check 6: (following the trySpawnUnit check counter)
        // If there is an era in the playerEra class, we need to ensure that firstly our current era the use
        // is on is a valid era, so that we can actaully find the unit the player wants to spawn based on that
        // said era, or else we log that the era / stage propsed is not correct, and return null
        if (currentEraIndex < 0 || currentEraIndex >= playerEras.Length)
        {
            Debug.LogWarning("Invalid current era index!");
            return null;
        }

        // If not let me get the details of the era name and the units that the player can spawn from that era
        PlayerEraData currentEra = playerEras[currentEraIndex];

        // Check 7: (following the trySpawnUnit check counter)
        // If that era cannot be extracted and/or the units in that era are empty, then we cannot get any unit
        // out to spawn, thus log it and return null
        if (currentEra == null || currentEra.units == null || currentEra.units.Length == 0)
        {
            Debug.LogWarning("Current era has no units assigned!");
            return null;
        }

        // Check 8: (following the trySpawnUnit check counter)
        // Now based on the unit index given from the unitselect toolbar, we need to ensure that unit the player
        // wants to spawn exist range of the units found in the current era units array
        // If not we return null and log the era.
        if (unitIndex < 0 || unitIndex >= currentEra.units.Length)
        {
            Debug.LogWarning("Invalid unit index requested by the UI button!");
            return null;
        }

        // If all the checks succed, then we extract the info of the unit the player want from the current
        // era they are in.
        return currentEra.units[unitIndex];
    }

    // This method is for the future implemention, where we will run this when the user wants
    // progress to a new era.
    public bool TrySetEra(int eraIndex)
    {
       // If the player era class is not null, and the new era index we want to "promote" to is a valid
       // era, then we change the era index to the next requested era.
        if (playerEras == null || eraIndex < 0 || eraIndex >= playerEras.Length)
        {
            Debug.LogWarning("Invalid era index!");
            return false;
        }

        currentEraIndex = eraIndex;
        return true;
    }


    /* No Longer need this function
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
    */

    // These methods are for the testing set up, i.e. to say for creating an empty object and putting in this
    // script, since all the fields are set to private, I need "setter" methods to set these fields up correctly
    public void SetPlayerErasForTesting(PlayerEraData[] testEras)
    {
        playerEras = testEras;
    }

    // To set up the other scripts since this playerSpawner script need other scripts for it to run smoothly
    public void SetDependenciesForTesting(Transform testSpawnPoint, HealthSystem testPlayerBaseHealth,
                                          EconomySystem testEconomySystem)
    {
        spawnPoint = testSpawnPoint;
        playerBaseHealth = testPlayerBaseHealth;
        economySystem = testEconomySystem;
    }

    // Getter methods for testing scripts later on
    public int GetCurrentEraIndex()
    {
        return currentEraIndex;
    }

    public String GetCurrentEraName()
    {
        return playerEras[currentEraIndex].eraName;
    }

    public float GetCooldownTimerForTesting()
    {
        return spawnCooldownTimer;
    }
}