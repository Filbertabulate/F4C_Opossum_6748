// https://www.youtube.com/watch?v=ivvv8kld6_0&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=6
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// For the audio control
using UnityEngine.Audio;
// For the Slider
using UnityEngine.UI;
 
public class MainMenu : MonoBehaviour
{

    //Create a public variable audioMixer as reference
    public AudioMixer audioMixer;
    // To know which slider refers to which one, one is for music, one is for sound effects
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Ensure that on start, the volume sliders are set correctly
        LoadVolume();
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

    // Create an UpdateMusicVolume Method to update music volume
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    // This is to update the sound effect volume
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }
 
    // Acessomg the audio mixer to get the music and sfx volume value, temporary storing it in the musicVolume
    // and sfxVolume float variable.
    // Then we save the musicVolume and SFXVolume under the playerPrefs for now under the "MusicVolume" and 
    // "SFXVolume" name.
    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
 
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
 
    // This is to just update the slider values based on what volumne we have saved previously
    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}