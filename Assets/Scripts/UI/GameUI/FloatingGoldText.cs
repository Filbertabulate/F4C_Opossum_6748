using System.Collections;
using TMPro;
using UnityEngine;

// What this script does overall is that it displays a temporary floating gold value.
//
// Example:
// +100
//
// The text moves upwards, fades out, and destroys itself.
public class FloatingGoldText : MonoBehaviour
{
    // UI References
    [Header("UI References")]

    // TextMeshPro text used to display the refunded gold amount.
    [SerializeField]
    private TMP_Text goldText;

    // CanvasGroup used to fade the entire popup.
    [SerializeField]
    private CanvasGroup canvasGroup;

    // Animation Configuration
    [Header("Animation Configuration")]

    // How far upwards the popup moves in UI units.
    [Min(0f)]
    [SerializeField]
    private float riseDistance = 60f;

    // How long the entire popup remains alive.
    [Min(0.01f)]
    [SerializeField]
    private float animationDuration = 1f;

    // How long the popup remains completely visible before fading.
    [Min(0f)]
    [SerializeField]
    private float fadeDelay = 0.25f;

    // ===========================
    // Runtime State
    // ===========================

    // RectTransform is used because this popup belongs to a Canvas.
    private RectTransform rectTransform;

    // Starting anchored position before the popup begins moving.
    private Vector2 startingPosition;

    // Prevents the animation from being started multiple times.
    private Coroutine animationCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Set up the script fields on start of the game.
        rectTransform = GetComponent<RectTransform>();

        // Just to ensure that all my floating goldText has a rectTransform datatype, since we are using it as
        // a UI Object, not a normal sprite object.
        if (rectTransform == null)
        {
            // Every UI object in this case should use RectTransform rather than a normal Transform.
            rectTransform = transform as RectTransform;
        }

        // Automatically find the TMP text when it was not assigned manually through the Inspector.
        if (goldText == null)
        {
            goldText = GetComponent<TMP_Text>();
        }

        // Automatically find the CanvasGroup when it was not assigned manually through the Inspector.
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    // ===========================
    // Helper Methods
    // ===========================

    // This method displays a refunded gold amount and starts the animation.
    // For example:
    // ShowRefund(100)
    // displays "+100".
    public void ShowRefund(int refundAmount)
    {
        // Ensure the popup is active before trying to animate it.
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Retrieve the components again here instead of relying only on Awake().
        //
        // This is important when the prefab was saved as inactive,
        // because ShowRefund() may be called before Awake() has cached
        // all the references.
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (goldText == null)
        {
            goldText = GetComponent<TMP_Text>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        // Dont show text since refound amount cant be zero either way
        if (refundAmount <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // If gold text not provided, then this object should just be destroyed, not shown to user
        if (goldText == null)
        {
            Debug.LogWarning($"FloatingGoldText on {gameObject.name} has no TMP text assigned.");

            Destroy(gameObject);
            return;
        }

        // Likewise for the rectTransform object
        if (rectTransform == null)
        {
            Debug.LogWarning($"FloatingGoldText on {gameObject.name} has no RectTransform.");

            Destroy(gameObject);
            return;
        }

        // Else we will show the exact amount being returned.
        goldText.text = $"+{refundAmount}";

        // The temporary text should never block mouse clicks.
        goldText.raycastTarget = false;

        // Now, we make sure that the popup starts fully visible at this stage.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Store the starting position before movement begins.
        startingPosition = rectTransform.anchoredPosition;

        // Stop an existing animation if ShowRefund is somehow called more than once on the same popup.
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // If not let start the animation
        animationCoroutine = StartCoroutine(AnimatePopup());
    }

    // ===========================
    // Animation methods
    // ===========================

    // This method essentially just moves the text upward while gradually fading it out.
    private IEnumerator AnimatePopup()
    {
        // keep track of how fast the text should be moving up
        float elapsedTime = 0f;

        Vector2 endingPosition = startingPosition + Vector2.up * riseDistance;

        // Prevent the fade delay from exceeding the complete animation duration.
        float safeFadeDelay = Mathf.Min(fadeDelay, animationDuration);
        // This is to ensure that the fad transition still takes place
        float fadeDuration = Mathf.Max(0.01f, animationDuration - safeFadeDelay);

        // So while the animation of text moving up is still allowed, we will keep moving up the text based
        // of the time taken in terms of percentage, positions the movement speed such that the text will go
        // from its starting to ending position in the correct animation duration length
        while (elapsedTime < animationDuration)
        {
            // Used unscaledDeletaTime instead of deltaTime so that the popup will finish its animation
            // even if the player pause the game as the action has already been completed.
            elapsedTime += Time.unscaledDeltaTime;

            // Store it as a percentage value
            float movementProgress = Mathf.Clamp01(elapsedTime / animationDuration);

            // Smoothly move from the starting position to the ending position.
            float smoothMovement = Mathf.SmoothStep(0f, 1f, movementProgress);

            // Ensure that the text movement is "uniform"
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, endingPosition, smoothMovement);

            // Keep the text fully visible until fadeDelay passes.
            if (canvasGroup != null)
            {
                if (elapsedTime <= safeFadeDelay)
                {
                    // still show the text
                    canvasGroup.alpha = 1f;
                }
                else
                {
                    // Else we keep track of the fade progress and update the text transparency till it becomes
                    // transparent
                    float fadeProgress = Mathf.Clamp01((elapsedTime - safeFadeDelay) / fadeDuration);
                    canvasGroup.alpha = 1f - fadeProgress;
                }
            }

            // Make sure the animation still take places and finishes before moving on.
            yield return null;
        }

        // Remove the temporary popup after the animation.
        Destroy(gameObject);
    }
}
