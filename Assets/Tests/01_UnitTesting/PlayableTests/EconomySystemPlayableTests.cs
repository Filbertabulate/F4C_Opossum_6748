using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// I had created this file since I want to test the passive income generation still works, which can only be
// done in playmode since we need to time to tick to update the income / exp change

public class EconomySystemPlayableTests
{
    // Since this economy System is technically considered as a singleton, in the sense that
    // there should only be one instance of it throughout the game, where there isn't a need to destory
    // the "economy" object throught the game, we can use this test set up in the editor mode.

    // Since this is a unit test, we can just set up this test class such that the economy system
    // is refering to economyObject and economy that is set up correctly for each test.
    private GameObject economyObject;
    private EconomySystem economy;

    // So using the [SetUp] attribute, we can set up the economy system before each test is run.
    [SetUp]
    public void Setup()
    {
        economyObject = new GameObject("Test Economy System");
        economy = economyObject.AddComponent<EconomySystem>();

        // The economy values will be set to 50 Gold and 0 EXP as its initial state.
        economy.SetResourcesForTesting(50, 0);

        // Set up the passiveGold gain as well as the exp gain to be 5 and 1 respectively, like the game
        // default settings, just that the interval is set to be 0.1f this time to make the test "faster".
        economy.passiveGoldAmount = 5;
        economy.goldInterval = 0.1f;

        economy.passiveExpAmount = 1;
        economy.expInterval = 0.1f;
    }

    // After each test, we can use the [TearDown] attribute to clean up the economy system after each test
    // has finished executing. This is important to ensure that each test starts with a clean state and does not 
    // interfere with other tests.
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(economyObject);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.

    // Now I want to do unity test, where it just involves time tick on passive income.
    [UnityTest]
    public IEnumerator PassiveGold_IncreasesAfterInterval()
    {        
        // Need to yield to allow unity to process the change in passive gold
        // Use yield to skip a frame.
        // Set the wait time to increase by 0.15f to ensure that we are at a "frame" where the first increase
        // has happened, but not yet the second one.
        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(55, economy.Gold);
    }

    // Same test but for exp this time
    [UnityTest]
    public IEnumerator PassiveExp_IncreasesAfterInterval()
    {
        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(1, economy.Exp);
    }

    // We do a testing solt where the gold and exp "ticks" have not been met yet, so the passive income for
    // gold and exp is not given yet.
    [UnityTest]
    public IEnumerator PassiveGold_DoesNotIncreaseBeforeInterval()
    {
        yield return new WaitForSeconds(0.05f);

        Assert.AreEqual(50, economy.Gold);
    }

    [UnityTest]
    public IEnumerator PassiveExp_DoesNotIncreaseBeforeInterval()
    {
        yield return new WaitForSeconds(0.05f);

        Assert.AreEqual(0, economy.Exp);
    }
}
