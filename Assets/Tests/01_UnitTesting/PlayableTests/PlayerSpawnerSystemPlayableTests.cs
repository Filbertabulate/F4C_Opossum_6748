using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// For testing of the PlayerSpawn Systems, even though this is unit testing, the player spawner needs multiple
// others scripts as well to function properly, i.e. the position of the spawer where the units will be deployed
// the economy system to track that unit can be deployed or not
// the base helath of the ally / enemy base since I have set that spawnning only works when the base is not 
// destroyed, which makes sense.
// Also I need to prefabs of "fake" units as well

// NOTE THAT FOR THE SPRITE / PREFAB, I WILL ADD UNIT MOVE LATER AS PART OF INTEGRATION TESTING, to test
// the spawn order as well as the move out order of the units.

// Changes made, which are important context for the update test scripts:
// TryQueueToSpawnUnit() when returned true now means "successfully QUEUED and paid for", not "instantly spawned".
// Moreover, the GameObject creation only happens once UnitProductionQueue.PlayerUnitTrainingTick() counts a unit's
// trainTime down to 0 inside PlayerSpawner.Update().
// This would mean that the previous method of CountObjectsByName() immediately after calling TryQueueToSpawnUnit() 
// now need to yield/wait for that unit's trainTime to elapse first. 
// Tests that were checking "the second click gets rejected while busy" are rewritten, 
// since the queue model ACCEPTS the click (and takesthe gold) instead of rejecting it - it just waits its turn.

public class PlayerSpawnerSystemPlayableTests
{
    // Creating references to be used throughout all the test, where for each test, there
    // will be a set up and tear down process of the spawner state.
    private GameObject spawnerObject;
    private PlayerSpawner spawner;

    private GameObject economyObject;
    private EconomySystem economySystem;

    private GameObject baseObject;
    private HealthSystem baseHealth;

    private GameObject spawnPointObject;
    private Transform spawnPoint;

    private GameObject era1SmallUnitPrefab;
    private GameObject era1MediumUnitPrefab;
    private GameObject era1LargeUnitPrefab;

    private GameObject era2SmallUnitPrefab;
    private GameObject era2MediumUnitPrefab;
    private GameObject era2LargeUnitPrefab;

    // The set up process that will initialise each compontent needed throughout all the tests
    [SetUp]
    public void SetUp()
    {
        // This is creating a game object that will store the player spawner script, which in how the game is set
        // up, would be located at the player spawn point
        // But in this case to make it clearer, I will just create this as a sepeate object, think of it more
        // like player spawner manager in this case.
        spawnerObject = new GameObject("PlayerSpawner");
        spawner = spawnerObject.AddComponent<PlayerSpawner>();

        // Creating a sepeare object to store the economy system script, i.e. in this case would be the economy
        // manager object
        economyObject = new GameObject("EconomySystem");
        economySystem = economyObject.AddComponent<EconomySystem>();
        // Set up the economy state, 100 gold 0 exp to start
        economySystem.SetResourcesForTesting(100, 0);
        // I dont want passive income generation for this test portion so that I can do gold based deduction
        // purely on total start minus cost of each unit
        economySystem.SetPassiveIncomeGenerationBox(false);

        // Creating the ally base for this test, so that we can track if base is not destroyed / 0HP, then
        // we can spawn units. As such, the health system component for this player base is needed
        baseObject = new GameObject("PlayerBase");
        baseHealth = baseObject.AddComponent<HealthSystem>();

        // Defining a specific spawn point where the unit should be deployed. Since this is a 2D game, I will
        // set the coordinates of the spawn point to be at x: 5, y: 2, z: 0. why y not 0 is becuase I have set
        // a script later on that account for "falling" unit to not move till hit ground, so y can be any number.
        // As for the Z, since we are working on a 2D game, the z axis is not needed.
        spawnPointObject = new GameObject("SpawnPoint");
        spawnPointObject.transform.position = new Vector3(5f, 2f, 0f);
        // Since the PlayerSpawner scripts needs this transform position, I neeed to create a variable
        // of type transform called spawnpoint to hold the spawnpoint coordinates.
        spawnPoint = spawnPointObject.transform;

        // Now I need to create two different units, in this case I will called it smallUnit and MediumUnit
        // for testing that the units would be spawn correctly.
        // Note that for all the prefab units, they come with the unitMove script to see if the unit is moving
        // correcty in the right direction.
        era1SmallUnitPrefab = new GameObject("Era1SmallUnit");
        // era1SmallUnitPrefab.AddComponent<UnitMove>();

        era1MediumUnitPrefab = new GameObject("Era1MediumUnit");
        // era1MediumUnitPrefab.AddComponent<UnitMove>();

        era1LargeUnitPrefab = new GameObject("Era1LargeUnit");
        // era1LargeUnitPrefab.AddComponent<UnitMove>();

        // Now since the PlayerSpawner scripts wants the playable units to be in the PlayerUnitData class, i.e.
        // I need to define the small unit and medium unit to contain the correct info of unitname, gold cost
        // and train time / cooldown time that will be used in this playerspawner testing.
        PlayerUnitData era1SmallUnit = new PlayerUnitData
        {
            unitName = "Era 1 Small",
            prefab = era1SmallUnitPrefab,
            goldCost = 10,
            trainTime = 0.3f
        };

        PlayerUnitData era1MediumUnit = new PlayerUnitData
        {
            unitName = "Era 1 Medium",
            prefab = era1MediumUnitPrefab,
            goldCost = 25,
            trainTime = 0.8f
        };

        PlayerUnitData era1LargeUnit = new PlayerUnitData
        {
            unitName = "Era 1 Large",
            prefab = era1LargeUnitPrefab,
            goldCost = 50,
            trainTime = 1.5f
        };

        // After which, I need to create the era class, to store the playerunit data into 
        // the correct field. This is all so that we can toggle between eras easier later on as we scale up
        // the project.
        PlayerEraData testEra1 = new PlayerEraData
        {
            eraName = "Test Era 1",
            units = new PlayerUnitData[] { era1SmallUnit, era1MediumUnit, era1LargeUnit}
        };


        // Now we also create another era segment for testing later on, mainly just switching between eras
        // and trying to spawn those era units

        era2SmallUnitPrefab = new GameObject("Era2SmallUnit");
        // era2SmallUnitPrefab.AddComponent<UnitMove>();

        era2MediumUnitPrefab = new GameObject("Era2MediumUnit");
        // era2MediumUnitPrefab.AddComponent<UnitMove>();

        era2LargeUnitPrefab = new GameObject("Era2LargeUnit");
        // era2LargeUnitPrefab.AddComponent<UnitMove>();

        PlayerUnitData era2SmallUnit = new PlayerUnitData
        {
            unitName = "Era 2 Small",
            prefab = era2SmallUnitPrefab,
            goldCost = 20,
            trainTime = 0.5f
        };

        PlayerUnitData era2MediumUnit = new PlayerUnitData
        {
            unitName = "Era 2 Medium",
            prefab = era2MediumUnitPrefab,
            goldCost = 40,
            trainTime = 1f
        };

        PlayerUnitData era2LargeUnit = new PlayerUnitData
        {
            unitName = "Era 2 Large",
            prefab = era2LargeUnitPrefab,
            goldCost = 75,
            trainTime = 2f
        };

        // After which, I need to create the era class, to store the playerunit data into 
        // the correct field. This is all so that we can toggle between eras easier later on as we scale up
        // the project.
        PlayerEraData testEra2 = new PlayerEraData
        {
            eraName = "Test Era 2",
            units = new PlayerUnitData[] { era2SmallUnit, era2MediumUnit, era2LargeUnit}
        };

        // Setting up the spawner maanger in this case by addding the defined spawnPoint, basehealth tracking,
        // economy systes, as well as the playable eras that the test can use, in this case is just one 
        // era for now
        spawner.SetDependenciesForTesting(spawnPoint, baseHealth, economySystem);
        // Note: the usage of {} in this case is cuz we are doing a Collection Initializer, not a argument
        // list, which is represented as ().
        spawner.SetPlayerErasForTesting(new PlayerEraData[] { testEra1, testEra2 });
    }

    [TearDown]
    public void TearDown()
    {
        // Find all possible GameObject(s) that have been create, such that we go and destory those objects
        // before moving on to a new test
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            Object.DestroyImmediate(obj);
        }
    }

    // Basically this is just a counter tracker to see of all the objects created, an in terms, all the
    // game object clones created, how many game objects have names that matches the query name we are looking for
    private int CountObjectsByName(string objectName)
    {
        int count = 0;

        // Find all possible GameObject(s) that have been create, such that we go and destory those objects
        // before moving on to a new test
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                count++;
            }
        }

        return count;
    }

    // Create a scenario where the unit should not be able to spawn as the based has been destroyed.
    [UnityTest]
    public IEnumerator CannotQueue_WhenPlayerBaseIsNull()
    {
        spawner.SetDependenciesForTesting(spawnPoint, null, economySystem);

        // Since the base health is gone, the base should be destroyed log should be thrown out
        LogAssert.Expect(LogType.Warning, "Based has been destroyed!");

        // the try queue method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        // No unit of that name should be create as an object.
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        // Gold should remain unchanged.
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Testing for error catching when the economyUnit is not defined for the spawner system 
    [UnityTest]
    public IEnumerator CannotQueue_WhenEconomySystemIsMissing()
    {
        spawner.SetDependenciesForTesting(spawnPoint, baseHealth, null);

        LogAssert.Expect(LogType.Warning, "EconomySystem not assigned!");

        // the try queue method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        // No unit can be spawn, thus should return false.
        Assert.IsFalse(result);
        // No unit deployed
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Likewise for the spawn point, when it is missing, there should we a log warning thrown, and
    // the unit should not be queued, with the economy value being untouched
    [UnityTest]
    public IEnumerator CannotQueue_WhenSpawnPointIsMissing()
    {
        spawner.SetDependenciesForTesting(null, baseHealth, economySystem);

        LogAssert.Expect(LogType.Warning, "Spawn point not assigned!");

        // the try queue method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now where there is no era defined, it means there would also be no units found, thus we make sure
    // the log warning is thrown, and no additional unit is queued and gold does not change
    [UnityTest]
    public IEnumerator CannotQueue_WhenEraListIsEmpty()
    {
        spawner.SetPlayerErasForTesting(new PlayerEraData[] { });

        LogAssert.Expect(LogType.Warning, "No player eras assigned!");

        // the try queue method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now in this case, Lets say somehow the button on click is giving an index array value that is out
    // of bounds of the PlayerUnitData array, in which we should not allow any unit to be queued, no transactions
    // should be made and a log warning should be throwned.
    [UnityTest]
    public IEnumerator CannotQueue_WhenUnitIndexIsInvalid()
    {
        LogAssert.Expect(LogType.Warning, "Invalid unit index requested by the UI button!");

        bool result = spawner.TryQueueToSpawnUnit(99);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now we simulate a scenario where there is an era and unit defined, but there is no prefab object being
    // assigned to said PlayerUnitData, in which we should thorw a log warning error, and do not queue a unit, with
    // gold being untouched, since there is no "unit" object to even spawn out.
    [UnityTest]
    public IEnumerator CannotQueue_WhenUnitPrefabIsMissing()
    {
        // Creating the broken player unit and era to simulate the prefab not being attached scenario
        PlayerUnitData brokenUnit = new PlayerUnitData
        {
            unitName = "Broken",
            prefab = null,
            goldCost = 10,
            trainTime = 0.3f
        };

        PlayerEraData brokenEra = new PlayerEraData
        {
            eraName = "Broken Era",
            units = new PlayerUnitData[] { brokenUnit }
        };

        spawner.SetPlayerErasForTesting(new PlayerEraData[] { brokenEra });

        LogAssert.Expect(LogType.Warning, "Selected unit data or prefab is missing!");

        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now we are doing some integration slight integration testing, in which if we do not
    // have enough money to spend on a unit we want to summon, then the unit should not be spawned
    // and the economy gold amount should remain unchanged
    [UnityTest]
    public IEnumerator CannotQueue_WhenGoldIsInsufficient()
    {
        economySystem.SetResourcesForTesting(5, 0);

        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(5, economySystem.Gold);
    }

    // Now we test the opposite when we do have enough gold to make the purchase, in which we 
    // test to make sure that the unit is created and spawned successfully, and the gold amount has been deducted
    // accordingly
    // We change this method slidly so that while gold is deducted, the unit does not spoawn until the training
    // time of the unit we want to spawn elapses. As such, we need to add an additional yield with time pass
    // to reflect that change.
    [UnityTest]
    public IEnumerator SuccessfulSpawn_ReturnsTrueAndDeductsGold()
    {
        bool result = spawner.TryQueueToSpawnUnit(0);

        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(90, economySystem.Gold);
        // Unit is not spawn yet, currently under training
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Wait out the trainTime so the queue actually spawns the unit, since era 1 small unit take 0.3f, 0.4f
        // is sufficient wait time.
        yield return new WaitForSeconds(0.4f);
 
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Now as an extension, I need to do the same test, but I want to ensure that the spawned unit is actually
    // coming out from the spawn point location that has been defined above.
    // Likewise, need to add wait time to show that the unit has been trained finished to be spawned.
    [UnityTest]
    public IEnumerator SuccessfulSpawn_CreatesUnitAtSpawnPoint()
    {
        bool result = spawner.TryQueueToSpawnUnit(0);

        // Wait out the trainTime so the queue actually spawns the unit, since era 1 small unit take 0.3f, 0.4f
        // is sufficient wait time for the object to instantiate.
        yield return new WaitForSeconds(0.4f);

        GameObject spawnedUnit = GameObject.Find("Era1SmallUnit(Clone)");

        Assert.IsTrue(result);
        Assert.IsNotNull(spawnedUnit);
        Assert.AreEqual(spawnPoint.position, spawnedUnit.transform.position);
    }

    // Testing if the spawner system works correctly where diffrent index value being given in by the button
    // will spawn the corret unit based on the array, in this case, if I want to spawn a medium unit, its array
    // index value should be 1, costing 25 gold to purchase.
    // This means that there should be 100 - 25 = 75 gold remaining and only the mediumUnit for era 1 should have
    // a clone being produced, and no small unit era 1 clone should be observed.
    // Change the yeild to include wait time, as 
    // Medium unit's trainTime is 0.8f, so wait for that before checking clone count.
    [UnityTest]
    public IEnumerator SpawnUsesDifferentUnitData_ByIndex()
    {
        bool result = spawner.TryQueueToSpawnUnit(1);

        // Need to wait 0.9f to make sure that we clearn the medium unit's 0.8f trainTime.
        yield return new WaitForSeconds(0.9f);

        Assert.IsTrue(result);
        Assert.AreEqual(75, economySystem.Gold);
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Change this checking from blocking second unit deployment to queuing second unit deployment
    // This helps follows the queue model, where a second click while the first is still training is no longer
    // rejected, but rather accepted, paid for, and queued behind the first. 
    // This test is set up to show that queuing behaviour instead of the old blocking behaviour.
    [UnityTest]
    public IEnumerator SmallUnitTrainTime_QueuesSecondUnitInsteadOfBlocking()
    {
        // We do a double click, where the unit to spawn now is 2 small era 1 units.
        bool firstResult = spawner.TryQueueToSpawnUnit(0);
        bool secondResult = spawner.TryQueueToSpawnUnit(0);
 
        yield return null;
 
        // Both clicks succeed immediately - queuing never rejects on the second click of the unit
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
 
        // Gold for BOTH units is deducted right away, even though only the first is training.
        Assert.AreEqual(80, economySystem.Gold);
 
        // Neither has actually spawned into the "world" yet.
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Wait long enough for one units to finish training 0.3f.
        yield return new WaitForSeconds(0.4f);

        // One unit has spawn into the "world"
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Wait long enough for both units to finish training back-to-back, a 0.3f + 0.3f = 0.6f.
        yield return new WaitForSeconds(0.7f);
 
        // Both unit has spawn into the "world"
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Now we try and test the mechnic where we wait for the unit to spawn after the first click, before
    // creating the second click on the same unit to be trained. So what we want is to have a 
    // click -> start training -> finish training -> deployed -> click -> instead of queue we start training 
    // again -> finish training -> deployed.
    [UnityTest]
    public IEnumerator SmallUnitTrainTime_AllowsSecondSpawnAfterFirstFinishes()
    {
        bool firstResult = spawner.TryQueueToSpawnUnit(0);
 
        // first unit's 0.3f trainTime elapses
        yield return new WaitForSeconds(0.4f);
 
        // Unit should be created into the "world"
        Assert.IsTrue(firstResult);
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(90, economySystem.Gold);
 
        // nothing is currently being trained, so the training starts immediately for the requested unit
        bool secondResult = spawner.TryQueueToSpawnUnit(0);

        // second unit's 0.3f trainTime elapses
        yield return new WaitForSeconds(0.4f);
 
        // All training should pass, and the amount spend should be deducted accordingly
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(80, economySystem.Gold);
    }

    // Using the queue idea, I want to ensure that if we start a unit to traing with a longer queueing time
    // the unit of the longer queuing time is not spawn when the time is not cleared yet, all while ensuring
    // that there smaller unit that we want to train next is allowed to join into the queue until after the
    // medium unit is trained finish, then the smaller unit can start training.
    [UnityTest]
    public IEnumerator MediumUnitTrainTime_QueuesSmallUnitBehindMedium()
    {
        // medium unit with a trainTime of 0.8f
        bool firstResult = spawner.TryQueueToSpawnUnit(1);
 
        // medium unit still mid-training (0.4f of 0.8f done)
        yield return new WaitForSeconds(0.4f);
 
        // While medium unit still training, queue up a small unit, which should join behind the medium unit being
        // trained as the next unit to be trained.
        bool secondResult = spawner.TryQueueToSpawnUnit(0);
 
        yield return null;
 
        // Both calls should be true since we are able to train the medium unit while allowing the small unit
        // to join the queue
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
 
        // Medium hasn't finished training yet, so neither unit has spawned into the world.
        Assert.AreEqual(0, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
 
        // Both costs are deducted up front: 100 - 25 (medium) - 10 (small) = 65
        Assert.AreEqual(65, economySystem.Gold);

        // Wait for medium's remaining ~0.4f, while ensuring that the 
        // small's full 0.3f trainTime has not passed yet.
        // thus with 0.4f + 0.5f we get 0.9f > 0.8f medium unit training time
        yield return new WaitForSeconds(0.5f);

        // Medium unit created, small unit not yet created
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
 
        // Wait for small's units remaining 0.2f of trainTime left.
        // In this case, I set the wait time to 0.3f to give some buffer for the unit to spawn properly
        yield return new WaitForSeconds(0.3f);
 
        // Now there should be one small and medium unit that have been spawned successfully
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Change the method to make it more understandable on what we are trying to test here.
    // Now we create a scenario where the medium unit is spawned, but enough time has passed to allow the
    // instant training of the small unit, so there should be one medium and small unit spawned at the end,
    // where 10 + 25 = 35 gold being deducted from the 100 base starting gold, which is 65 gold remaining
    [UnityTest]
    public IEnumerator MediumUnitTrainTime_AllowsSmallUnitToStartAfterMediumFinishes()
    {
        // medium, trainTime 0.8f
        bool firstResult = spawner.TryQueueToSpawnUnit(1);
 
        // medium finishes and spawns
        yield return new WaitForSeconds(0.9f);
 
        // Only medium unit spawned, and priced paid accordingly
        Assert.IsTrue(firstResult);
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(75, economySystem.Gold);
 
        // Nothing is currently being trained, so small unit starts training immediately
        bool secondResult = spawner.TryQueueToSpawnUnit(0);
 
        // small's 0.3f trainTime elapses
        yield return new WaitForSeconds(0.4f);
 
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(65, economySystem.Gold);
    }

    // Now we try to change the era from the spawnersystem, so since we are using array indexing, the
    // original era 1 is at position 0, thus moving to the new era we are giving the value one as the era index
    // value.
    // Note that we also want to check that the era name and index matches.
    [UnityTest]
    public IEnumerator TrySetEra_ChangesCurrentEraIndex()
    {
        bool result = spawner.TrySetEra(1);

        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(1, spawner.GetCurrentEraIndex());
        Assert.AreEqual("Test Era 2", spawner.GetCurrentEraName());
    }

    // Creating a scenaario where the era index we are looking for does not exist, i.e. unablle to go the 
    // the next era cuz its not there / defined or there is just no more eras to go, in which we expect a log
    // warning and no changes to the era values.
    [UnityTest]
    public IEnumerator TrySetEra_ReturnsFalse_WhenEraIndexInvalid()
    {
        LogAssert.Expect(LogType.Warning, "Invalid era index!");

        bool result = spawner.TrySetEra(99);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, spawner.GetCurrentEraIndex());
        Assert.AreEqual("Test Era 1", spawner.GetCurrentEraName());
    }

    // Now combining a bit, where we will try and change the era, then spawn the second era unit, in which
    // the second era unit (small) cost, is 20 gold, should be spent.
    // So only the second era unit clone should be created, no 1 era units clone should be found, and 
    // gold is changed according to the second era price.
    // Note that accomodate for the train time, we added wait for era 2 small unit 0.5f trainTime before 
    // checking clone count, if the unit has been successfully been instantiated.
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesEra2SmallUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TryQueueToSpawnUnit(0);

        // Wait for era 2 small unit 0.5f trainTime.
        yield return new WaitForSeconds(0.6f);

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2SmallUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Era 2 small costs 20 gold, so 100 - 20 = 80
        Assert.AreEqual(80, economySystem.Gold);
    }

    // Now we try this with a medium unit
    // Note that accomodate for the train time, we added wait for era 2 medium unit 1.0f trainTime before 
    // checking clone count, if the unit has been successfully been instantiated.
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesCorrectEra2MediumUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TryQueueToSpawnUnit(1);

        // Wait for era 2 small unit 1.0f trainTime.
        yield return new WaitForSeconds(1.1f);

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1MediumUnit(Clone)"));

        // Era 2 medium costs 40 gold, so 100 - 40 = 60
        Assert.AreEqual(60, economySystem.Gold);
    }

    // Likewise with a large unit.
    // Note that accomodate for the train time, we added wait for era 2 large unit 2.0f trainTime before 
    // checking clone count, if the unit has been successfully been instantiated.
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesCorrectEra2LargeUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TryQueueToSpawnUnit(2);

        // Wait for era 2 small unit 2.0f trainTime.
        yield return new WaitForSeconds(2.1f);

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2LargeUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1LargeUnit(Clone)"));

        // Era 2 large costs 75 gold, so 100 - 75 = 25
        // Note however since we set a passive income gold to take place
        Assert.AreEqual(25, economySystem.Gold);
    }

    // Just a confirmation that the era spawned clone unit name is correct.
    // This was more for internal testing to let me know what is the find object name I should use
    // to check if the unit "clone" name follows a certain convention.
    // Note of change, where we added the wait for trainTime before searching for the object.
    [UnityTest]
    public IEnumerator SpawnCloneNamingConvention_IsCorrect()
    {
        bool result = spawner.TryQueueToSpawnUnit(0);

        // Wait for the 0.3f trainTime since we trying to spawn small unit before the clone exists.
        yield return new WaitForSeconds(0.4f);

        GameObject spawnedUnit = GameObject.Find("Era1SmallUnit(Clone)");

        Assert.IsTrue(result);
        Assert.IsNotNull(spawnedUnit);
        Assert.AreEqual("Era1SmallUnit(Clone)", spawnedUnit.name);
    }

    // Lastly, we try to test a scenario wher ewe summon an old era unit, upgrade the era, then try and
    // spawn the next era unit after enough time has passed.
    // In which all should work where there is one era 1 small unit, and one era 2 small unit, and we are
    // currently on era 2.
    // We make some changes to factor in the training time, i.e we had to restructured waits, whereby:
    // era 1 small must finish (0.3f) before era 2 small is queued,
    // and era 2 small then needs its own 0.5f to finish. 
    // Era switching should NOT affect a unit that is already mid-training, 
    // since UnitProductionQueue holds a direct PlayerUnitData reference, not an era index.
    [UnityTest]
    public IEnumerator EraSwitch_DoesNotRenameOldSpawnedUnits()
    {
        // training time is 0.3f
        bool firstSpawn = spawner.TryQueueToSpawnUnit(0);
        // After that we swap eras
        bool eraChanged = spawner.TrySetEra(1);

        // We let the time pass where we spawn era 1 small unit, and we are in a new era
        yield return new WaitForSeconds(0.4f);

        // By now the first era small unit should been spawned
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Then we queue the second era small unit
        bool secondSpawn = spawner.TryQueueToSpawnUnit(0);

        // Now we wait for era 2 small unit own trainTime (0.5f).
        yield return new WaitForSeconds(0.6f);

        Assert.IsTrue(firstSpawn);
        Assert.IsTrue(eraChanged);
        Assert.IsTrue(secondSpawn);

        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era2SmallUnit(Clone)"));
        Assert.AreEqual("Test Era 2", spawner.GetCurrentEraName());

        // Era 1 small unit cost 10 gold, while Era 2 small unit cost 20 gold
        // so there sould be 70 gold left.
        Assert.AreEqual(70, economySystem.Gold);
    }

    // This is just to test that even after immediate switch of eras, the previous unit that we were training
    // and in the queue from a previous era does not get discarded.
    [UnityTest]
    public IEnumerator EraSwitch_UnitsAreRegisteredCorrectlyTiedByEraNotByTime()
    {
        // training time is 0.3f
        bool firstSpawn = spawner.TryQueueToSpawnUnit(0);
        // training time is 0.3f (forces the second spawn to join the queue)
        bool secondSpawn = spawner.TryQueueToSpawnUnit(0);
        // After that we swap eras
        bool eraChanged = spawner.TrySetEra(1);
        // Then we queue the second era small unit
        bool thirdSpawn = spawner.TryQueueToSpawnUnit(0);

        // We let the time pass where we spawn era 1 small unit, and we are in a new era
        yield return new WaitForSeconds(0.4f);

        // All should be true since era should have changed, both era small units should have been able
        // to join the queue to start training
        Assert.IsTrue(firstSpawn);
        Assert.IsTrue(eraChanged);
        Assert.IsTrue(secondSpawn);
        Assert.IsTrue(thirdSpawn);
        // Era changed successfully
        Assert.AreEqual("Test Era 2", spawner.GetCurrentEraName());
        // Era 1 small unit cost 10 gold, while Era 2 small unit cost 20 gold
        // so there sould be 60 gold left. (2 * 10 + 1 * 20)
        // Payment for both should be made already
        Assert.AreEqual(60, economySystem.Gold);

        // By now the first era small unit should been spawned
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        // Second era small unit should not have "spawned" yet.
        Assert.AreEqual(0, CountObjectsByName("Era2SmallUnit(Clone)"));

        // Now we wait for era 1 small unit (second unit) own trainTime (0.3f) to pass.
        yield return new WaitForSeconds(0.4f);

        // By now both the first era small unit should been spawned
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
        // Second era small unit should not have "spawned" yet.
        Assert.AreEqual(0, CountObjectsByName("Era2SmallUnit(Clone)"));

        // Now we wait for era 2 small unit own trainTime (0.5f).
        yield return new WaitForSeconds(0.6f);

        // Now both small units from different eras should have been "spawned"
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era2SmallUnit(Clone)"));
    }

    // We are testing that the spawn click of Small -> Medium -> Small should spawn in exactly that order, 
    // proving the queue is a single global FIFO line and NOT grouped/batched by unit type.
    [UnityTest]
    public IEnumerator TestingQueue_ProcessesMixedUnitTypesInStrictClickOrder()
    {
        // small (0.3f) - starts training immediately
        spawner.TryQueueToSpawnUnit(0);
        // medium (0.8f) - queued behind small
        spawner.TryQueueToSpawnUnit(1);
        // small (0.3f) - queued behind medium
        spawner.TryQueueToSpawnUnit(0);

        yield return null;
 
        // Nothing has spawned yet - only the very first click has started training.
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1MediumUnit(Clone)"));

        // Payment for all of them should be made already
        // 100 - 10 - 25 - 10 = 55
        Assert.AreEqual(55, economySystem.Gold);
 
        // first small unit finishes training
        yield return new WaitForSeconds(0.4f);
 
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1MediumUnit(Clone)"));
 
        // medium unit finishes training next, second small has not completed yet
        yield return new WaitForSeconds(0.9f);
 
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
 
        // second small unit should have finish by then
        yield return new WaitForSeconds(0.4f);
 
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
    }

    // Validates the data the UI badges ("+2", "+3", etc.) would showcase from: how many of each
    // unit type are currently sitting in the queue, not including whichever one is actively training.
    [UnityTest]
    public IEnumerator GetPendingCount_ReflectsQueueDepthPerUnitType()
    {
        // small - starts training immediately, pending count = 0
        spawner.TryQueueToSpawnUnit(0);
        // second small queued behind, pending count = 1
        spawner.TryQueueToSpawnUnit(0);
        // medium queued, pending count = 1 
        spawner.TryQueueToSpawnUnit(1);
 
        yield return null;
 
        // Now based on the unit index, we try as see how many of the units are in the queue currently
        Assert.AreEqual(1, spawner.GetPendingCount(0));
        Assert.AreEqual(1, spawner.GetPendingCount(1));
        Assert.AreEqual(0, spawner.GetPendingCount(2));
 
        // Once the first small finishes, its pending count should drop to 1.
        // Since the next unit to join the queue is a small unit as well.
        yield return new WaitForSeconds(0.4f);
 
        Assert.AreEqual(0, spawner.GetPendingCount(0));
        Assert.AreEqual(1, spawner.GetPendingCount(1));
        Assert.AreEqual(0, spawner.GetPendingCount(2));
    }
}
