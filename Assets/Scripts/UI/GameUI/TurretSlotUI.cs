using UnityEngine;
using UnityEngine.UI;

// To allow build the turret slot holder, and the turret itslef
public class TurretSlotUI : MonoBehaviour
{
    // Get the economy and exp value counter, which is in the player spawner script, to get
    // - currentMoney
    // - currentExp
    // - UpdateEconomyUI()
    [Header("Economy")]
    public PlayerSpawner playerSpawner;

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
        // Light up the unlock button when enough EXP.
        if (unlockButton.gameObject.activeSelf)
        {
            unlockButton.interactable = playerSpawner.currentExp >= unlockExpCost;
        }

        // Light up the build button when there holder is unlocked, there is no turrent placed, 
        // and we have enough money
        if (buildButton.gameObject.activeSelf)
        {
            buildButton.interactable = isUnlocked && !hasTurret && playerSpawner.currentMoney >= turretGoldCost;
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

        // Else for debug purpose, we try to buy the holder when we not enough exp to do so,
        // dont allow the "purchase" to happen
        if (playerSpawner.currentExp < unlockExpCost)
        {
            Debug.Log("Not enough EXP to unlock holder!");
            return;
        }

        // Else we spend the EXP required, and update the economy UI tracker
        playerSpawner.currentExp -= unlockExpCost;
        playerSpawner.UpdateEconomyUI();

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

        // Eror catching, just it case it breaks
        if (turretPrefab == null || turretSpawnPoint == null)
        {
            Debug.LogWarning("Turret prefab or spawn point not assigned!");
            return;
        }

        // We ensure that when we want to build the turrent, we have enough money, else dont build
        // log that we dont have enough cash
        if (playerSpawner.currentMoney < turretGoldCost)
        {
            Debug.Log("Not enough gold to build turret!");
            return;
        }

        // Else if we do have enought cash / money, then we pay the cost to build the turret
        playerSpawner.currentMoney -= turretGoldCost;
        playerSpawner.UpdateEconomyUI();

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

        // If turret selection is not valid, dont build the turret
        if (turretIndex < 0 || turretIndex >= turretPrefabs.Length)
        {
            return;
        }

        // Obtain the selected turret cost
        int cost = turretGoldCosts[turretIndex];

        // see if we can purchase that turret based on what we have in gold
        // if cannot, we dont build the turret
        if (playerSpawner.currentMoney < cost)
        {
            Debug.Log("Not enough Gold.");
            return;
        }

        // Else we build the turret and pay the price
        playerSpawner.currentMoney -= cost;
        playerSpawner.UpdateEconomyUI();

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
    }
    */
}