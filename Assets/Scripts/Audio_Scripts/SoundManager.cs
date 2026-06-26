// https://www.youtube.com/watch?v=jEoobucfoL4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=5
using UnityEngine;
 
public class SoundManager : MonoBehaviour
{
    // Allow access of this class wherever we are in the game 
    public static SoundManager Instance;
 
    [SerializeField]
    // To be able to be seen in the inspector, just that we cannot edit it there, only can be edited in this class
    // itself
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
 
    private void Awake()
    {
        // Only one instance of this is allowed at any point in time
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // To persist between scene changes
            DontDestroyOnLoad(gameObject);
        }
    }
 
    // Play the audio only if the clip exist at that position
    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }
 
    // Method overload, that plays the sound library just based on the name itself
    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);
    }
 
    // This is mainly for UI, which is what I am looking at currently, the others are potential future implementions
    // Take in the soundName, and play the audio of the sound effect stright away based on the soundName.
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }
}