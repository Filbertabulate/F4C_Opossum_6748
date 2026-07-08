using System;
using NUnit.Framework;
using UnityEngine;
// I need to use the system collections to gain access to a Queue<T> which is how I want to 
// queue up the units to spawn.
using System.Collections.Generic;
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

// I will create a new class called UnitProductionQueue, such that we can port over the old
// single cooldown, reject while busy to become a global FIFO queue. 
// 
// Creating this class helps to ensure the PlayerSpawner keeps a Single responsibility, which is that
// PlayerSpawner handles economy/era/spawning.
//
// All the queueing idea of how many unit are in front of the queue, what is the current unit being train
// will be handled by this class.
[System.Serializable]
public class UnitProductionQueue
{
    // I will create the FIFO queue where only PlayerUnitData references are stored
    // Note that since every queued entry of the same unit type is identical, we would not
    // need to create unique instances until the moment we actually spawn.
    private Queue<PlayerUnitData> queue = new Queue<PlayerUnitData>();
 
    // Whichever unit is at the front of the line and actively being trained to deploy.
    private PlayerUnitData currentlyTraining;
 
    // Counts down from currentlyTraining.trainTime to 0.
    private float trainTimer;
 
    // I need to create public read-only view of state, so PlayerSpawner (and UI/tests) can query 
    // the status of is training without being able to mutate internal state directly.
    // Set up the IsTraining boolean to first contain if there is a unit being actively train, return false
    // else return true
    public bool IsTraining => currentlyTraining != null;
    
    // Public reference to the current unit we are training
    public PlayerUnitData CurrentlyTraining => currentlyTraining;

    // Public reference to the total number of units in the queue. Note that we count the length of the current
    // queue, as well as the recently pop out element if that element / unit is currently being trained.
    public int Count => queue.Count + (IsTraining ? 1 : 0);
 
    // Method to called whenever the unit can successfully join the queue, i.e.
    // the player has the necessary amount need to pays for that unit.
    public void Enqueue(PlayerUnitData unit)
    {
        queue.Enqueue(unit);
 
        // If nothing is currently training, this new unit should start immediately.
        if (!IsTraining)
        {
            StartNext();
        }
    }
 
    // Pops out the next unit (if any) off the queue and begins its training timer.
    private void StartNext()
    {
        // If there is no unit in the queue, then we reset the train timer and currently taining to
        // be thier "default", which is saying there is no unit being trained
        if (queue.Count == 0)
        {
            currentlyTraining = null;
            trainTimer = 0f;
            return;
        }
 
        // Else we pop out the first elemnt found in the queue to obtain the unit we want to be currently
        // training, and then set the train timer to be that current unit train time as defined.
        currentlyTraining = queue.Dequeue();
        trainTimer = currentlyTraining.trainTime;
    }
 
    // I need to create a method that will be called once per frame in the update() method of the
    // PlayerSpawner class. This is so that the trainTimer will actually tick down correctly to simluate that
    // the unit is currently being trained.
    // Also this method will return the PlayerUnitData if that unit has finished training during that update frame,
    // or null if nothing finished. 
    // This is so that in the PlayerSpawner class, we will uses this return value to know when to actually 
    // Instantiate a unit to be deployed.
    public PlayerUnitData PlayerUnitTrainingTick(float deltaTime)
    {
        if (!IsTraining)
        {
            return null;
        }
 
        trainTimer -= deltaTime;
 
        // Once the training of the unit ends, we need to start the next training process.
        if (trainTimer <= 0f)
        {
            PlayerUnitData finishedUnit = currentlyTraining;
            StartNext();
            return finishedUnit;
        }
 
        // If the unit is not ready to be deployed, i.e. not fully trained yet, we return null.
        return null;
    }
 
    // This is for the UI, where I want to change the unit being train the the queue to show a +2 / +3 based on 
    // how many of a given unit type are currently waiting (Not including the one being trained.)
    // Note that if the count is 0, then I would not show any text at the unit side
    public int GetPendingCount(PlayerUnitData unit)
    {
        int count = 0;
 
        foreach (PlayerUnitData queuedUnit in queue)
        {
            if (queuedUnit == unit)
            {
                count++;
            }
        }
 
        return count;
    }
 
    // For UI purposes: To obtain the 0 to 1 progress bar value of the currently training unit, 
    // which helps to fill up the training bar.
    // Returns -1 for any unit that isn't the one currently at the front of the queue.
    // Why -1 is so we dont get it confused an show a bar when we are not suppose to do so.
    public float GetProgressBar(PlayerUnitData unit)
    {
        if (currentlyTraining != unit || currentlyTraining.trainTime <= 0f)
        {
            return -1f;
        }
 
        return 1f - (trainTimer / currentlyTraining.trainTime);
    }
 
    // Need to create Testing helpers, i.e getting and setter method to simulate the queue in use.
    public float GetCooldownTimerForTesting()
    {
        return trainTimer;
    }
 
    public void ClearForTesting()
    {
        queue.Clear();
        currentlyTraining = null;
        trainTimer = 0f;
    }
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
    // private float spawnCooldownTimer = 0f;

    // Changing the spawnCooldownTimer float to now beocme a dedicated UnitProductionQueue instance.
    // As such, what I am trying to change is instead of a "Am I (player) currently on a cooldown (yes / no)",
    // it now lets the class handle the queue / timer tracking, i.e. the class becomes a bookeeper for this 
    // function
    private UnitProductionQueue productionQueue = new UnitProductionQueue();

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
        
        // Now we need to use the productionQueue method to see if we can generate a unit to be spawn or not,
        // if spawning is successful.
        // As such, I will be using the PlayerUnitTrainingTick() method, which both advances the timer AND tells 
        // us if the front-of-queue unit just finished training. If the unit has finished training, then we spawn 
        // it and the queue automatically starts the next one in line .
        // All of this are handled inside UnitProductionQueue.StartNext().
        PlayerUnitData finishedUnit = productionQueue.PlayerUnitTrainingTick(Time.deltaTime);
        // If the unit has been finished, it means that we would obtain the info of that said unit to train,
        // in which we would want to spawn that said unit.
        if (finishedUnit != null)
        {
            SpawnUnit(finishedUnit);
        }
    }

    // Refractoring this entire code so that the spawning of the player unit if successfully would return True
    // False otherwise. This is so that I can do Asset testing of true and false later one.

    // Changed to PUBLIC so UI buttons can access it.
    // Added 'int unitIndex' so the button can specify exactly WHICH unit to spawn, rather than a random one.
    public void QueueAndSpawnUnitFromUI(int unitIndex)
    {
        // Try out the refrector method
        TryQueueToSpawnUnit(unitIndex);
    }

    // Refrector method to spawn the unit desired
    // Set to public so that test script can test unit spawnning if it works.
    // Note that since I am changing the queuing system, the "true" in this method now means
    // that the unit has been "successfully QUEUED (and paid for)", and 
    // not "successfully instantiated in the world". As such, the unit may still be waiting in line behind
    // others. 
    // This matches the AgeOfWar2-style behaviour where clicking a unit always accepts the order, provided that
    // the user can afford it, rather than rejecting the click while busy.
    public bool TryQueueToSpawnUnit(int unitIndex)
    {
        // Check 1: Is our base destroyed? If yes, we can't spawn.
        if (playerBaseHealth == null) 
        {
            Debug.LogWarning("Based has been destroyed!");
            return false;
        }

        // No longer need to Are we sitll on cooldown check since we are changing it to be handlled within the
        // queue class itself.

        // Check 2: Do we even have an economy system to check if we can afford the unit? If not, we can't spawn.
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return false;
        }

        // Check 3: If spawnPoint is not assigned, return false since we dont have a place for the unit
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

        // Check 8:
        // Lets check if the unit is available / been defined correctly. We do not want out of index units.
        // Note this this is a double loop checking since the GetCurrentEraUnit method will return null and throw
        // a logwarning when:
        // 1) era list is empty
        // 2) when current era index we are on is no longer valid
        // 3) Current era is valid but there is no units assign to that era
        // 4) the unit we are looking for from that era is invalid.
        if (selectedUnit == null || selectedUnit.prefab == null)
        {
            Debug.LogWarning("Selected unit data or prefab is missing!");
            return false;
        }

        // Check 9:
        // Now to check if the player can afford the unit requested ---
        // Note that this method if the player can afford the unit, it will automatically deduct the cost from 
        // the player's gold.
        // Another thing to note is that now, gold can still be deducted immediately on click, even if the unit 
        // would be in queue  instead of spawning straight away. This helps avoids the scenario where 
        // we queue a unit only to realise there is not enough gold to actually make the unit.
        if (!economySystem.TrySpendGold(selectedUnit.goldCost))
        {
            // If the purchase fails, the method TrySpendGold will return false, 
            // and we can log a message to the console indicating that the player cannot afford the unit.
            Debug.Log("Not enough gold! Need: " + selectedUnit.goldCost + ", current: " + economySystem.Gold);
            return false;
        }

        // Now we are changing this method, so instead of spawnning the unit straight away, I will now hand the
        // said unit that can be purchase and valid to join the production queue. Note that if nothing else
        // is currently being trained, the said unit will start immediately inside Enqueue(), if not it 
        // will wait for its turn in FIFO order.
        productionQueue.Enqueue(selectedUnit);

        // If we have reached the end of this point, it means that the unit has been successfully queued
        // and paid for, thus we need to return true.
        return true;

        /*
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

        // Check 10:
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
        */
    }

    // So right now the flow is QueueUnitFromUI -> TryQueueUnit -> Update -> PlayerUnitTrainingTick -> SpawnUnit
    // So this method is where we would actually creates the GameObject in the world.
    private bool SpawnUnit(PlayerUnitData unitToSpawn)
    {
        // Then we can instantiate the selected unit at the spawn point's position and rotation.
        // https://docs.unity3d.com/ScriptReference/Object.Instantiate.html
        // Instantiate is a method that creates a copy of the given object, in this case, the unitToSpawn, 
        // at the specified position and rotation.
        // This way I only need to create one of that unit in the hierarchy, and I can spawn as many as I want 
        // by instantiating it.
        // Why I am storing the value of the spawned Unit is becuase I need to updates its spawn Order value
        // in its UnitMove Script so that the is Ally tracking method works as indented.
        GameObject spawnedUnit = Instantiate(unitToSpawn.prefab, spawnPoint.position, spawnPoint.rotation);

        // Obtain the UnitMove script form the spawned unit, if there is.
        UnitMove unitMove = spawnedUnit.GetComponent<UnitMove>();

        // Check 10:
        // If such a script is available in this ally unit, then I will define its spawn Order value as such
        // And increment the next spawnOrder value up by one to keep it unique, where lower spawnOrder number
        // means the unit was spawned first
        if (unitMove != null)
        {
            unitMove.InitialiseSpawnOrder(nextSpawnOrder);
            nextSpawnOrder++;
        }

        // If we reach this point in the code, it means the unit has been spawned, thus it means that
        // we should return true since the unit was successfully spawned
        // But at this stage it should usually just be true all the time
        return true;
    }

    // Helper method to get the specific PlayerUnitData based on era the player is currently on
    private PlayerUnitData GetCurrentEraUnit(int unitIndex)
    {
        // Check 4: (following the trySpawnUnit check counter)
        // Firstly if the current era class is null or empty, we cannot do anything thus we only can return null
        // with waring logs.
        if (playerEras == null || playerEras.Length == 0)
        {
            Debug.LogWarning("No player eras assigned!");
            return null;
        }

        // Check 5: (following the trySpawnUnit check counter)
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

        // Check 6: (following the trySpawnUnit check counter)
        // If that era cannot be extracted and/or the units in that era are empty, then we cannot get any unit
        // out to spawn, thus log it and return null
        if (currentEra == null || currentEra.units == null || currentEra.units.Length == 0)
        {
            Debug.LogWarning("Current era has no units assigned!");
            return null;
        }

        // Check 7: (following the trySpawnUnit check counter)
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

    // Keep the same name so that the test scripts would not complain as much
    public float GetCooldownTimerForTesting()
    {
        return productionQueue.GetCooldownTimerForTesting();
    }
 
    // Getter methods to exposes queue state for tests and for the eventual UI badge/progress bar code.
    public int GetPendingCount(int unitIndex)
    {
        // So based on the current era we are at, we want to get the unit we are looking for based on that index
        PlayerUnitData unit = GetCurrentEraUnit(unitIndex);
        // If the unit that we are looking for is a valid unit, then we want to get the unit count of the number
        // of units currently in the queue
        if(unit != null)
        {
            return productionQueue.GetPendingCount(unit);
        }

        // else if it is not a valid unit, then we set the return to -1 since it is impossible for us to
        // own the queue somthing in theory
        return -1;
    }
 
    // Likewise if I want to get the current traning bar progess of the current unit
    // So the scenarios here are
    // return the percentage (training progress) from 0 to 1 if the unit we are looking for is the current unit in
    // training
    // return -1 if the unit we are looking for is not the current unit in training
    // return -2 if we cannot find the unit we are searching for.
    public float GetTrainProgress(int unitIndex)
    {
        PlayerUnitData unit = GetCurrentEraUnit(unitIndex);
        
        if (unit != null)
        {
            return productionQueue.GetProgressBar(unit);
        }

        return -2f;
    }
 
    // Just a checker to see if the queue right now is empty or not, the training queue, not the waiting to
    // go into training queue.
    public bool IsAnyUnitTraining()
    {
        return productionQueue.IsTraining;
    }
}