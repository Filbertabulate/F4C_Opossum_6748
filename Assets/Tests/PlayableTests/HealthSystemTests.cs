// https://www.youtube.com/watch?v=pr5FBtu5SvQ

// Originally i had planned on putting this in the editor test folder, but since the takeDamge method
// i created does execute destroy(gameobject) when hp is 0, and I want to have a test to ensure that
// 1) The unit health never goes below 0, and
// 2) The unit is destroyed when its hp is 0,
// it makes more sense to place this test in the play mode test folder, 
// since the destroy(gameobject) method is part of the unity engine lifecycle, 
// and will not be executed in edit mode.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HealthSystemTests
{
    // From unity original test template, I will name this test as 
    // Take Damage, then we should reduce health, which in this case
    // I will create a new gameobject for testing, inlcude the health system script, which is the script to
    // maintain the health of the unit/base, be it enemy or ally.
    // As such we want to ensure that the health system take damage work as indended, where of a hp of 100
    // if we take 30 damage, the hp should be reduced to 70.
    
    [Test]
    public void TakeDamage_ReducesHealthPoints()
    {
        GameObject obj = new GameObject("TestUnit");
        HealthSystem health = obj.AddComponent<HealthSystem>();

        health.maxHp = 100;
        health.hp = 100;

        health.TakeDamage(30);

        Assert.AreEqual(70, health.hp);
    }

    // Another sepearte test, but this time we want to ensure that the health system of any unit
    // does not go below 0, where if the unit which we set has 100 health points, but has taken 999 damage,
    // the the actual hp should be 0, not -899. This is to ensure that for the health bar, we do not see a health
    // bar that is "flipped" when we get "negative" hp, which is not possible.

    [Test]
    public void TakeDamage_HealthShouldNotGoBelowZero()
    {
        GameObject obj = new GameObject("TestUnit");
        HealthSystem health = obj.AddComponent<HealthSystem>();

        health.maxHp = 100;
        health.hp = 100;

        health.TakeDamage(999);

        Assert.AreEqual(0, health.hp);
    }

    /*
    Above, I am only uising [Test] methods as they are perfect for checking HP changes as we are just doing simple calculations,
    which is usually instant and synchronous.

    However, checking if an object was destroyed needs Unity internal Engine lifecycle. 
    Based on what I understand, Unity delays object destruction until the very end of the frame loop. 
    Because a standard [Test] cannot pause or yield a frame to let Unity process that cleanup loop, 
    it will always falsely report that the object is still alive. 
    
    As a result, I would need to use [UnityTest] so we can yield return null; 
    and let Unity actually execute the destruction.
    */

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.

    [UnityTest]
    public IEnumerator TakeDamage_DestroyObjectWhenHealthIsZero()
    {
        // In essence this is just to create a temporary game object, with the healthsystem script attached to it.
        GameObject obj = new GameObject("TestUnit");
        HealthSystem health = obj.AddComponent<HealthSystem>();

        // Set up the obj health values.
        health.maxHp = 100;
        health.hp = 100;

        // Take instant death damage, which should trigger the object to be destroyed.
        health.TakeDamage(100);

        // Need to yield to allow unity to process the destruction of the object.
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;

        // the object should no longer exisit
        Assert.IsTrue(obj == null);
    }

    // Additional Test to ensure health system also works for larger health values, i.e. for bases.
    [Test]
    public void BaseHealth_CanTakeDamage()
    {
        GameObject baseObj = new GameObject("PlayerBase");
        HealthSystem health = baseObj.AddComponent<HealthSystem>();

        health.maxHp = 500;
        health.hp = 500;

        health.TakeDamage(150);

        Assert.AreEqual(350, health.hp);
    }

    // Include testing for healing method of health.
    [Test]
    public void HealDamage_IncreasesHealthPoints()
    {
        GameObject obj = new GameObject("TestUnit");
        HealthSystem health = obj.AddComponent<HealthSystem>();

        health.maxHp = 100;
        health.hp = 40;

        health.HealDamage(30);

        Assert.AreEqual(70, health.hp);
    }

    // Test to ensure unit health does not excceed max health when healing, i.e. can only heal to max.
    [Test]
    public void HealDamage_HealthShouldNotGoAboveMaxHealth()
    {
        GameObject obj = new GameObject("TestUnit");
        HealthSystem health = obj.AddComponent<HealthSystem>();

        health.maxHp = 100;
        health.hp = 80;

        health.HealDamage(999);

        Assert.AreEqual(100, health.hp);
    }

    // Test to ensure that if the unit is not healable, then the heal damage method should not work.
    // i.e for bases in this scenario.
    [Test]
    public void HealDamage_DoesNotHeal_WhenCanBeHealedIsFalse()
    {
        GameObject baseObj = new GameObject("PlayerBase");
        HealthSystem health = baseObj.AddComponent<HealthSystem>();

        health.maxHp = 500;
        health.hp = 300;
        health.SetCanBeHealed(false);

        health.HealDamage(100);

        Assert.AreEqual(300, health.hp);
    }
}
