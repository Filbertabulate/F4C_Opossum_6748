// Import systems that I need to use since I am trying to tie in the user button click to a specific turret
// type, as well as dynamically updating the turret gold cost based on user defined values.
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Create a class here that will contain each turrent data, their cost, prefab object
// as well as the turrent name, which includes thier era as well
[System.Serializable]
public class TurretData
{
    [Header("Turret Name")]
    public string turretName;

    [Header("Turret Prefab Object")]
    public GameObject turretPrefab;

    [Header("Turret Gold Cost")]
    public int goldCost;
}

// Since one era may have more that one turrent to choose from, I need to ensure that I set up another
// class such that it is able to contain the turrent(s) for each different era, where the turrent banner
// updates for every era shift
[System.Serializable]
public class TurretEraData
{
    [Header("Era Information")]
    public string eraName;

    [Tooltip("Index 0, 1 and 2 correspond to the three turret buttons shown on the UI slot.")]
    [Header("Turrets")]
    public TurretData[] turrets;

     [Header("Era UI")]
    public Sprite turretBannerSprite;
}

public class TurretManager : MonoBehaviour
{
    // I will create an enumeration datatype name TurretMode, where the goal is
    // to represet at any point of time during the game, what step is the user on in regardings to wanting to
    // build a turrent or not
    // Type 1: None -> The player is not indicating his/her interest into building up the turret Holder or buying/
    // selling the turret
    // Type 2: Build -> The player has already selected a turrent he wants to build from the button UI, and now
    // he is able to click the highlighted slots that are avalaible to build said turrent, if he/she has enough
    // monet
    // Type 3: Sell -> The player now want to sell the turrent he/she has build, where only existing turrent are
    // highlighted annd clikcing on an existing turrent will remove it and return half the amount spend to purchase
    // the turret.
    public enum TurretMode
    {
        None, Build, Sell
    }


    // =============================================================================================
    // Era Data
    // =============================================================================================

    // Create the insepctor for me to fill up all the necessary fields for the turrent based on the
    // current era and its turrents available
    [Header("Era Turret Data")]
    [SerializeField] 
    private TurretEraData[] turretEras;
    [SerializeField] 
    private int currentEraIndex = 0;

    // =============================================================================================
    // Turret Data
    // =============================================================================================

    // Update based on the turrent manager, the max number of turrent the user can deploy the whole game
    // which were we set a saveguard of the min value to be 0, so that we cannot have negative max number
    // of turrent holders as that does not make sense.
    [Header("Holder Configuration")]
    [Min(0)]
    [SerializeField] 
    private int maximumTurretHolders = 2;

    // These are the positions we want to unlock, which we are sorting in order based on the current layout
    // of the castle configurations
    [SerializeField] 
    private TurretSlotUI[] turretSlots;

    // =============================================================================================
    // Turret Holder Upgrade Costs
    // =============================================================================================

    // Right now we set up the holder purchase value, where the first holder price is 100, and for now
    // we configure it such that per each holder purchase, the next holder will always increase by a value of 100
    [Header("Holder Purchase")]
    [Min(0)]
    [SerializeField] 
    private int initialHolderGoldCost = 100;

    [Min(0)]
    [SerializeField] 
    private int holderGoldCostIncrease = 100;

    // =============================================================================================
    // Other manager dependencies
    // =============================================================================================

    // For this script to pull the current gold and exp count the player currently has
    [Header("Other Manager Dependencies")]
    [SerializeField] 
    private EconomySystem economySystem;

    // =============================================================================================
    // Turret UI toobar Buttons
    // =============================================================================================

    // Button access to know which button the press as well as to update the button gold cost text
    // according to the era
    [Header("Turret Toolbar Buttons")]
    [SerializeField] 
    private Button turretButton0;
    [SerializeField] 
    private Button turretButton1;
    [SerializeField] 
    private Button turretButton2;

    [SerializeField] 
    private Button unlockHolderButton;
    [SerializeField] 
    private Button sellTurretButton;

    // =============================================================================================
    // Turret UI toobar Text
    // =============================================================================================

    [Header("Toolbar Cost Text")]
    [SerializeField] 
    private TMP_Text turret0CostText;
    [SerializeField] 
    private TMP_Text turret1CostText;
    [SerializeField] 
    private TMP_Text turret2CostText;
    [SerializeField] 
    private TMP_Text holderCostText;

    // =============================================================================================
    // Resources to Beautify the UI dependencies
    // =============================================================================================

    // This is for updating the turrent banner images availale to the user based on the 
    // current era we are in.
    [Header("Toolbar Artwork (For each Era UI)")]
    [SerializeField] 
    private Image turretEraBannerImage;

    // Button to cancel / exit the mode of building / selling of the turrent so that we can click other buttons
    // like units / specials.
    [Header("Mode Cancellation")]
    [SerializeField]
    private GameObject cancelModeOverlay;

    // =============================================================================================
    // Runtime state variables
    // =============================================================================================

    // This is just to keep track of the number of turret holders that are being unlock, so we do not
    // have the scenario where the same holder is being unlocked multiple times.
    private int unlockedHolderCount;

    // Set the tracker of the currentMode the user is at to be None, since we are starting of not wanting to buy
    // sell any turret
    private TurretMode currentMode = TurretMode.None;
    // To keep track of the selected turrent the user want to buy and play the turret at.
    private TurretData selectedTurret;

    // Keeping track of the index the user had selected from the turret toolbar UI.
    // I will set -1 to mean that no turret is currently selected.
    private int selectedTurretIndex = -1;
    

    // =============================================================================================
    // Public Read-Only access states, mainly used for unit testing
    // =============================================================================================

    // This read only data type is for me to do system testing later on
    public TurretMode CurrentMode => currentMode;
    public TurretData SelectedTurret => selectedTurret;
    public int SelectedTurretIndex => selectedTurretIndex;
    public int CurrentEraIndex => currentEraIndex;
    public int UnlockedHolderCount => unlockedHolderCount;

    // the next holder cost price based on the number of turret holders already unlocked
    public int CurrentHolderGoldCost => initialHolderGoldCost + unlockedHolderCount * holderGoldCostIncrease;

    // Ensures that the game begins in normal turret mode with the blocking overlay hidden.
    private void Awake()
    {
        currentMode = TurretMode.None;
        selectedTurret = null;
        selectedTurretIndex = -1;

        if (cancelModeOverlay != null)
        {
            cancelModeOverlay.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start performs the first complete turret UI refresh.
    private void Start()
    {
        // On start, we need to update the starting era banner and turret prices.
        RefreshEraUI();

        // Likewise, we need to make sure that all the buttons, slots and the cancel overlay is at the correct
        // configurations.
        RefreshAllStates();
    }

    // Update is called once per frame
    void Update()
    {
        // Keep track of every single turret related change and update what to show and what not to show
        // as well as price check accordingly
        RefreshToolbarButtonStates();
    }

    // ============================================================
    // Creating On Click methods for the turret Toolbar UI buttons
    // ============================================================

    // For all the Onclick() methods, I will manaually configue the turretPos button value
    // for each button, where the first button from the left is 0, next is 1 so on and forth
    public void PlayerChosenTurret(int turretPos)
    {
        SelectTurretFromUI(turretPos);
    }

   
    // Based on the button the user has selected, if the turrent meets the critera of:
    // 1) Turret is a valid turret
    // 2) There is an economy system that is already defined to ensure we can maintain and use the
    // current economy state
    // 3) There is a valid open Turrent Holder Slot
    // 4) We can make the purchase of the turrent selected based on the curret amount of money we have
    // Selecting a valid turret enters Build mode.
    // Note: gold is not deducted until the player clicks a valid slot.
    public void SelectTurretFromUI(int turretIndex)
    {
        // Now lets get the turret info that the user is looking for based on the curret era we at in, as well
        // as the turret the user selected for that era.
        TurretData turret = GetCurrentEraTurret(turretIndex);

        if (turret == null || turret.turretPrefab == null)
        {
            // For debugging purposes
            Debug.LogWarning("No valid turret exists at toolbar index" +  turretIndex + ".");

            // Dont allow the turret Cancel Overlay to take place, so we cancel the current mode back to None.
            CancelCurrentMode();
            // Since we cannot buld the turret, we do an early return
            return;
        }

        if (economySystem == null)
        {
            // For debugging purposes
            Debug.LogWarning("EconomySystem is not assigned to TurretManager.");

            // Dont allow the turret Cancel Overlay to take place, so we cancel the current mode back to None.
            CancelCurrentMode();
            // Since we cannot buld the turret, we do an early return
            return;
        }

        if (!HasEmptyUnlockedSlot())
        {
            // For debugging purposes
            Debug.Log("There are no unlocked empty turret holders.");

            // Dont allow the turret Cancel Overlay to take place, so we cancel the current mode back to None.
            CancelCurrentMode();
            // Since we cannot buld the turret, we do an early return
            return;
        }

        if (!economySystem.CanAffordGold(turret.goldCost))
        {
            // For debugging purposes   
            Debug.Log($"Not enough gold to select {turret.turretName}. " + $"Required gold: {turret.goldCost}");

            // Dont allow the turret Cancel Overlay to take place, so we cancel the current mode back to None.
            CancelCurrentMode();
            // Since we cannot buld the turret, we do an early return
            return;
        }

        // Should not be needing this since I will be using the cancel button that will overlap all the other
        // buttons
        /*
        // clicking the currently selected turret button again cancels Build mode.
        if (currentMode == TurretMode.Build && selectedTurretIndex == turretIndex)
        {
            CancelCurrentMode();
            return;
        }
        */

        // We set our turrent manager to be in build mode, i.e. to say we are trying to build the turrent
        // that the user selected, importing the turrent information needed for later when the user selects
        // which slot, if there are multiple, to place the turrent he wants to purchase.
        currentMode = TurretMode.Build;
        selectedTurret = turret;
        selectedTurretIndex = turretIndex;

        // For Debugging purposes
        Debug.Log("Selected " + turret.turretName +  ", now please choose a flashing empty holder.");

        // Update the UI in case we have like price increases later on, just to ensure that like
        // our UI screen shows the correct screen based on stage we are at in the game
        RefreshAllStates();
    }

    // ============================================================
    // Building the turrent based on the user selection of the turret slot
    // ============================================================

    // Based on the placement button the user click, this method will be called, which would help
    // ensure that from this button, we want to build this turrent right here
    public void BuildSelectedTurretAtSlot(TurretSlotUI selectedSlot)
    {
        // If the mode we are at is not build mode or there is no selected turret to build, we cannot build
        // anything, thus return early and revent the mode we are in back to None, original game mode
        if (currentMode != TurretMode.Build || selectedTurret == null)
        {
            CancelCurrentMode();
            return;
        }

        // Likewise if the slot that we have selected is not a correct slot that does not have the correct
        // turretSlotUI script, then we dont have the correct details to put the turrent, thus we cancel the build
        // mode we have and revent back to the original state of the game, no turrent building
        if (selectedSlot == null || !selectedSlot.IsEmpty)
        {
            CancelCurrentMode();
            return;
        }

        // Same thing, if there is no economy state tracking, we dont know if the turret the user selected
        // can he actually pay for it, thus we cannot build a turrent and do a early return
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem is not assigned to TurretManager.");

            CancelCurrentMode();
            return;
        }

        // Obtain the selected turret the user wants to purchase gold cost
        int purchaseCost = selectedTurret.goldCost;

        // Gold is deducted only when the player chooses a valid physical slot.
        // Where if there is not enough gold, we dont make the purchase, cancel the build mode and
        // return back to the game.
        if (!economySystem.TrySpendGold(purchaseCost))
        {
            Debug.Log("Not enough gold to build " + selectedTurret.turretName + ".");

            CancelCurrentMode();
            return;
        }

        // If not we want to do the building of the turrent based on the curret turrent spot, using that
        // current turrent spot TurretSlotUI script to build said turrent at that spot
        bool buildSucceeded = selectedSlot.BuildTurret(selectedTurret.turretPrefab, purchaseCost);

        // If somehow the turrent could not be build on that slot,
        // I will refund the player the turret cost.
        // Should not happened, but this mainly for testing
        if (!buildSucceeded)
        {
            economySystem.AddGold(purchaseCost);
        }

        // One turrent has been placed, we will end the build mode.
        CancelCurrentMode();
    }

    // ============================================================
    // For Building up the turrent holders
    // ============================================================

    // Upon clicking the turrent holder build button, we will purchase and build the next locked physical turret 
    // holder. This is the onClick method for the Purchase Holder button.
    public void PurchaseNextHolder()
    {
        // Purchasing a holder cancels Build or Sell mode.
        CancelCurrentMode();

        // Just ensuring that we can only make the purchase only if we have a valid enconomy system tracker
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem is not assigned to TurretManager.");
            return;
        }

        // Obtain the max number of holders we are allowing the user to build in this game mode.
        int allowedHolderCount = GetAllowedHolderCount();

        // We will stop when every allowed holder is already unlocked
        if (unlockedHolderCount >= allowedHolderCount)
        {
            Debug.Log("The maximum turret holder count has been reached.");
            // This is placed here to update the UI such that the button should no longer be clickable, and that
            // the gold text should show MAX level, not numbers
            RefreshAllStates();
            return;
        }

        // If we did not assign any possible turrent slots, or the turrent slot has reach the max, where
        // it exits pass the max turrent slots from the array, we throw and warning and do an early return
        // since we cannot build any more turret holders.
        if (turretSlots == null || unlockedHolderCount >= turretSlots.Length)
        {
            Debug.LogWarning("The next turret slot does not exist.");

            RefreshAllStates();
            return;
        }

        // If not lets get the turret slot we want to unlock now, which is the based on the current 
        // unlock holderCount of numbers of turrents already unlocked.
        // 0 turrets unlocked, so we unlock turrer slot 0 in the array position
        TurretSlotUI nextSlot = turretSlots[unlockedHolderCount];

        // No valid turret slot / the slot does not have the TurretSlotUI script
        if (nextSlot == null)
        {
            Debug.LogWarning($"Turret slot {unlockedHolderCount} is not assigned.");

            RefreshAllStates();
            return;
        }

        // We get the cost of the curret turrent holder we want to purchase, since we know the nextslot
        // is a valid turrent slot we want to purchase
        int purchaseCost = CurrentHolderGoldCost;

        // Now we try to buy the turrent slot, where if we cannot buy, then no gold will be spent and this
        // Debug log message will be shown.
        if (!economySystem.TrySpendGold(purchaseCost))
        {
            Debug.Log( $"Not enough gold to purchase a holder. " + $"Required gold: {purchaseCost}");

            RefreshToolbarButtonStates();
            return;
        }

        // Now we try to build the turrent hold into the game.
        bool unlockSucceeded = nextSlot.UnlockHolder();

        // Refund the holder cost if the slot could not be unlocked.
        // Should not happen but just in case.
        if (!unlockSucceeded)
        {
            economySystem.AddGold(purchaseCost);

            RefreshAllStates();
            return;
        }

        // Since the holder was build successfully, we now need to increment the unlocked holder count
        // to show that the turrent holder has been unlocked successfully, and we know what how many turrent
        // holders has been unlocked thus far.
        unlockedHolderCount++;

        // For logging purposes
        Debug.Log($"Purchased holder for {purchaseCost} gold. " + $"Unlocked holders: " + 
                  $"{unlockedHolderCount}/{allowedHolderCount}");

        // This is needed since I want to ensure that if we reach pass the max holder count from the array/defined
        // value, then I want to show the text as Max cost, and no longer want the turret holder slot button to
        // be clicked anymore.
        RefreshAllStates();
    }

    // ============================================================
    // Selling of the turrents that has been placed
    // ============================================================

    // We want to enter the sell mode, and get a refuned of 1/2 price of the turret cost,
    // as well as freeing up space to build another turret based on the user wish.
    // This will be assigned to the Sell toolbar button's On Click() event.
    public void SelectSellMode()
    {
        // If not turrets has been build, we cannot sell anything, thus we return the log the attempt and do nothing
        if (!HasAnyBuiltTurret())
        {
            Debug.Log("There are no turrets available to sell.");

            CancelCurrentMode();
            return;
        }

        // Else if there is a turrent we can sell, we change the mode to sell mode, turrent we want to sell
        // to be none first, with no turrent selected as the index value
        currentMode = TurretMode.Sell;
        selectedTurret = null;
        selectedTurretIndex = -1;

        // For logging purposes
        Debug.Log("Sell mode selected. " + "Choose a flashing Sell button.");

        // Update the UI to show up the cancel overlay and the sell buttons
        RefreshAllStates();
    }

    /// Called by the Sell button belonging to a physical slot, which glows up when in sell mode
    public void SellTurretFromSlot(TurretSlotUI selectedSlot)
    {
        // If there is no economySystem, selling of turrents not allowed.
        if (economySystem == null)
        {
            Debug.LogWarning("EconomySystem is not assigned to TurretManager.");

            CancelCurrentMode();
            return;
        }
        
        // Safety check to ensure we can only sell the turret if we are in sell mode
        if (currentMode != TurretMode.Sell)
        {
            CancelCurrentMode();
            return;
        }

        // If the slot we selected does not exisit, or does not have a turret, then we cannot do any
        // selling and early return
        if (selectedSlot == null || !selectedSlot.HasTurret)
        {
            CancelCurrentMode();
            return;
        }

        // else there is a turrent at the curret slot they selected, thus we run the sellTurret method
        // from that slot TurretSlotUI script to get a 50% of the turret's original purchase price.
        // for now we set it up such that 0.5f = 50% is the default value, thus we dont give any value inside
        // for now
        int refundAmount = selectedSlot.SellTurret();

        // Add refund money only if it is positive, which should be, just this placed as a precaution.
        if (refundAmount > 0)
        {
            economySystem.AddGold(refundAmount);

            Debug.Log( $"Received {refundAmount} gold " +"from selling a turret.");
        }

        // Selling one turret ends Sell mode.
        CancelCurrentMode();
    }

    // ============================================================
    // Turret Mode Management
    // ============================================================

    // We will cancel the Build or Sell mode, that we are in, if we are, and revent back to normal, where
    // no turret is selected, and the mode for the turrent manager is set to None.
    // This also helps to keep track if the cancel overlay should be shown or not.
    public void CancelCurrentMode()
    {
        currentMode = TurretMode.None;
        selectedTurret = null;
        selectedTurretIndex = -1;

        RefreshAllStates();
    }

    // Clears the current mode without immediately refreshing.
    // This is used when another method will perform a complete
    // refresh immediately afterwards, such as TrySetEra().
    private void ClearModeWithoutRefreshing()
    {
        currentMode = TurretMode.None;
        selectedTurret = null;
        selectedTurretIndex = -1;
    }

    // ============================================================
    // Era Tracking Management
    // ============================================================

    // Similar to the Player Spawner Change era, we want to change the turret catalogue UI to another era, updating
    // the turret price cost as well.
    // Note that PlayerSpawner will call this method the player upgrades the era.
    public bool TrySetEra(int eraIndex)
    {
        // If the era we are to upgrade is out of range of turrent regiested eras we have defined in this script
        // it means we cannot update any UI since the is no assets to be updated, thus return false and log
        // a warning on this.
        if (turretEras == null || eraIndex < 0 || eraIndex >= turretEras.Length)
        {
            Debug.LogWarning($"Invalid turret era index: {eraIndex}");

            return false;
        }

        // If not lets obtain the requested era data from the eraindex we are trying to update
        TurretEraData requestedEra = turretEras[eraIndex];

        // If the updated requested era is not defined correctly, where there is no data defined in that era list
        // then we canonot update anything, and have to return false.
        if (requestedEra == null)
        {
            Debug.LogWarning($"Turret era {eraIndex} is not assigned.");

            return false;
        }

        // Or else we know that there is data available to update the turrent era to a new one, thus we
        // update our currentEra index tracker for this script accordingly as well. 
        currentEraIndex = eraIndex;

        // Clear Build or Sell mode without refreshing yet.
        ClearModeWithoutRefreshing();

        // Update the new era's banner, costs and button states for the turret section.
        RefreshEraUI();
        RefreshAllStates();

        // For logging purposes only
        Debug.Log($"Turret era changed to {requestedEra.eraName}.");

        // To tell the playerSpawner script that this trySetEra for the turrent manager has been successfully
        // updated.
        return true;
    } 

    // This helper method helps to retrieve a turret data from the current era, as right now we are
    // storing each turret in the TurretEraData area, where it has a variable called turrets, which is an array
    // that stores the turretData of all turrets for that era, so this help to get the turret data directly.
    private TurretData GetCurrentEraTurret(int turretIndex)
    {
        // if there is no turretEra defined, like the array is empty, there is no way we have any turretdata at
        // all, thus reutrn null.
        if (turretEras == null || turretEras.Length == 0)
        {
            return null;
        }

        // likeiwse if our curretEra is out of range, it means there is no turretEra data, let alone turretData
        // we can extract, thus return null as well.
        if (currentEraIndex < 0 || currentEraIndex >= turretEras.Length)
        {
            return null;
        }

        // Else it means the era is defined, so we want to extract the TurretEraData value
        // to eventually extract the TurretData value
        TurretEraData currentEra = turretEras[currentEraIndex];

        // if the era is not defined, or if the turrets in the current era is not defined, we canot get
        // any turret data, thus return null
        if (currentEra == null || currentEra.turrets == null)
        {
            return null;
        }

        // Here, if the turret data that the user is looking for is out of range of the turret data in the
        // curret array of turrets, it mean we cannot find the turret data the user is inquiring, thus return null
        if (turretIndex < 0 || turretIndex >= currentEra.turrets.Length)
        {
            return null;
        }

        // Else we wull return the turretdata the user is enquiring
        return currentEra.turrets[turretIndex];
    }
   

    // ============================================================
    // Era UI Refreshing Methods
    // ============================================================

    // This helper methods helps to refreshes the optional era banner and turret cost labels.
    private void RefreshEraUI()
    {
        // If our new era does not contain any data, or we are out of the index range, that means there is no
        // data for us to take to update the UI, thus return early and log warning for this attempt.
        if (turretEras == null || currentEraIndex < 0 || currentEraIndex >= turretEras.Length)
        {
            Debug.LogWarning("Cannot refresh turret UI because era data is invalid.");

            return;
        }

        // If not let obtain the turrent UI banner from the TurretEra data array
        TurretEraData currentEra = turretEras[currentEraIndex];

        // Firstly, I need to make sure the turretEraBannerImage is actually set, as this is an optional field
        // I may more many not want to change this.
        if (turretEraBannerImage != null)
        {
            // If this image object is set, and the curret era that I am in is valid has has as curretEra sprite
            // assigned to it, then I while make the image shown in this image "sprite" to be the banner sprite
            // found from this curretEra array value turretBannerSprite object, updating it accordingly
            //  and ensure that the colour is not transparent
            if (currentEra != null && currentEra.turretBannerSprite != null)
            {
                turretEraBannerImage.sprite = currentEra.turretBannerSprite;

                turretEraBannerImage.color = Color.white;
            }
            else
            {
                // If not if there is not banner that exist in the era, then i will set up this image "sprite" to
                // be null and make it transparent.
                turretEraBannerImage.sprite = null;

                turretEraBannerImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        // Likewise, besides the turret UI banner, I will also need to update the each turret cost text
        // and since I had configured a max of 3 turrets, the array values of each turret would be
        // 0, 1, 2, where I will use another helper method UpdateTurretCostText, with the correct turretCost
        // text button and turret details being given to this helper function to update the next accordingly
        UpdateTurretCostText(turret0CostText, GetCurrentEraTurret(0));
        UpdateTurretCostText(turret1CostText, GetCurrentEraTurret(1));
        UpdateTurretCostText(turret2CostText, GetCurrentEraTurret(2));
    }

    // Update the gold display text for each button based on the turret data given
    private void UpdateTurretCostText(TMP_Text costText, TurretData turret)
    {
        // If there is not text from the button, we do nothing and early return
        if (costText == null)
        {
            return;
        }

        // If there is a text for the button, but the is no details of the turret that we want to upgrade
        // it come mean the next era does not have a button for that turrent slot, so there should be no gold
        // text value, thus set the text value to ne "nothing" and return.
        if (turret == null || turret.turretPrefab == null)
        {
            costText.text = "";
            return;
        }

        // else update the button text value based on the new turret goldcost details we ware looking at.
        costText.text = turret.goldCost.ToString();
    }

    // ============================================================
    // Complete UI Refreshing Methods
    // ============================================================

    // Refreshes every state controlled by TurretManager:
    // - Toolbar button interactability
    // - Holder price text
    // - Physical slot visuals
    // - Cancel overlay visibility
    private void RefreshAllStates()
    {
        RefreshToolbarButtonStates();
        RefreshAllSlotVisuals();
        RefreshCancelOverlay();
    }

    // Updates the interactable state of every toolbar button.
    // This methods also automatically exits invalid Build or Sell modes.
    private void RefreshToolbarButtonStates()
    {
        // Refresh each turret-selection button independently.
        RefreshTurretButton(turretButton0, GetCurrentEraTurret(0));
        RefreshTurretButton(turretButton1, GetCurrentEraTurret(1));
        RefreshTurretButton(turretButton2, GetCurrentEraTurret(2));

        // ========================================================
        // Holder Purchase Button
        // ========================================================

        // Find the actual maximum holder count
        int allowedHolderCount = GetAllowedHolderCount();

        // Check if there is still a holder that is yet to be unlocked
        bool hasLockedHolder = unlockedHolderCount < allowedHolderCount;

        // Only perform the affordability check when another
        // holder is actually available for purchase.
        // This bool value will be used to check affordability only when another holder exists.
        bool canAffordHolder = hasLockedHolder && 
                               economySystem != null && 
                               economySystem.CanAffordGold(CurrentHolderGoldCost);

        // The button is set to active or not base on if we can still make a purchase of a turret holder.
        if (unlockHolderButton != null)
        {
            // canAffordHolder already includes hasLockedHolder,
            // so there is no need to check it again here.
            unlockHolderButton.interactable = canAffordHolder;
        }

        // Likewise for the gold cost, where we only update the gold based on the current holder cost, if not
        // if we go pass the max amount of holder purchase, we return the text to be Max instead.
        if (holderCostText != null)
        {
            holderCostText.text = hasLockedHolder ? CurrentHolderGoldCost.ToString() : "MAX";
        }

        // ========================================================
        // SELL-MODE BUTTON
        // ========================================================

        // Check if there has been any turret that has been built
        bool hasBuiltTurret = HasAnyBuiltTurret();

        // Make sure the sell Mode button it active only when there is actually a turret placed that can be sold
        if (sellTurretButton != null)
        {
            sellTurretButton.interactable = hasBuiltTurret;
        }

        // ========================================================
        // AUTOMATIC MODE CANCELLATION
        // ========================================================

        // Cancel Sell mode automatically when no turrets remain.
        // This could happen after the player sells the final turret
        // or another script destroys the final turret.
        // So like after clicking on one turret to be sold, we revet back to normal stage, exiting the sell mode.
        if (currentMode == TurretMode.Sell && !hasBuiltTurret)
        {
            ClearModeWithoutRefreshing();

            RefreshAllSlotVisuals();
            RefreshCancelOverlay();

            return;
        }

        // Cancel Build mode automatically when there are no
        // unlocked empty slots remaining.
        if (currentMode == TurretMode.Build && !HasEmptyUnlockedSlot())
        {
            ClearModeWithoutRefreshing();

            RefreshAllSlotVisuals();
            RefreshCancelOverlay();
            
            return;
        }
    }

    // This method updates one turret-selection button.
    // The button is interactable only when:
    // - Valid turret data exists
    // - A turret prefab is assigned
    // - At least one unlocked empty holder exists
    // - EconomySystem exists
    // - The player can afford the turret
    private void RefreshTurretButton(Button button, TurretData turret)
    {
        if (button == null)
        {
            return;
        }

        bool hasValidTurret = turret != null && turret.turretPrefab != null;

        bool hasEmptySlot = HasEmptyUnlockedSlot();

        bool canAffordTurret = hasValidTurret &&
                               economySystem != null &&
                               economySystem.CanAffordGold(turret.goldCost);

        button.interactable = hasValidTurret && hasEmptySlot && canAffordTurret;
    }

    // This help update each physical TurretSlotUI to refresh itself, where each slots decides whether to display:
    // - Its Scaffolding
    // - Its Placement button
    // - Its Sell button
    private void RefreshAllSlotVisuals()
    {
        if (turretSlots == null)
        {
            return;
        }

        foreach (TurretSlotUI slot in turretSlots)
        {
            if (slot != null)
            {
                slot.RefreshVisualState();
            }
        }
    }

    // This methods helps to show the full-screen cancellation overlay during Build or Sell mode.
    // The overlay is hidden during None mode.
    private void RefreshCancelOverlay()
    {
        if (cancelModeOverlay == null)
        {
            return;
        }

        cancelModeOverlay.gameObject.SetActive(currentMode != TurretMode.None);
    }

    // ============================================================
    // Holder and Slot Checking
    // ============================================================

    // Returns the real maximum number of holders that may be unlocked.
    // This prevents maximumTurretHolders from exceeding the actual number of slots assigned in the Inspector.
    private int GetAllowedHolderCount()
    {
        if (turretSlots == null)
        {
            return 0;
        }

        // This is to allow scalling where we can in theory prep for turret slots, but for different difficulty
        // allow different max turrent slots.
        return Mathf.Min(maximumTurretHolders, turretSlots.Length);
    }
    
    // Returns true when at least one physical slot is:
    // - Unlocked
    // - Not currently occupied by a turret
    private bool HasEmptyUnlockedSlot()
    {
        if (turretSlots == null)
        {
            return false;
        }

        // Scan each turret slot
        foreach (TurretSlotUI slot in turretSlots)
        {
            if (slot != null && slot.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    // Returns true when at least one physical slot currently contains a constructed turret.
    private bool HasAnyBuiltTurret()
    {
        if (turretSlots == null)
        {
            return false;
        }

        // Scan each turret slot
        foreach (TurretSlotUI slot in turretSlots)
        {
            if (slot != null && slot.HasTurret)
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // Unit Testing Helper Methods
    // ============================================================

    // Replaces the turret-era array during automated tests.
    public void SetTurretErasForTesting(
        TurretEraData[] testTurretEras)
    {
        turretEras = testTurretEras;
    }

    // Assigns the manager dependencies during automated tests.
    // The actual scene dependencies should still be assigned through the Unity Inspector.
    public void SetDependenciesForTesting(EconomySystem testEconomySystem, TurretSlotUI[] testTurretSlots)
    {
        economySystem = testEconomySystem;
        turretSlots = testTurretSlots;
    }

    // Allows tests to configure holder prices and limits.
    public void SetHolderConfigurationForTesting(int testMaximumHolders, int testInitialCost, int testCostIncrease)
    {
        maximumTurretHolders = Mathf.Max(0, testMaximumHolders);

        initialHolderGoldCost = Mathf.Max(0, testInitialCost);

        holderGoldCostIncrease = Mathf.Max(0, testCostIncrease);
    }
}
