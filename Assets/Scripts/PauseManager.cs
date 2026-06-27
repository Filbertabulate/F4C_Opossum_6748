using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;

    // A tracker on if the game is paused or not, for potential expension in case we want other escape
    // pause methods, default value is set to false, since the game should not be starting a paused
    private bool isPaused = false;

    // On start, make sure that the pauseMenu canvas overlay is not shown, by setting the active to false
    // at that the timescale is set to 1 which one it is play, i.e. to say the game is active
    void Start()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    // Create a method called TogglePause, and what it does is that it first check if the pause Menu canvas
    // overaly is active
    // i.e. if the Pause Menu is not being shown, the canvas is not active (false), so I should set the 
    // pauseMenu to be active
    // vice versa for the opposite scenario to stop pausing the game.
    public void TogglePause()
    {
        // For tracking and toggling the current pause state.
        isPaused = !isPaused;
        // either show or hid the pause menu based on the current pause state.
        pauseMenu.SetActive(isPaused);
        // Since if the game should be pause, we set the time to be 0f which mean nothing behind will be moving
        Time.timeScale = isPaused ? 0f : 1f;
    }

    // Create a method resume game to deactivate the pause menu screen, and play the game again, by setting
    // the timer back to 1f
    public void ResumeGame()
    {
        // Update the isPaused tracker back to false
        isPaused = false;
        // Remove the pause menu
        pauseMenu.SetActive(false);
        // Resume the game
        Time.timeScale = 1f;
    }

    // If the user wants to quit this game instance, and return back to the Main Menu, we allow them to go back
    // to the Main Menu scene.
    public void ReturnToMainMenu()
    {
        // Reset the game's time scale before changing scenes. This is needed to be done as
        // Time.timeScale is a global value and persists across scene loads, therefore, 
        // without resetting it, the next scene would remain paused.
        Time.timeScale = 1f;
        LevelManager.Instance.LoadSceneNoLoadingBar("Main_Menu", "CrossFade");
    }

    // In the Pause Manager, the method created allows us to play the click and Hover Sounds that are found
    // in the soundManager
    public void Play2DSound(string soundName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(soundName);
        }
    }
}