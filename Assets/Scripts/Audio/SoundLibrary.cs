// https://www.youtube.com/watch?v=jEoobucfoL4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=5
using UnityEngine;

// To create a structure to store the sound effect we want to use
[System.Serializable]
public struct SoundEffect
{
    public string groupID;
    public AudioClip[] clips;
}

public class SoundLibrary : MonoBehaviour
{
    // To see the sound effect in the inspector window
    public SoundEffect[] soundEffects;
 
    // Go through all the sound effects in the soundEffects array, and return the sound effect that
    // we ware looking for, in terms of its audio clip
    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        return null;
    }
}