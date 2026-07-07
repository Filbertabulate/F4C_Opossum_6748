using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // Set up the base's health points, where this
    // is the current health of the base/unit
    public int hp;

    // Set up the unit/ base max health points, where this is the maximum health that the base/unit can have.
    public int maxHp = 100;

    [Header("Healing Settings")]
    // Set up a boolean variable to determine if the unit can be healed or not, 
    // where this is useful to tag which units can and cannot be healed, i.e. bases should not be healable.
    [SerializeField] 
    private bool canBeHealed = true;

    [SerializeField]
    // Obtain the HealthBarUI component of the base/unit, so that we can update the health bar UI 
    // when the base/unit takes damage.
    private HealthBarUI healthBar;

    // Initialise the HP of the unit/base to the max HP at the start of the game, 
    // and also set the max health value of the health bar UI to the max HP of the unit/base.
    // I use awake since this thing does not need reference to other game objects.
    private void Awake()
    {
        hp = maxHp;

        if (healthBar != null)
        {
            // This is to set the boundary of the health bar UI
            healthBar.SetMaxHealth(maxHp);
            // This is for the red colour of the health bar to be full at the start of the game.
            healthBar.SetHealth(maxHp);
        }
    }

    // Creating a method called TakeDamage, where if the base takes damage, 
    // it will reduce the hp by the damage amount and print the current hp 
    // to the console. If the hp is less than or equal to 0, 
    // it will print a message saying that the enemy base is destroyed.
    public void TakeDamage(int damage)
    {
        // Reduce the hp by the damage amount and print the current hp to the console.
        hp -= damage;

        // Clamp the health value to ensure it does not go below 0 or above maxHealth, 
        // which can prevent potential bugs and ensure that the health bar behaves as expected.
        hp = Mathf.Clamp(hp, 0, maxHp);

        // For logging purposes. 
        Debug.Log(gameObject.name + " took " + damage + " damage. HP left: " + hp);

        // Update the health bar UI to reflect the current hp of the base/unit.
        if (healthBar != null)
        {
            healthBar.SetHealth(hp);
        }

        // If the unit hp is 0 or less, then we should destroy the unit/base
        // since they have been defeated.
        if (hp <= 0)
        {
            // Since the unit hp is now less that or equal to 0, the unit should be considered as dead.
            Debug.Log(gameObject.name + " destroyed!");
            Destroy(gameObject);
        }
    }

    // The reverse of take Damage, where if there is special effect to do healing, 
    // we can call this method to increase the hp of the unit, only applicable to units, not base.
    public void HealDamage(int healingAmount)
    {
        // If the unit detected cannot be healed, then we debug saying unit cannot be healed, and return.
        // i.e. dont do anything.
        if (!canBeHealed)
        {
            Debug.Log(gameObject.name + " cannot be healed.");
            return;
        }

        // Increase the hp by the healing amount and print the current hp to the console.
        hp += healingAmount;

        // Clamp the health value to ensure it does not go below 0 or above maxHealth, 
        // which can prevent potential bugs and ensure that the health bar behaves as expected.
        hp = Mathf.Clamp(hp, 0, maxHp);

        // For logging purposes. 
        Debug.Log(gameObject.name + " healed for " + healingAmount + ". HP left: " + hp);

        // Update the health bar UI to reflect the current hp of the base/unit.
        if (healthBar != null)
        {
            healthBar.SetHealth(hp);
        }
    }

    // Useful for testing purposes, cuz right now we set canbeHealed to true by default, 
    // but for testing purposes, we want to be able to set it to false.
    public void SetCanBeHealed(bool value)
    {
        canBeHealed = value;
    }
}
