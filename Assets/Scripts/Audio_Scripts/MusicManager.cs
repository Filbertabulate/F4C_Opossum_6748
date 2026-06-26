// https://www.youtube.com/watch?v=Q-bKHocRvE0
using UnityEngine;

// Using System.collection for arrays
using System.Collections;

public class MusicManager : MonoBehaviour
{
    // Able to access anywhere in our game
    public static MusicManager Instance;

    // Reference to MusicLibrary class
    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    // To keep track of the currently active fade loop to prevent overlap bugs
    private Coroutine activeFadeCoroutine;
    
    // Create an awake method so that on start of the game, this this is executed
    private void Awake()
    {
        // Remove any gameObject duplicate if the instance is not null
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // When we change scenes, we dont want the game object to be destroyed
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        // Created a variable to get the clip file from our library list
        AudioClip nextTrack = musicLibrary.GetClipFromName(trackName);

        // In case if the song name was misspelled, we will catch the error and return a debug warning
        if (nextTrack == null)
        {
            Debug.LogWarning($"Music track '{trackName}' not found in library!");
            return;
        }

        // Second check to stop any active crossfade loop to prevent volume glitching
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        // Once all the catches are done, if we can reach here, we will start the clean crossfade process
        activeFadeCoroutine = StartCoroutine(AnimateMusicCrossFade(nextTrack, fadeDuration));
    }
    
    // Take in audio clip that we want to play, and the fade duration, which is set by default to be 0.5f
    // i.e. half a second
    IEnumerator AnimateMusicCrossFade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        // temp float percent variable created, which will be used to help keep track of the music
        // where the percent increase goes from 0 to 1 based on the fade duration, in this case percent goes
        // from 0 to 1 in 0.5 seconds
        float percent = 0;
        // Store the original volume setting, based on what the user is looking for
        float targetMaxVolume = 1.0f; 
        float startVolume = musicSource.volume;

        // To slowly fade out the music we are playing on the current track
        while (percent < 1 && musicSource.isPlaying)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            // We are fading out the music of the current track
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            // Since we are inside the Coroutine, i.e. the Ienumerator, this helps to pause
            // the execution of this coroutine until the very next frame, all while preserving all of its
            // local variable data, in this case percent.
            yield return null;
        }

        // Swap the asset file and start playing the new track, which has the initial volumns of 0
        musicSource.clip = nextTrack;
        musicSource.volume = 0f;
        musicSource.Play();

        // Same thing, but this time we are fading the new volumne in, from 0 to 1.
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, targetMaxVolume, percent);
            yield return null;
        }
    }
}