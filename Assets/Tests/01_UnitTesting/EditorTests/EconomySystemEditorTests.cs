using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EconomySystemEditorTests
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
    }

    // After each test, we can use the [TearDown] attribute to clean up the economy system after each test
    // has finished executing. This is important to ensure that each test starts with a clean state and does not 
    // interfere with other tests.
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(economyObject);
    }

    // Checking the setup for the eceonomy test is correct.
    [Test]
    public void PlayerStartsWithCorrectGold()
    {
        // Gold should be 50.
        Assert.AreEqual(50, economy.Gold);
    }

    [Test]
    public void PlayerStartsWithCorrectExp()
    {
        // Exp should be 0.
        Assert.AreEqual(0, economy.Exp);
    }

    // Testing the AddGold() method in the economySystem class to ensure that it correctly
    // increases the player's gold when called.
    [Test]
    public void AddGold_IncreasesGoldCorrectly()
    {
        economy.AddGold(25);

        Assert.AreEqual(75, economy.Gold);
    }

    // Testing the AddExp() method in the economySystem class to ensure that it correctly
    // increases the player's exp when called.
    [Test]
    public void AddExp_IncreasesExpCorrectly()
    {
        economy.AddExp(10);

        Assert.AreEqual(10, economy.Exp);
    }

    // Testing the TrySpendGold method in the economySystem class to ensure that it correctly
    // spends gold when the player has enough.
    // In this case, we give a price that should be able to spend.
    [Test]
    public void TrySpendGold_WhenEnoughGold_ReturnsTrue()
    {
        bool result = economy.TrySpendGold(30);

        // The result should be true, since the player has enough gold to spend, and that
        // the gold deduction should be successful.
        Assert.IsTrue(result);
        Assert.AreEqual(20, economy.Gold);
    }

    [Test]
    public void TrySpendGold_WhenEnoughGold_GoldIsDeducted()
    {
        bool result = economy.TrySpendGold(30);

        // the gold deduction should be successful.
        Assert.AreEqual(20, economy.Gold);
    }

    // Testing the TrySpendGold method in the economySystem class to ensure that it correctly
    // spends gold when the player has enough.
    // In this case, we give a price that should not be able to spend, thus should return false and
    // no amount of gold should be deducted from the player's gold.
    [Test]
    public void TrySpendGold_WhenInsufficientGold_ReturnsFalse()
    {
        bool result = economy.TrySpendGold(100);

        Assert.IsFalse(result);
    }

    [Test]
    public void TrySpendGold_WhenInsufficientGold_GoldNotDeducted()
    {
        bool result = economy.TrySpendGold(100);

        Assert.AreEqual(50, economy.Gold);
    }

    // Testing that when we spend exp when player has enough exp, we return true, and subsequently
    // we test that the exp spend is decudcted correctly.
    [Test]
    public void TrySpendExp_WhenEnoughExp_ReturnsTrue()
    {
        // Change the setup slightly so that we actually have Exp to spend.
        economy.SetResourcesForTesting(50, 20);

        bool result = economy.TrySpendExp(10);

        Assert.IsTrue(result);
    }

    [Test]
    public void TrySpendExp_WhenEnoughExp_ExpIsDeducted()
    {
        // Change the setup slightly so that we actually have Exp to spend.
        economy.SetResourcesForTesting(50, 20);

        economy.TrySpendExp(10);

        Assert.AreEqual(10, economy.Exp);
    }

    // Now the opposite scenario, where we do not have enough exp to spend
    // This also means that no exp should have been deducted.
    [Test]
    public void TrySpendExp_WhenInsufficientExp_ReturnsFalse()
    {
        bool result = economy.TrySpendExp(10);

        Assert.IsFalse(result);
    }

    [Test]
    public void TrySpendExp_WhenInsufficientExp_ExpNotDeducted()
    {
        // Change the setup slightly so that we track that Exp has not been to spend.
        economy.SetResourcesForTesting(50, 5);

        bool result = economy.TrySpendExp(10);

        Assert.AreEqual(5, economy.Exp);
    }

    // For testing the system of when enemy unit destroyed, the exp and gold gain increase is successful
    [Test]
    public void AwardResources_IncreasesGoldAndExp()
    {
        economy.AwardResources(20, 5);

        Assert.AreEqual(70, economy.Gold);
        Assert.AreEqual(5, economy.Exp);
    }

    // Testing scenario where player has enough gold and exp to make a purchase that does both
    [Test]
    public void CanAffordResources_WhenEnoughGoldAndExp_ReturnsTrue()
    {
        economy.SetResourcesForTesting(100, 50);

        bool result = economy.CanAffordResources(75, 30);

        Assert.IsTrue(result);
    }

    // Test case where player only have enough Exp but not enough gold, should return false
    [Test]
    public void CanAffordResources_WhenInsufficientGold_ReturnsFalse()
    {
        economy.SetResourcesForTesting(50, 50);

        bool result = economy.CanAffordResources(75, 30);

        Assert.IsFalse(result);
    }

    // Test case where player only have enough gold but not enough exp, should return false
    [Test]
    public void CanAffordResources_WhenInsufficientExp_ReturnsFalse()
    {
        economy.SetResourcesForTesting(100, 10);

        bool result = economy.CanAffordResources(80, 30);

        Assert.IsFalse(result);
    }
    
    // ALL in ALL:
    // Note that I do not test the CanAffordExp and CanAffordGold methods since all the other tests
    // are already using this method for validation if the purchase can be done or not.    
}
