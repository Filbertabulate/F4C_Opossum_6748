using TMPro;
using UnityEngine;

// Brining over the economy system from the player spawner script, to allow us to update the UI for the economy
// This also allow us to do sepeate testing for the economy system.

public class EconomySystem : MonoBehaviour
{
    // Initial starting values for the player, which is 50 gold and 0 exp.
    [Header("Starting Resources")]
    // The amount of money you start with
    [SerializeField] 
    private int gold = 50;
    // The amount of EXP you start with
    [SerializeField] 
    private int exp = 0;

    // For now we are using passive income, but eventually we will have a more complex economy system.
    [Header("Passive Income: Gold")]
    // How much money you get per tick
    public int passiveGoldAmount = 5;
    // How often you get money (e.g., 1 second)
    public float goldInterval = 1f;
    // A timer to track when to give the next income
    private float goldTimer;

    [Header("Passive Income: EXP")]
    // How much EXP you get per tick
    public int passiveExpAmount = 1;
    // How often you get EXP (e.g., 1 second)
    public float expInterval = 1f;
    // A timer to track when to give the next EXP
    private float expTimer;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;

    // To ensure OOP is kept, I want the gold and exp be still be readable, i.e. read only but not writeable,
    // so i will use the "=>" syntax to allow the getter to be public, but the setter as private.
    public int Gold => gold;
    public int Exp => exp;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial text on the screen at startup
        UpdateEconomyUI();
    }

    // Update is called once per frame
    void Update()
    {
        // For every "tick" of time, as right now we are doing passive gold and exp generation, we will call
        // these methods to do so.
        AddPassiveGold();
        AddPassiveExp();
    }

    // Method to add passive gold based on the gold timer tick value defined earlier.
    private void AddPassiveGold()
    {
        goldTimer += Time.deltaTime;

        if (goldTimer >= goldInterval)
        {
            // Call the method to give the player money based on the passive gold amount defined.
            // Why I created a add gold method is cuz eventually i want to have more compelex economy system, 
            // where enemy unit defeated gives gold and exp as well.
            AddGold(passiveGoldAmount);
            // Reset the gold tracking timer
            goldTimer = 0f;
            // Update the screen text
            UpdateEconomyUI();
        }
    }

    // Method to add passive exp based on the exp timer tick value defined earlier.
    private void AddPassiveExp()
    {
        expTimer += Time.deltaTime;

        if (expTimer >= expInterval)
        {
            // Call the method to give the player money based on the passive exp amount defined.
            // Why I created a add exp method is cuz eventually i want to have more compelex economy system, 
            // where enemy unit defeated gives gold and exp as well.
            AddExp(passiveExpAmount);
            // Reset the exp tracking timer
            expTimer = 0f;
            // Update the screen text
            UpdateEconomyUI();
        }
    }

    // Setter methods to add more gold
     public void AddGold(int amount)
    {
        // Dont allow negative values to be added, since that is a bug.
        if (amount <= 0)
        {
            return;
        }

        // Add the amount to the current gold and update the UI
        gold += amount;
        UpdateEconomyUI();
    }

    public void AddExp(int amount)
    {
        // Dont allow negative values to be added, since that is a bug.
        if (amount <= 0)
        {
            return;
        }

        // Add the amount to the current exp and update the UI
        exp += amount;
        UpdateEconomyUI();
    }

    // This is for future use, where enemy defeated will give both gold and exp, so we can just call this method
    // instead.
    public void AwardResources(int goldReward, int expReward)
    {
        AddGold(goldReward);
        AddExp(expReward);
    }

    // Method to check if the player has enough gold to spend on a certain item, based on thier cost.
    public bool CanAffordGold(int cost)
    {
        return gold >= cost;
    }

    // Method to check if the player has enough exp to spend on a certain item, based on thier cost.
    public bool CanAffordExp(int cost)
    {
        return exp >= cost;
    }

    // For future use, in case an item needs both gold and exp to be purchased, we need to check to ensure
    // the player has enough of both resources to spend on a certain item, based on thier cost.
    public bool CanAffordResources(int goldCost, int expCost)
    {
        return CanAffordGold(goldCost) && CanAffordExp(expCost);
    }

    // This method is for actual use, where user want to purchase we gold, we check if they can afford it
    // in the first place, where if so, then we update the gold value to show the transcation successful,
    // and return true telling the user he/she can spend and has spent that gold amount or not.
    public bool TrySpendGold(int cost)
    {
        if (!CanAffordGold(cost))
        {
            Debug.Log("Not enough money! Need: " + cost + ", but you only have: " + gold);
            return false;
        }

        gold -= cost;
        UpdateEconomyUI();
        return true;
    }

    // Same thing, but for exp this time.
    public bool TrySpendExp(int cost)
    {
        if (!CanAffordExp(cost))
        {
            Debug.Log("Not enough experience! Need: " + cost + ", but you only have: " + exp);
            return false;
        }

        exp -= cost;
        UpdateEconomyUI();
        return true;
    }

    // Same thing, but for purchasing something that needs both gold and exp.
    public bool TrySpendResources(int goldCost, int expCost)
    {
        if (!CanAffordResources(goldCost, expCost))
        {
            Debug.Log("Not enough resources! Need: " + goldCost + " gold and " + expCost + " experience.");
            return false;
        }

        gold -= goldCost;
        exp -= expCost;

        UpdateEconomyUI();
        return true;
    }

    // A helper method to easily update all UI text whenever the economy changes
    // Make it public since I need to use this update amount tracker via the turrent holder manager script
    public void UpdateEconomyUI()
    {
        if (goldText != null)
        {
            // No Need to show Gold Text anymore
            goldText.text = gold.ToString();
            // goldText.text = "Gold: " + gold.ToString();
        }

        if (expText != null)
        {
            // No need to show "Exp: " text anymore
            expText.text = exp.ToString();
            // expText.text = "Exp: " + exp.ToString();
        }
    }

    // Since the gold and exp value are private, I need to create a public method to set
    // the gold and exp to be accessible for testing purposes, where I can set the gold and exp to whatever
    // value I want to test.
    public void SetResourcesForTesting(int newGold, int newExp)
    {
        gold = Mathf.Max(0, newGold);
        exp = Mathf.Max(0, newExp);

        // Enusre that after each test state, the gold exp and timer are set to 0, just in case.
        goldTimer = 0f;
        expTimer = 0f;

        UpdateEconomyUI();
    }
}
