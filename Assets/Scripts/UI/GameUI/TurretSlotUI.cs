using UnityEngine;
using UnityEngine.UI;

// To allow build the turret slot holder, and the turret itslef
public class TurretSlotUI : MonoBehaviour
{
    // Get the economy and exp value counter, which is in now in the economy system script, to get
    // - currentMoney
    // - currentExp
    // - UpdateEconomyUI()
    [Header("Economy")]
    public EconomySystem economySystem;

    // The objects to build, in this case is the turret holder and the turret prefab to spawn at
    // that location
    [Header("Slot Objects")]
    public GameObject scaffolding;
    public Transform turretSpawnPoint;

    // Referece the buttons that we will click to buy the turret and build the turret holder
    [Header("Buttons")]
    public Button unlockButton;
    public Button buildButton;
    // For future build, if there is more than one turret that we can select, for now just one only
    // public GameObject turretSelectionPanel;

    // Current temp cost for the building of the turrret holder (100 exp) and the turrent cost,
    // which in the case is 150 gold
    [Header("Costs")]
    public int unlockExpCost = 100;
    public int turretGoldCost = 150;

    // Next step idea will use this concept:
    // Index 0 -> Crossbow
    // Index 1 -> Cannon
    // Index 2 -> Mage
    // The UI buttons simply call:
    // BuildTurret(0)
    // BuildTurret(1)
    // BuildTurret(2)
    // To get the all the possible turret objects
    // public GameObject[] turretPrefabs;
    // to get all each turret cost
    // public int[] turretGoldCosts;

    // If not right now we just define the turrent we want to build
    [Header("Turret")]
    public GameObject turretPrefab;

    // To help referece the next turret holder that we can build since we "unlocked" it
    [Header("Next Holder")]
    public TurretSlotUI nextHolder;

    // State tracking for the button showing or not, set to false first, since it not been build at all
    private bool isUnlocked = false;
    private bool hasTurret = false;

    // At start, ensure that the turret holder is not shown, likewise for the unlock and build buttons
    private void Start()
    {
        scaffolding.SetActive(false);
        
        // This one need to manually confiugre since we trying to use this script on multiple holders,
        // so it not a one size fit all approach in this case
        // unlockButton.gameObject.SetActive(false);
        buildButton.gameObject.SetActive(false);

        /* In the future when we give turret selection options
        // Turret menu hidden.
        if (turretSelectionPanel != null)
            turretSelectionPanel.SetActive(false);
        */

        // Only the first holder should be manually shown in Inspector
        // OR call ShowUnlockButton() from another script.
    }

    private void Update()
    {
        // If there is no ecomomy system, we cannot do anything, so we return
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return;
        }

        // Light up the unlock button when enough EXP.
        if (unlockButton.gameObject.activeSelf)
        {
            unlockButton.interactable = economySystem.CanAffordExp(unlockExpCost);
        }

        // Light up the build button when there holder is unlocked, there is no turrent placed, 
        // and we have enough money
        if (buildButton.gameObject.activeSelf)
        {
            buildButton.interactable = isUnlocked && !hasTurret && economySystem.CanAffordGold(turretGoldCost);
        }
    }

    // Once we unlocked the holder, we should
    public void ShowUnlockButton()
    {
        // If the turrent is alr unlocked, we dont show the unlock button to be active anymore
        if (isUnlocked) 
        {
            return;
        }

        // Else the unlock button should be shown, and the build button should not be shown
        unlockButton.gameObject.SetActive(true);
        buildButton.gameObject.SetActive(false);
    }

    public void UnlockHolder()
    {
        // If the holder is already unlock, we end this function
        if (isUnlocked)
        {
            return;
        }

        // If there is no ecomomy system, we cannot do anything, so we return
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return;
        }

        // Else for debug purpose, we try to buy the holder when we not enough exp to do so,
        // dont allow the "purchase" to happen
        // Note that this method if the player can afford the turret holder, it will automatically deduct the cost
        // from the player's exp.
        if (!economySystem.TrySpendExp(unlockExpCost))
        {
            // If the purchase fails, the method TrySpendExp will return false, 
            // and we can log a message to the console indicating that the player cannot afford the turret holder.
            Debug.Log("Not enough EXP to unlock holder!");
            return;
        }

        // Then we set the unlocked turret holder to be true
        isUnlocked = true;

        // activate the turret holder
        scaffolding.SetActive(true);

        // Stop showing the unlock button, and start showing the build turret button
        unlockButton.gameObject.SetActive(false);
        buildButton.gameObject.SetActive(true);

        // If there is a next turret holder, then we show the next turrent holder unlock button
        if (nextHolder != null)
        {
            nextHolder.ShowUnlockButton();
        }

        // For debugging purposes only
        Debug.Log("Turret holder unlocked: " + gameObject.name);
    }

    public void BuildTurret()
    {
        // If we have not unlock the holder or the turret has been build we stop
        if (!isUnlocked || hasTurret)
        {
            return;
        }

        // If there is no ecomomy system, we cannot do anything, so we return
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return;
        }

        // Eror catching, just it case it breaks
        if (turretPrefab == null || turretSpawnPoint == null)
        {
            Debug.LogWarning("Turret prefab or spawn point not assigned!");
            return;
        }

        // We ensure that when we want to build the turrent, we have enough money, else dont build
        // log that we dont have enough cash
        // Note that this method if the player can afford the turret, it will automatically deduct the cost from 
        // the player's gold.
        if (!economySystem.TrySpendGold(turretGoldCost))
        {
            // If the purchase fails, the method TrySpendGold will return false, 
            // and we can log a message to the console indicating that the player cannot afford the turret.
            Debug.Log("Not enough gold to build turret!");
            return;
        }

        // Load in the turret that we purchases
        Instantiate(turretPrefab, turretSpawnPoint.position, turretSpawnPoint.rotation);

        // update the tracker to say that we have bought a turret
        hasTurret = true;

        // Stop showing the build button since we already have a turret
        buildButton.gameObject.SetActive(false);

        // For debug purposes
        Debug.Log("Turret built on: " + gameObject.name);
    }

    /* For next step implemention, can ignore first
    // To build the turrent based on the selection panel of turrent choices
    public void OpenTurretSelection()
    {
        if (hasTurret)
            return;

        if (turretSelectionPanel != null)
        {
            turretSelectionPanel.SetActive(true);
        }
    }

    // Likewise for the build turrent method based on the turrent selected
    public void BuildTurret(int turretIndex)
    {
        // Stop if turret is already built
        if (hasTurret)
        {
            return;
        }

        // Stop if holder is not unlocked yet
        if (!isUnlocked)
        {
            return;
        }

        // Safety check
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem not assigned!");
            return;
        }

        // If turret selection is not valid, dont build the turret
        if (turretIndex < 0 || turretIndex >= turretPrefabs.Length)
        {
            return;
        }

        // Check cost array
        if (turretGoldCosts == null || turretIndex >= turretGoldCosts.Length)
        {
            Debug.LogWarning("Turret gold cost not assigned for turret index: " + turretIndex);
            return;
        }

        // Check prefab
        if (turretPrefabs[turretIndex] == null || turretSpawnPoint == null)
        {
            Debug.LogWarning("Turret prefab or spawn point not assigned!");
            return;
        }

        // Obtain the selected turret cost
        int cost = turretGoldCosts[turretIndex];

        // Try to spend gold through the economy system script.
        if (!economySystem.TrySpendGold(cost))
        {
            Debug.Log("Not enough Gold.");
            return;
        }

        // Show out the turret that we had selected
        Instantiate(turretPrefabs[turretIndex], turretSpawnPoint.position, turretSpawnPoint.rotation);

        // Ensure that we track the turret to be already built
        hasTurret = true;

        // Now hide the build turret button since no use for it anymore
        buildButton.gameObject.SetActive(false);

        // Stop showing the turret selection panel
        if (turretSelectionPanel != null)
        {
            turretSelectionPanel.SetActive(false);
        }

        // For debug purposes
        Debug.Log("Turret built on: " + gameObject.name);
    }
    */
}