// Since I want my audio to be carried across multiple stages/scene, I should just create a dedicated audio
// setting UI script.

using UnityEngine;
// For Audio Usage
using UnityEngine.Audio;
// For the Slider
using UnityEngine.UI;
 

public class AudioSettingsUI : MonoBehaviour
{
    //Create a public variable audioMixer as reference
    public AudioMixer audioMixer;
    // To know which slider refers to which one, one is for music, one is for sound effects
    public Slider musicSlider;
    public Slider sfxSlider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure that on start, the volume sliders are set correctly
        LoadVolume();
    }
    

    // Create an UpdateMusicVolume Method to update music volume
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        // Once the audio is update, I would want to save that updated volume
        SaveVolume();
    }

    // This is to update the sound effect volume
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        // Once the audio is update, I would want to save that updated volume
        SaveVolume();
    }

    // Accessingg the audio mixer to get the music and sfx volume value, temporary storing it in the musicVolume
    // and sfxVolume float variable.
    // Then we save the musicVolume and SFXVolume under the playerPrefs for now under the "MusicVolume" and 
    // "SFXVolume" name.
    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
 
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

        // Once I configured both the Music and SFX volumns, I should save the PlayerPrefs
        PlayerPrefs.Save();
    }

    // This is to just update the slider values based on what volume we have saved previously.
    // However, note that the AudioMixer values are also set directly here, instead of only relying on the
    // slider OnValueChanged event to call UpdateMusicVolume or UpdateSoundVolume.
    // I chose to do this as it makes the loading process safer because LoadVolume works correctly even if
    // the slider event does not fire during scene initialization.
    public void LoadVolume()
    {
        // Try to get the music volumne, else we get it set back to default (max), which is 0f
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);

        // Update the slider position values
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        // Update the audio mixer values
        audioMixer.SetFloat("MusicVolume", musicVolume);
        audioMixer.SetFloat("SFXVolume", sfxVolume);
    }
}
