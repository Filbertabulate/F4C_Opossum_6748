// Since I want my audio to be carried across multiple stages/scene, I should just create a dedicated audio
// setting UI script.

using UnityEngine;
// For Audio Usage
using UnityEngine.Audio;
// For the Slider
using UnityEngine.UI;
 

public class AudioSettingsUI : MonoBehaviour
{
    //Create a private variable audioMixer as reference
    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixer audioMixer;
    // To know which slider refers to which one, one is for music, one is for sound effects
    [Header("Volume Sliders")]
    [SerializeField]
    private Slider musicSlider;

    [SerializeField]
    private Slider sfxSlider;

    // These are the names used when saving the player's volume settings inside PlayerPrefs.
    // Note that we are saving the slider value from 0 to 1, rather than saving the AudioMixer decibel value.
    private const string MusicVolumeKey = "MusicVolumeLinear";
    private const string SFXVolumeKey = "SFXVolumeLinear";

    // If the player has never changed their volume before, the volume will start at 1, which represents 100% 
    // volume.
    private const float DefaultVolume = 1f;

    // Note that we cannot use exactly 0 when converting the slider value into decibels, 
    // as Log10(0) is mathematically undefined and approaches negative infinity.
    // Therefore, 0.0001 is used as the minimum value instead.
    //
    // 20 * Log10(0.0001) = -80 dB (This is the minimum volume that the AudioMixer can handle without issues)
    //
    // -80 dB is effectively silent for the game, so it works as our minimum volume without causing problems from 
    // trying to calculate Log10(0).
    //
    // As for why log10() is being used, it is because the human ear perceives sound volume logarithmically rather 
    // than linearly.
    private const float MinimumVolume = 0.0001f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure that on start, the volume sliders are set correctly
        LoadVolume();
    }
    

    // Create an UpdateMusicVolume Method to update music volume
    public void UpdateMusicVolume(float volume)
    {
        // Make sure that the slider value stays between 0.0001 and 1, preventing any Log10 from ever receiving 
        // 0 as its input.
        volume = Mathf.Clamp(volume, MinimumVolume, 1f);

        // Now, based on the slider, it will give us a simple value between 0.0001 and 1.
        //
        // However, Unity's AudioMixer volume parameter uses decibels (dB), so we should not directly pass the 
        // slider value into the AudioMixer.
        //
        // As a result, from researching online, it turns out that Audio volume is percived as logarithmic  based 10
        // rather than linear. Therefore, we use:
        //
        //      dB = 20 * Log10(volume)
        //
        // Some examples are:
        //      Slider = 1.0    -->   0 dB   -> Full volume
        //      Slider = 0.5    -->  -6 dB
        //      Slider = 0.1    --> -20 dB
        //      Slider = 0.01   --> -40 dB
        //      Slider = 0.0001 --> -80 dB  -> Effectively silent
        //
        // This allows the 0 to 1 slider to behave naturally while giving the AudioMixer the decibel value that it 
        // expects.
        float volumeInDecibels = Mathf.Log10(volume) * 20f;

        // Apply the converted decibel value to the exposed MusicVolume parameter inside the AudioMixer.
        audioMixer.SetFloat("MusicVolume", volumeInDecibels);

        // Save the original slider value from 0.0001 to 1, as this is what we want to save for the player's 
        // preference, rather than saving the decibel value.
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);

        // Once I configured the Music volume, I should save the PlayerPrefs
        PlayerPrefs.Save();
    }

    // This is to update the sound effect volume
    public void UpdateSoundVolume(float volume)
    {
        // Make sure that the slider value stays between 0.0001 and 1, preventing any Log10 from ever receiving 
        // 0 as its input.
        volume = Mathf.Clamp(volume, MinimumVolume, 1f);

        // Now, based on the slider, it will give us a simple value between 0.0001 and 1.
        //
        // However, Unity's AudioMixer volume parameter uses decibels (dB), so we should not directly pass the 
        // slider value into the AudioMixer.
        //
        // As a result, from researching online, it turns out that Audio volume is percived as logarithmic  based 10
        // rather than linear. Therefore, we use the formula of "dB = 20 * Log10(volume)".
        //
        // This allows the 0 to 1 slider to behave naturally while giving the AudioMixer the decibel value that it 
        // expects.
        float volumeInDecibels = Mathf.Log10(volume) * 20f;

        // Apply the converted volume to the SFX mixer group.
        audioMixer.SetFloat("SFXVolume", volumeInDecibels);

        // Save the player's SFX slider position.
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);

        // Once I configured the SFX volume, I should save the PlayerPrefs
        PlayerPrefs.Save();
    }

    // This is to just update the slider values based on what volume we have saved previously.
    // However, note that the AudioMixer values are also set directly here, instead of only relying on the
    // slider OnValueChanged event to call UpdateMusicVolume or UpdateSoundVolume.
    // I chose to do this as it makes the loading process safer because LoadVolume works correctly even if
    // the slider event does not fire during scene initialization.
    public void LoadVolume()
    {
        // Firstly, we will try to retrieve the player's previously saved slider values.
        // If there is no saved value, such as when someone plays the game for the first time, 
        // DefaultVolume (1f) is used instead.
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume);

        // Ensure that any loaded values are still within the valid range of 0.0001 to 1.
        // Just addition precaution to ensure that the values are valid.
        musicVolume = Mathf.Clamp(musicVolume, MinimumVolume, 1f);
        sfxVolume = Mathf.Clamp(sfxVolume, MinimumVolume, 1f);

        // Update the slider position values
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        // Since we are currently storing the linear slider values, we must convert them back into decibels 
        // before giving them to the AudioMixer (ensure the music and SFX volumes are set correctly).
        //
        // As such, we will use this formula to convert the linear slider value into decibels:
        // dB = 20 * Log10(volume)
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20f);
    }
}
