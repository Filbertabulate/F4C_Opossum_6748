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
    public IEnumerator CannotSpawn_WhenPlayerBaseIsNull()
    {
        spawner.SetDependenciesForTesting(spawnPoint, null, economySystem);

        // Since the base health is gone, the base should be destroyed log should be thrown out
        LogAssert.Expect(LogType.Warning, "Based has been destroyed!");

        // the try spawn method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        // No unit of that name should be create as an object.
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        // Gold should remain unchanged.
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Testing for error catching when the economyUnit is not defined for the spawner system 
    [UnityTest]
    public IEnumerator CannotSpawn_WhenEconomySystemIsMissing()
    {
        spawner.SetDependenciesForTesting(spawnPoint, baseHealth, null);

        LogAssert.Expect(LogType.Warning, "EconomySystem not assigned!");

        // the try spawn method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        // No unit can be spawn, thus should return false.
        Assert.IsFalse(result);
        // No unit deployed
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Likewise for the spawn point, when it is missing, there should we a log warning thrown, and
    // the unit should not be spawned, with the economy value being untouched
    [UnityTest]
    public IEnumerator CannotSpawn_WhenSpawnPointIsMissing()
    {
        spawner.SetDependenciesForTesting(null, baseHealth, economySystem);

        LogAssert.Expect(LogType.Warning, "Spawn point not assigned!");

        // the try spawn method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now where there is no era defined, it means there would also be no units found, thus we make sure
    // the log warning is thrown, and no additional unit is spawned and gold does not change
    [UnityTest]
    public IEnumerator CannotSpawn_WhenEraListIsEmpty()
    {
        spawner.SetPlayerErasForTesting(new PlayerEraData[] { });

        LogAssert.Expect(LogType.Warning, "No player eras assigned!");

        // the try spawn method should return false, as no unit should have spawn
        // in this case, I just arbitrary chose 0 to summon a unit, which is alaways the small unit
        // based on how I would order the PlayerUnitData array.
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now in this case, Lets say somehow the button on click is giving an index array value that is out
    // of bounds of the PlayerUnitData array, in which we should not allow any unit to spawn, no transactions
    // should be made and a log warning should be throwned.
    [UnityTest]
    public IEnumerator CannotSpawn_WhenUnitIndexIsInvalid()
    {
        LogAssert.Expect(LogType.Warning, "Invalid unit index requested by the UI button!");

        bool result = spawner.TrySpawnUnit(99);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now we simulate a scenario where there is an era and unit defined, but there is no prefab object being
    // assigned to said PlayerUnitData, in which we should thorw a log warning error, and not spawn a unit with
    // gold being untouched, since there is no "unit" object to even spawn out.
    [UnityTest]
    public IEnumerator CannotSpawn_WhenUnitPrefabIsMissing()
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

        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(100, economySystem.Gold);
    }

    // Now we are doing some integration slight integration testing, in which if we do not
    // have enough money to spend on a unit we want to summon, then the unit should not be spawned
    // and the economy gold amount should remain unchanged
    [UnityTest]
    public IEnumerator CannotSpawn_WhenGoldIsInsufficient()
    {
        economySystem.SetResourcesForTesting(5, 0);

        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(5, economySystem.Gold);
    }

    // Now we test the opposite when we do have enough gold to make the purchase, in which we 
    // test to make sure that the unit is created and spawned successfully, and the gold amount has been deducted
    // accordingly
    [UnityTest]
    public IEnumerator SuccessfulSpawn_ReturnsTrueAndDeductsGold()
    {
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(90, economySystem.Gold);
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Now as an extension, I need to do the same test, but I want to ensure that the spawned unit is actually
    // coming out from the spawn point location that has been defined above.
    [UnityTest]
    public IEnumerator SuccessfulSpawn_CreatesUnitAtSpawnPoint()
    {
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

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
    [UnityTest]
    public IEnumerator SpawnUsesDifferentUnitData_ByIndex()
    {
        bool result = spawner.TrySpawnUnit(1);

        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(75, economySystem.Gold);
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
    }

    // Now we are trying to test the "cooldown" of the unit, so after the first unit is selected to be
    // spawn, if we try to spawn the second unit right after / at the same time as the first unit, 
    // the second unit should not be created since not enough time has passed since the cooldown timer reset
    // thus only one of the small unit from era 1 should be created, and only 10 gold should be deduced.
    // I.e. we return false on the second result since no unit is allowed to be spawned.
    [UnityTest]
    public IEnumerator SmallUnitTrainTime_BlocksImmediateSecondSpawn()
    {
        bool firstResult = spawner.TrySpawnUnit(0);
        bool secondResult = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
        Assert.AreEqual(1, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(90, economySystem.Gold);
    }

    // Now I will do a test where I will spawn two of the cheap units, in this case, it would be
    // the era 1 small unit, which have a cooldown time / train time of 0.3f as per defined above,
    // so after the first spawn, I will wait / allow time to tick for 0.4f so that the cooldown time 
    // has completed to allow the next unit to be spawned.
    // In this case two units of the Era1SmallUnits should be spawned, where 2 x 10gold amount is spent.
    [UnityTest]
    public IEnumerator SmallUnitTrainTime_AllowsSpawnAfterWaiting()
    {
        bool firstResult = spawner.TrySpawnUnit(0);

        yield return new WaitForSeconds(0.4f);

        bool secondResult = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(2, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(80, economySystem.Gold);
    }

    // Now for testing a scenario where we are spawning a unit that has a longer training / cooldown time
    // i.e. to say we have a a medium unit that take 0.8f to cooldown, by the 0.4f mark, the next deployable
    // unit still cannot be spawnned, thus we need to ensure that only the gold is spent on the medium unit
    // which is 25 gold, and that only the medium unit is spawned, not the small unit
    [UnityTest]
    public IEnumerator MediumUnitTrainTime_BlocksSpawnUntilLongerWait()
    {
        bool firstResult = spawner.TrySpawnUnit(1);

        yield return new WaitForSeconds(0.4f);

        bool secondResult = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
        Assert.AreEqual(1, CountObjectsByName("Era1MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));
        Assert.AreEqual(75, economySystem.Gold);
    }

    // Now we create a scenario where the medium unit is spawned, but enough time has passed to allow the
    // spawning of the small unit, so there should be one medium and small unit spawned
    // as well as the should be 10 + 25 = 35 gold being deducted from the 100 base starting gold, which
    // is 65 gold remaining
    [UnityTest]
    public IEnumerator MediumUnitTrainTime_AllowsSpawnAfterFullWait()
    {
        bool firstResult = spawner.TrySpawnUnit(1);

        yield return new WaitForSeconds(0.9f);

        bool secondResult = spawner.TrySpawnUnit(0);

        yield return null;

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
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesEra2SmallUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TrySpawnUnit(0);

        yield return null;

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2SmallUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1SmallUnit(Clone)"));

        // Era 2 small costs 20 gold, so 100 - 20 = 80
        Assert.AreEqual(80, economySystem.Gold);
    }

    // Now we try this with a medium unit
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesCorrectEra2MediumUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TrySpawnUnit(1);

        yield return null;

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2MediumUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1MediumUnit(Clone)"));

        // Era 2 medium costs 40 gold, so 100 - 40 = 60
        Assert.AreEqual(60, economySystem.Gold);
    }

    // Likewise with a large unit.
    [UnityTest]
    public IEnumerator SpawnAfterEraSwitch_UsesCorrectEra2LargeUnit()
    {
        bool eraChanged = spawner.TrySetEra(1);
        bool spawnResult = spawner.TrySpawnUnit(2);

        yield return null;

        Assert.IsTrue(eraChanged);
        Assert.IsTrue(spawnResult);

        Assert.AreEqual(1, CountObjectsByName("Era2LargeUnit(Clone)"));
        Assert.AreEqual(0, CountObjectsByName("Era1LargeUnit(Clone)"));

        // Era 2 large costs 75 gold, so 100 - 75 = 25
        Assert.AreEqual(25, economySystem.Gold);
    }

    // Just a confirmation that the era spawned clone unit name is correct.
    // This was more for internal testing to let me know what is the find object name I should use
    // to check if the unit "clone" name follows a certain convention.
    [UnityTest]
    public IEnumerator SpawnCloneNamingConvention_IsCorrect()
    {
        bool result = spawner.TrySpawnUnit(0);

        yield return null;

        GameObject spawnedUnit = GameObject.Find("Era1SmallUnit(Clone)");

        Assert.IsTrue(result);
        Assert.IsNotNull(spawnedUnit);
        Assert.AreEqual("Era1SmallUnit(Clone)", spawnedUnit.name);
    }

    // Lastly, we try to test a scenario wher ewe summon an old era unit, upgrade the era, then try and
    // spawn the next era unit after enough time has passed.
    // In which all should work where there is one era 1 small unit, and one era 2 small unit, and we are
    // currently on era 2.
    [UnityTest]
    public IEnumerator EraSwitch_DoesNotRenameOldSpawnedUnits()
    {
        bool firstSpawn = spawner.TrySpawnUnit(0); // Cooldonw / training time is 0.3f
        bool eraChanged = spawner.TrySetEra(1);

        yield return new WaitForSeconds(0.4f);

        bool secondSpawn = spawner.TrySpawnUnit(0);

        yield return null;

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
}
