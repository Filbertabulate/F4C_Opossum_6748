using UnityEngine;
using UnityEngine.SceneManagement; // Required to restart the level

public class GameManager : MonoBehaviour
{
    [Header("Base References")]
    [Tooltip("Drag the main Player Base GameObject here")]
    public GameObject playerBase;
    [Tooltip("Drag the main Enemy Base GameObject here")]
    public GameObject enemyBase;

    // The container holding economy and spawn toolbars
    [Header("UI References")]
    public GameObject gameplayUI;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    private bool gameEnded = false;

    void Start()
    {
        // Ensure time flows normally when the scene starts, and the correct UI is showing
        Time.timeScale = 1f;
        gameEnded = false;

        if (gameplayUI != null) gameplayUI.SetActive(true);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    void Update()
    {
        // If the game is already over, stop checking
        if (gameEnded) return;

        // Because HealthSystem uses Destroy(gameObject), the base will literally become 'null' when it dies.
        if (playerBase == null)
        {
            TriggerDefeat();
        }
        else if (enemyBase == null)
        {
            TriggerVictory();
        }
    }

    void TriggerVictory()
    {
        gameEnded = true;
        Debug.Log("F4C Opossum: Victory Achieved!");

        // Hide the playing UI and show the Victory screen
        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);

        // Freeze time so units stop moving and attacking
        Time.timeScale = 0f; 
    }

    void TriggerDefeat()
    {
        gameEnded = true;
        Debug.Log("F4C Opossum: Base Defeated!");

        // Hide the playing UI and show the Defeat screen
        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(true);

        // Freeze time
        Time.timeScale = 0f; 
    }

    // This method needs to be PUBLIC so the "Play Again" buttons can trigger it
    public void RestartGame()
    {
        // Unfreeze time before reloading, otherwise the new game will start frozen!
        Time.timeScale = 1f; 
        
        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}