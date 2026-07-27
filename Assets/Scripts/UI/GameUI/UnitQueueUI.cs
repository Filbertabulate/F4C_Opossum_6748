using TMPro;
using UnityEngine;

// This script is to help keep track of the current Unit Queue that has been calld by the player
public class UnitQueueUI : MonoBehaviour
{
    // Stores all UI references belonging to one unit button.
    [System.Serializable]
    public class UnitQueueSlotUI
    {
        [Header("Training Bar")]
        // The parent training bar, where this object should be the one with the green Image component.
        // It is shown only while this unit is actively training.
        public GameObject trainingBarRoot;

        // The red child placed over the green training bar.
        // Its Y scale decreases from 1 to 0 as training progresses.
        public RectTransform redTrainingFill;

        [Header("Queue Count")]
        // Shows how many additional copies of this unit
        // are waiting in the production queue.
        // Examples:
        // +1
        // +2
        // +3
        public TextMeshProUGUI pendingCountText;
    }

    [Header("Dependencies")]
    // The PlayerSpawner owns and updates the production queue.
    // We will be using use:
    // - GetTrainProgress()
    // - GetPendingCount()
    [SerializeField]
    private PlayerSpawner playerSpawner;

    [Header("Unit Queue Slots")]
    // The array order must match the PlayerSpawner unit order:
    // Element 0 = Unit index 0
    // Element 1 = Unit index 1
    // Element 2 = Unit index 2
    [SerializeField]
    private UnitQueueSlotUI[] unitSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide all queue UI and reset all red bars when the scene begins.
        ResetAllQueueUI();
    }

    // Update is called once per frame
    void Update()
    {
        // We cannot update anything without PlayerSpawner, thus early return.
        if (playerSpawner == null)
        {
            return;
        }

        // Likewise if the slot array does not exist, there is no unit that can be trained, thus we should not
        // show the train bar and the text count.
        if (unitSlots == null)
        {
            return;
        }

        // At every frame, we will update every configured unit slot.
        for (int unitIndex = 0; unitIndex < unitSlots.Length; unitIndex++)
        {
            UpdateUnitSlot(unitIndex);
        }
    }

    // Helper method to update every unit UI
    private void UpdateUnitSlot(int unitIndex)
    {
        // Obtain this unit's UI references.
        UnitQueueSlotUI slot = unitSlots[unitIndex];

        // Skip an empty or unconfigured slot.
        if (slot == null)
        {
            return;
        }

        // Update the training bar, to be shown or not, to show progress or not, as well as its pending
        // count if there is
        UpdateTrainingBar(slot, unitIndex);
        UpdatePendingCount(slot, unitIndex);
    }

    private void UpdateTrainingBar(UnitQueueSlotUI slot, int unitIndex)
    {
        // For PlayerSpawner.GetTrainProgress() returns values: 
        // 
        // 0 to 1   -> It means that this unit is currently training.
        // -1       -> This unit exists but is not currently training.
        // -2       -> The unit index is invalid.
        float progress = playerSpawner.GetTrainProgress(unitIndex);

        // A value from 0 to 1 means this specific unit is currently at the front of the queue.
        bool isCurrentlyTraining = progress >= 0f;

        // Show the bar only for the unit currently training.
        if (slot.trainingBarRoot != null)
        {
            slot.trainingBarRoot.SetActive(isCurrentlyTraining);
        }

        // Stop if the red RectTransform was not assigned, since it means we cannot show can update
        // the red bar dropping progress.
        if (slot.redTrainingFill == null)
        {
            return;
        }

        // If the unit is currently being trained
        if (isCurrentlyTraining)
        {
            // Prevent values below 0 or above 1.
            progress = Mathf.Clamp01(progress);

            // Training begins:
            // progress = 0
            // red scale Y = 1
            // bar is fully red.
            //
            // Halfway:
            // progress = 0.5
            // red scale Y = 0.5
            // top half reveals green.
            //
            // Finished:
            // progress = 1
            // red scale Y = 0
            // bar is fully green.
            float remainingRedAmount = 1f - progress;

            // Changing the red fill y vector height from 1 to 0, showcasing the progress being made.
            slot.redTrainingFill.localScale = new Vector3(1f, remainingRedAmount, 1f);
        }
        // If not, ensure the red fill is set as max height for now, so when it comes to this unit training turn
        // the bar is configued correctly when displayed
        else
        {
            // Reset the red overlay to full size. This ensures that the next time this unit starts training, 
            // its bar begins fully red again.
            ResetRedFill(slot.redTrainingFill);
        }
    }

    // Helper method to update each Unit pending count, if there is a valid backlog, the we show the count, if
    // its currently being trained, i.e. 0, do not need to show this text.
    private void UpdatePendingCount(UnitQueueSlotUI slot, int unitIndex)
    {
        // Stop if the text field was not assigned.
        if (slot.pendingCountText == null)
        {
            return;
        }

        // This counts unit spawn calls waiting behind the currently training unit.
        // It does not count the active training unit.
        int pendingCount = playerSpawner.GetPendingCount(unitIndex);

        if (pendingCount > 0)
        {
            // Show text such as +1, +2 or +3.
            slot.pendingCountText.gameObject.SetActive(true);
            slot.pendingCountText.text = "+" + pendingCount;
        }
        else
        {
            // Hide the text when there are no waiting copies.
            slot.pendingCountText.text = "";
            slot.pendingCountText.gameObject.SetActive(false);
        }
    }

    private void ResetRedFill(RectTransform redFill)
    {
        if (redFill == null)
        {
            return;
        }

        // Restore the red overlay to its full original size.
        redFill.localScale = Vector3.one;
    }

    private void ResetAllQueueUI()
    {
        if (unitSlots == null)
        {
            return;
        }

        foreach (UnitQueueSlotUI slot in unitSlots)
        {
            if (slot == null)
            {
                continue;
            }

            // Reset the red overlay.
            ResetRedFill(slot.redTrainingFill);

            // Hide the training bar.
            if (slot.trainingBarRoot != null)
            {
                slot.trainingBarRoot.SetActive(false);
            }

            // Hide the pending-count text.
            if (slot.pendingCountText != null)
            {
                slot.pendingCountText.text = "";
                slot.pendingCountText.gameObject.SetActive(false);
            }
        }
    }
}
