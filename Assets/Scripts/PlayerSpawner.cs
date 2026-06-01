using UnityEngine;
using TMPro; // We need this namespace to talk to TextMeshPro UI elements!

public class PlayerSpawner : MonoBehaviour
{
    // Create a variable that is able to access all possible player units that we can spawn,
    // from a folder declared in the hierarchy called "PlayerUnits", where we can add all the player 
    // unit prefabs that we want to spawn in the game.
    private GameObject[] playerUnits;

    // We also need a variable to store the spawn point of the player units, which is a transform 
    // that we can set in the Unity editor.
    public Transform spawnPoint;

    // Initially did not wanted to get this spawn cooldown, but since there is currently collision if I
    // do spawning of units too fast, a temporary fix is to have a cool down for spawn to prevent units jittering
    // and not being able to move.
    public float spawnCooldown = 0.5f;
    private float spawnCooldownTimer = 0f;

    // Get the player Base HealthBar that can be used later onwards since we want to stop spawnning units
    // if the based is destroyed
    public HealthSystem playerBaseHealth;

    // Since I need to differentiate which unit comes first, I need a tracker to tag each spawned unit
    private long nextSpawnOrder = 0;

    [Header("Economy Settings: Gold")]
    public int currentMoney = 50; // The amount of money you start with
    public int passiveIncomeAmount = 5; // How much money you get per tick
    public float incomeInterval = 1f; // How often you get money (e.g., 1 second)
    private float incomeTimer = 0f;

    [Header("Economy Settings: EXP")]
    public int currentExp = 0; // The amount of EXP you start with
    public int passiveExpAmount = 1; // How much EXP you get per tick
    public float expInterval = 1f; // How often you get EXP
    private float expTimer = 0f;

    [Tooltip("Type the cost of each unit here. Index 0 = Red, Index 1 = Yellow, Index 2 = Green")]
    public int[] unitCosts; 

    [Header("UI References")]
    public TextMeshProUGUI moneyText; // The text on screen showing your money
    public TextMeshProUGUI expText;   // The text on screen showing your EXP

    private void Awake()
    {
        // Load all the player unit prefabs from the Resources/PlayerUnits folder into the playerUnits array.
        // This way we can easily add or remove player unit prefabs from the folder without having to manually 
        // assign them in the Unity editor, which is more convenient and less error-prone.
        playerUnits = Resources.LoadAll<GameObject>("PlayerUnits");
        
        UpdateEconomyUI(); // Set the initial text on the screen at startup
    }

    // Update is called once per frame
    private void Update()
    {
        // If the based of the player (us in this case has been destroyed), stop running the update
        if (playerBaseHealth == null)
        {
            return;
        }
        
        // Continously tick down the cooldown timer in the background so the user can eventually spawn again
        if (spawnCooldownTimer > 0)
        {
            spawnCooldownTimer -= Time.deltaTime;
        }

        // --- NEW: Passive Income Generator (Gold) ---
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= incomeInterval)
        {
            currentMoney += passiveIncomeAmount; // Give the player money
            incomeTimer = 0f; // Reset the timer
            UpdateEconomyUI();  // Update the screen text
        }

        // --- NEW: Passive EXP Generator ---
        expTimer += Time.deltaTime;
        if (expTimer >= expInterval)
        {
            currentExp += passiveExpAmount; // Give the player EXP
            expTimer = 0f; // Reset the timer
            UpdateEconomyUI();  // Update the screen text
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

        // Check 3: Prevent errors if the array is empty or if the button passes a wrong number
        if (playerUnits.Length == 0)
        {
            Debug.LogWarning("No player units assigned to the spawner (Resources Folder in Assets)!");
            return;
        }
        if (unitIndex < 0 || unitIndex >= playerUnits.Length)
        {
            Debug.LogWarning("Invalid unit index requested by the UI button!");
            return;
        }

        // --- NEW: Determine the cost of the requested unit ---
        int cost = 0;
        // Make sure you actually typed a cost into the Unity Inspector for this unit
        if (unitIndex < unitCosts.Length) 
        {
            cost = unitCosts[unitIndex];
        }
        else
        {
            Debug.LogWarning("Warning: You forgot to set the cost for unit " + unitIndex + " in the Inspector!");
        }

        // --- NEW: Check if player can afford it ---
        if (currentMoney >= cost)
        {
            // Deduct the money and update the screen
            currentMoney -= cost;
            UpdateEconomyUI();

            // We use the specific index passed by the UI button instead of Random.Range
            GameObject unitToSpawn = playerUnits[unitIndex];

            // Then we can instantiate the selected unit at the spawn point's position and rotation.
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
        else
        {
            // If they are broke, log it and deny the spawn!
            Debug.Log("Not enough money! Need: " + cost + ", but you only have: " + currentMoney);
        }
    }

    // A helper method to easily update all UI text whenever the economy changes
    private void UpdateEconomyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Gold: " + currentMoney.ToString();
        }

        if (expText != null)
        {
            expText.text = "Exp: " + currentExp.ToString();
        }
    }
}