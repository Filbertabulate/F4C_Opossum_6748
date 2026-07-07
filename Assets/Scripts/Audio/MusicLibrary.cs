using UnityEngine;

// https://www.youtube.com/watch?v=Q-bKHocRvE0
// To see it in the inspector (the struct), I need to make it serializable
[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip trackClip;
}

public class MusicLibrary : MonoBehaviour
{
    // Array of Music track, called tracks
    public MusicTrack[] tracks;

    // Method called GetClipFromName
    // Essentially what is does is that it help scan through the audio track given, find the correct track
    // from the file that matches the name in the public array, and retrun that audioclip of that track
    public AudioClip GetClipFromName(string trackName)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.trackClip;
            }
        }

        // If no audio clip is found, we return null
        return null;
    }
}
