// https://www.youtube.com/watch?v=ivvv8kld6_0&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=6
using UnityEngine;
 
public class MainMenu : MonoBehaviour
{

    private void Start()
    {
        // On start of the game, we play the main menu music
        MusicManager.Instance.PlayMusic("MainMenu");
    }
    
    public void Play()
    {
        // When pressing the Play Button, we load the Stage_1_Game scene
        LevelManager.Instance.LoadScene("Stage_1_Game", "CrossFade");
        MusicManager.Instance.PlayMusic("GameMusic");
    }

    public void Quit()
    {
        // When we quit the game
        Application.Quit();
    }

    // In the Main Menu, the method created allows us to play the click and Hover Sounds that are found
    // in the soundManager
    public void Play2DSound(string soundName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(soundName);
        }
    }
}