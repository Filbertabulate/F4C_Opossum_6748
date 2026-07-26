using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// To allow build the turret slot holder, and the turret itslef
public class TurretSlotUI : MonoBehaviour
{
    // ============================================================
    // Dependencies
    // ============================================================
    [Header("Turret Manager")]
    [SerializeField]
    private TurretManager turretManager;

    // ============================================================
    // Physical Slot Objects
    // ============================================================

    [Header("Slot Objects")]

    [SerializeField]
    private GameObject scaffolding;
    
    [SerializeField]
    private Transform turretSpawnPoint;

    // ============================================================
    // Slot Buttons
    // ============================================================

    // Referece the buttons that we will click to buy the turret and sell the turret
    [Header("Buttons")]
    public Button buildButton;
    public Button sellButton;

    // ============================================================
    // Button Blinking Settings
    // ============================================================

    [Header("Button Flash Settings")]

    // Set the transulenct of the button flash to be 0.3 / 1 in terms of transparancy scale
    [Tooltip("Lowest opacity reached during flashing.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float flashMinimumAlpha = 0.1f;

    // Likewise the opposite.
    [Tooltip("Highest opacity reached during flashing.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float flashMaximumAlpha = 1f;

    // The flashing speed between transitions.
    [Tooltip("Speed of the flashing animation.")]
    [Min(0.1f)]
    [SerializeField]
    private float flashSpeed = 3f;

    // ============================================================
    // SELL REFUND POPUP
    // ============================================================

    [Header("Sell Refund Popup")]

    // Floating UI text prefab displayed when this turret is sold.
    [SerializeField]
    private FloatingGoldText floatingGoldTextPrefab;

    // RectTransform where the floating refund popup should appear.
    [SerializeField]
    private RectTransform refundPopupSpawnPoint;

    // ============================================================
    // Runtime State
    // ============================================================

    // Whether this holder has been purchased.
    private bool isUnlocked = false;

    // Current turret instance occupying this holder.
    private GameObject currentTurret;

    // Original amount paid for the current turret.
    private int currentTurretPurchaseCost;

    // Existing Images attached to the buttons.
    // Their alpha values are changed to create the flashing effect.
    private Image buildButtonImage;
    private Image sellButtonImage;

    // ============================================================
    // Public asses to value, in a Read-Only State
    // ============================================================

    public bool IsUnlocked => isUnlocked;

    public bool HasTurret => currentTurret != null;

    // A slot is considered empty only when it is:
    // - Unlocked
    // - Not currently occupied by a turret
    public bool IsEmpty => isUnlocked && !HasTurret;

    public GameObject CurrentTurret => currentTurret;

    public int CurrentTurretPurchaseCost => currentTurretPurchaseCost;

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


    // On start of the game instance, I will need to configue the UI accordingly
    private void Awake()
    {
        // Every holder begins locked and empty.
        isUnlocked = false;
        currentTurret = null;
        currentTurretPurchaseCost = 0;

        // Enusre the scaffolding sprite is set as not shown
        if (scaffolding != null)
        {
            scaffolding.SetActive(false);
        }

        // Dont show turret build button
        if (buildButton != null)
        {
            buildButtonImage = buildButton.image;

            buildButton.gameObject.SetActive(false);
        }

        // Dont show turret sell button
        if (sellButton != null)
        {
            sellButtonImage =sellButton.image;

            sellButton.gameObject.SetActive(false);
        }

        ResetButtonAlpha();
    }

    // At start, ensure that the turret holder is not shown, likewise for the unlock and build buttons
    private void Start()
    {
        RefreshVisualState();
    }

    private void Update()
    {
        UpdateButtonFlashing();
    }

    // ============================================================
    // Holder Management
    // ============================================================

    // Unlocks this physical holder.
    // The turret manager will handle the payment payment before calling this method.
    public bool UnlockHolder()
    {
        // If the holder alr unlock, we cannot unlock said holder, return warning and false value.
        if (isUnlocked)
        {
            Debug.LogWarning($"Turret holder is already unlocked: " + $"{gameObject.name}");

            return false;
        }

        // No Scaffolding object given, no UI Object to show, log warning and return false
        if (scaffolding == null)
        {
            Debug.LogWarning($"Scaffolding is not assigned on " + $"{gameObject.name}.");

            return false;
        }

        // If not we set the curret slot holder to be unlocked, showing the scaffolding object.
        isUnlocked = true;
        scaffolding.SetActive(true);

        RefreshVisualState();

        Debug.Log($"Turret holder unlocked: {gameObject.name}");

        return true;
    }

    // ============================================================
    // On-Click Methods for the buttons
    // ============================================================

    /// <summary>
    /// Assign this method to this slot's build button On Click() event.
    /// </summary>
    public void OnBuildButtonClicked()
    {
        // No turrent manager, dont know which turret was selected, thus return warning and early exit
        if (turretManager == null)
        {
            Debug.LogWarning($"TurretManager is not assigned on " + $"{gameObject.name}.");

            return;
        }

        // The button should only be visible when these conditions
        // are already valid. These checks provide additional safety.
        // - turret slot is empty
        // - turret manager is in build mode
        // - the selected turret to build is not null
        if (!IsEmpty || 
            turretManager.CurrentMode != TurretManager.TurretMode.Build || 
            turretManager.SelectedTurret == null)
        {
            // If not we revet back to turretManager build mode back to normal, cuz the conditions for buidling
            // a turret is not met.
            turretManager.CancelCurrentMode();
            return;
        }

        // If not we will build the turret at this curret slot we are in, which is why I use "this"
        turretManager.BuildSelectedTurretAtSlot(this);
    }

    // To assign this method to this slot's Sell button On Click() event.
    public void OnSellButtonClicked()
    {
        // No turrent manager, dont know which turret was selected, thus return warning and early exit
        if (turretManager == null)
        {
            Debug.LogWarning($"TurretManager is not assigned on " + $"{gameObject.name}.");

            return;
        }

        // The Sell button should only be visible when this slot contains a turret and Sell mode is active.
        if (!HasTurret || turretManager.CurrentMode != TurretManager.TurretMode.Sell)
        {
            // If not we revet back to turretManager build mode back to normal, cuz the conditions for selling
            // a turret is not met.
            turretManager.CancelCurrentMode();
            return;
        }

        // If not we will sell the turret at this curret slot we are in, which is why I use "this"
        turretManager.SellTurretFromSlot(this);
    }

    // ============================================================
    // BUILDING
    // ============================================================

    // Spawns a turret on this holder.
    // The manager deducts gold before calling this method.
    public bool BuildTurret(GameObject turretPrefab, int purchaseCost)
    {
        // Unable to build turret as there is a turret there already
        if (!IsEmpty)
        {
            Debug.LogWarning($"Cannot build on {gameObject.name}. " +"The holder is locked or occupied.");

            return false;
        }

        // No turrent object bring given to build said turret at this slot
        if (turretPrefab == null)
        {
            Debug.LogWarning($"Cannot build on {gameObject.name}. " + "The turret prefab is null.");

            return false;
        }

        // No spawnpoint decided for this turret
        if (turretSpawnPoint == null)
        {
            Debug.LogWarning($"Turret Spawn Point is not assigned on " + $"{gameObject.name}.");

            return false;
        }

        // This time, I will give the instantiate a fourth paramter to make it such that the new turret created
        // will be stored under the turretSpawnPoint object, where this turretSpawnPoint object is the parent
        // of the turretPrefab instatnce we want to build.
        currentTurret = Instantiate(turretPrefab, 
                                    turretSpawnPoint.position, 
                                    turretSpawnPoint.rotation, 
                                    turretSpawnPoint);

        // If we are unablw to create the turrent
        if (currentTurret == null)
        {
            Debug.LogWarning($"Failed to spawn a turret on {gameObject.name}.");

            currentTurretPurchaseCost = 0;
            return false;
        }

        // Ensure the instantiated turret is active.
        currentTurret.SetActive(true);

        // Else update the successful purchase of the turret cost as it has been deployed, this for refund tracking
        // later on.
        currentTurretPurchaseCost = Mathf.Max(0, purchaseCost);

        Debug.Log($"Built {currentTurret.name} on {gameObject.name} " + $"for {currentTurretPurchaseCost} gold.");

        RefreshVisualState();

        return true;
    }

    // ============================================================
    // SELLING
    // ============================================================

    // This method destroys the turret currently occupying this holder.
    // Moreover, it returns the refund amount to the manager, as the turret manager is responsible for adding 
    // the refund to the economy.
    public int SellTurret(float refundPercentage = 0.5f)
    {
        // No turret, return 0, and log warning
        if (!HasTurret)
        {
            Debug.LogWarning($"There is no turret to sell on {gameObject.name}.");

            return 0;
        }

        // Just ensuring that the value given has to be in the range form 0 to 1, as percentage style
        float clampedPercentage = Mathf.Clamp01(refundPercentage);

        int refundAmount = Mathf.FloorToInt(currentTurretPurchaseCost * clampedPercentage);

        // Destory the turrent we want to refund
        Destroy(currentTurret);

        // Set the parameters back to default
        currentTurret = null;
        currentTurretPurchaseCost = 0;

        Debug.Log($"Sold turret from {gameObject.name}. " + $"Refund: {refundAmount} gold.");

        RefreshVisualState();

        // Display the popup after destroying the turret.
        // Note that the popup belongs to SlotCanvas, not the turret itself, so destroying the turret will not 
        // destroy the popup.
        ShowRefundPopup(refundAmount);

        // Return the refund amount we gain back to the turret manager to add back to the economy manager
        return refundAmount;
    }

    // ============================================================
    // Visual State
    // ============================================================

    // Updates this slot's scaffolding and action buttons.
    // Placement button:
    // - Shown only during Build mode
    // - Shown only when this slot is unlocked and empty
    // - Shown only when a turret has been selected
    //
    // Sell button:
    // - Shown only during Sell mode
    // - Shown only when this slot contains a turret
    public void RefreshVisualState()
    {
        // A locked holder shows nothing.
        if (!isUnlocked)
        {
            if (scaffolding != null)
            {
                scaffolding.SetActive(false);
            }

            SetBuildButtonVisible(false);
            SetSellButtonVisible(false);

            ResetButtonAlpha();
            return;
        }

        // If not if the holde rhas been unlocked, ensure the scaffolding is set as active
        if (scaffolding != null)
        {
            scaffolding.SetActive(true);
        }

        // If there is no turret manager, ensure that we do not allow any of the build / sell buttons to be shown 
        if (turretManager == null)
        {
            SetBuildButtonVisible(false);
            SetSellButtonVisible(false);

            ResetButtonAlpha();
            return;
        }

        // Conditions to show build button
        bool shouldShowBuildButton = turretManager.CurrentMode == TurretManager.TurretMode.Build &&
                                     turretManager.SelectedTurret != null &&
                                     IsEmpty;

        // Conditions to shown sell button
        bool shouldShowSellButton = turretManager.CurrentMode == TurretManager.TurretMode.Sell &&
                                    HasTurret;

        // Based on the conditions update the build/sell button to be shown or not
        SetBuildButtonVisible(shouldShowBuildButton);

        SetSellButtonVisible(shouldShowSellButton);

        // Restore normal opacity whenever neither action is active.
        if (!shouldShowBuildButton && !shouldShowSellButton)
        {
            ResetButtonAlpha();
        }
    }

    // Shows or hides this slot's build button.
    private void SetBuildButtonVisible(bool isVisible)
    {
        if (buildButton == null)
        {
            return;
        }

        buildButton.gameObject.SetActive(isVisible);

        // This helps reset the build button "blinking"/ flashing value back to default, fully opaque
        if (!isVisible && buildButtonImage != null)
        {
            SetImageAlpha(buildButtonImage, flashMaximumAlpha);
        }
    }

    // Shows or hides this slot's Sell button.
    private void SetSellButtonVisible(bool isVisible)
    {
        if (sellButton == null)
        {
            return;
        }

        sellButton.gameObject.SetActive(isVisible);

         // This helps reset the sell button "blinking"/ flashing value back to default, fully opaque
        if (!isVisible && sellButtonImage != null)
        {
            SetImageAlpha(sellButtonImage, flashMaximumAlpha);
        }
    }

    // ============================================================
    // Blinking / Flashing button
    // ============================================================

    // Only the currently visable action button will blink / flash
    private void UpdateButtonFlashing()
    {
        // ===================================================
        // Trying to create a Repeating 0 to 1 blinking button
        // ===================================================
        // We chose to use Mathf.Sin as since standard timers count straight up (0 -> 1 -> 2...). 
        // However, since we want to make a pulsing button a value that automatically bounces up and down smoothly.
        // As such, we decided to use a sine wave as it goes from 0 to 1 , all while making sure that we handle
        // the negative range values.
        //  
        // How we do the calculations:
        // 1. Time.unscaledTime: Counts seconds continuously. We use "unscaled" so buttons keep flashing even if 
        //    the game is paused (Time.timeScale = 0).
        // 2. * flashSpeed: Scales time so you can speed up or slow down the pulse in the Inspector.
        // 3. Mathf.Sin(...): Outputs a smooth wave oscillating between -1 and +1.
        // 4. (+ 1f) / 2f: To handle the sine negative numbers, we will use this idea where:
        //    - Adding 1 shifts [-1, +1] up to [0, 2].
        //    - Dividing by 2 scales [0, 2] down to [0, 1].
        //
        // Outcome: flashProgress smoothly glides between 0.0 (darkest) and 1.0 (brightest).
        
        float flashProgress = (Mathf.Sin(Time.unscaledTime * flashSpeed) + 1f) / 2f;

        // ----------------------------------------------------------------------------------
        // Ensuring the flash Progress ties correctly to the transpancy values set
        // ----------------------------------------------------------------------------------
        // Wht we chose Math.Leap:
        // Right now, our flashProgress goes from 0 to 1, but we usually don't want buttons to turn 100% invisible.
        // Lerp (Linear Interpolation) allows us to Map 0.0 to my minimum alpha (e.g. 0.2) 
        // and map 1.0 to my maximum alpha (e.g. 1.0), in essence "stretching/tightening the vlaues to meet
        // our min and max values.
        //
        // HOW IT WORKS:
        // When flashProgress = 0.0  --> returns flashMinimumAlpha
        // When flashProgress = 0.5  --> returns halfway point
        // When flashProgress = 1.0  --> returns flashMaximumAlpha
        float currentAlpha = Mathf.Lerp(flashMinimumAlpha, flashMaximumAlpha, flashProgress);

        // At every point in time, we will update the button flashing state only if they are active.
        bool buildButtonIsVisible = buildButton != null && buildButton.gameObject.activeSelf;

        bool sellButtonIsVisible = sellButton != null && sellButton.gameObject.activeSelf;

        if (buildButtonIsVisible && buildButtonImage != null)
        {
            SetImageAlpha(buildButtonImage, currentAlpha);
        }

        if (sellButtonIsVisible && sellButtonImage != null)
        {
            SetImageAlpha(sellButtonImage, currentAlpha);
        }
    }

    // This method restores both action button images to their maximum opacity.
    private void ResetButtonAlpha()
    {
        if (buildButtonImage != null)
        {
            SetImageAlpha(buildButtonImage, flashMaximumAlpha);
        }

        if (sellButtonImage != null)
        {
            SetImageAlpha(sellButtonImage, flashMaximumAlpha);
        }
    }

    // This methods helps to change the transparancy of one UI Image's alpha without changing its RGB values.
    private void SetImageAlpha(Image targetImage, float alpha)
    {
        if (targetImage == null)
        {
            return;
        }

        Color currentColour = targetImage.color;

        currentColour.a = Mathf.Clamp01(alpha);

        targetImage.color = currentColour;
    }

    // ============================================================
    // Unit-Testing Helpers
    // ============================================================

    public void SetManagerForTesting(TurretManager testManager)
    {
        turretManager = testManager;
    }

    public void SetSlotObjectsForTesting(GameObject testScaffolding, Transform testSpawnPoint)
    {
        scaffolding = testScaffolding;
        turretSpawnPoint = testSpawnPoint;
    }

    public void SetButtonsForTesting(Button testPlacementButton, Button testSellButton)
    {
        buildButton = testPlacementButton;
        sellButton = testSellButton;

        buildButtonImage = buildButton != null ? buildButton.image : null;

        sellButtonImage = sellButton != null ? sellButton.image : null;
    }

    // Created a helper method that creates a floating refund message such as "+100".
    // The popup is created under its configured SlotCanvas position, moves upward, fades out, and destroys itself.
    private void ShowRefundPopup(int refundAmount)
    {
        // Edge cases where the variables needed for this method is not defined.
        if (refundAmount <= 0)
        {
            return;
        }

        if (floatingGoldTextPrefab == null)
        {
            Debug.LogWarning($"FloatingGoldText prefab is not assigned on {gameObject.name}.");
            return;
        }

        if (refundPopupSpawnPoint == null)
        {
            Debug.LogWarning($"Refund popup spawn point is not assigned on {gameObject.name}.");
            return;
        }

        // Instantiate the Gold text UI popup as a child of the configured at the slotCanvas spawn point.
        FloatingGoldText popup = Instantiate(floatingGoldTextPrefab, refundPopupSpawnPoint);

        // Obtain the position details of the curret popup genereated, which should be at the refundPopupSpawnPoint.
        RectTransform popupRectTransform = popup.GetComponent<RectTransform>();

        // Just to ensure the pop up is placed correctly
        if (popupRectTransform != null)
        {
            // Position the popup exactly at the spawn point.
            popupRectTransform.anchoredPosition = Vector2.zero;
            popupRectTransform.localRotation = Quaternion.identity;
            popupRectTransform.localScale = Vector3.one;
        }

        // After which, we will run the show refund script that will do the text floating up and fading away
        // animation.
        popup.ShowRefund(refundAmount);
    }
}