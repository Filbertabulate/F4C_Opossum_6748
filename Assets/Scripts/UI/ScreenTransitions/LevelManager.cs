// https://www.youtube.com/watch?v=hF1mkGENOS4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=4
// A singleton, so we can acces it anywhere
// This class allows the loading of the scene asynchroniously
// and Calls the specified transition animation
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
 
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
 
    // Reference to the Slider used as the loading progress bar
    public Slider progressBar;

    // To reference the transitionContainer GameObject that stores all the transition items needs
    public GameObject transitionsContainer;
 
    // An array of scene transition, it is private as we are going to programme it to be populated at the 
    // start via the transitionsContainer children field(s)
    private SceneTransition[] transitions;

    // Ensure only one instance exist
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }
 
    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }
 
    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        // A simple for loop, but using the Linq to make it one line.
        // What this is do is essentially accessing the transisitons array, looping through
        // all the elements in the array, and finding the trasition name in the array that
        // matchs the transitionName we are declaring for the Coroutine
        // It then return the screen transition that we are looking for
        SceneTransition transition = transitions.FirstOrDefault(t => t.name == transitionName);
 
        // For error catching purposes, in case we get like no transition name found by mistake
        if (transition == null)
        {
            Debug.LogError("Transition not found: " + transitionName);
            yield break;
        }

        // Load the scene that was passed in parallel
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        // We set this to false so that Unity does not immediately activite the scene once it finishes loading
        // but rather when we tell it to do so.
        scene.allowSceneActivation = false;
 
        // We wait for the animation to finish before progressing to the new line
        yield return transition.AnimateTransitionIn();
 
        // Active the progress loading bar, the slides to pop up in the animation screen
        progressBar.gameObject.SetActive(true);

        // This do loop is basically updating the progress bar by using the scene.progress value
        // to show how long it take for the scene to load
        do
        {
            // To making the loading more complete
            // In this case since the next scene not that big, should load quite fast
            progressBar.value = Mathf.Clamp01(scene.progress / 0.9f);
            yield return null;
        } while (scene.progress < 0.9f);

        // Unity's async scene loading usually stops at 0.9 when allowSceneActivation is false. Moreover, 
        // the remaining 0.1 happens only after we allow the scene to activate. thus I will stop the loop after
        // the 0.9f to activate the scene.
 
        yield return new WaitForSeconds(1f);
 
        // Activate the scene, i.e. to show the users the next scene
        scene.allowSceneActivation = true;

        // Just to ensure scene is indeed loaded before moving to the new scene, since we dont want
        // breakages between scenes
        while (!scene.isDone)
        {
            yield return null;
        }

        // Remove the progress bar from being shown 
        progressBar.gameObject.SetActive(false);
 
        // To complete the transition out animation
        yield return transition.AnimateTransitionOut();
    }

    // Create Another method to do the transistion, but this time there is no Loading Bar
    public void LoadSceneNoLoadingBar(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneNoLoadingBarAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneNoLoadingBarAsync(string sceneName, string transitionName)
    {
        // A simple for loop, but using the Linq to make it one line.
        // What this is do is essentially accessing the transisitons array, looping through
        // all the elements in the array, and finding the trasition name in the array that
        // matchs the transitionName we are declaring for the Coroutine
        // It then return the screen transition that we are looking for
        SceneTransition transition = transitions.FirstOrDefault(t => t.name == transitionName);
 
        // For error catching purposes, in case we get like no transition name found by mistake
        if (transition == null)
        {
            Debug.LogError("Transition not found: " + transitionName);
            yield break;
        }

        // Load the scene that was passed in parallel
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        // We set this to false so that Unity does not immediately activite the scene once it finishes loading
        // but rather when we tell it to do so.
        scene.allowSceneActivation = false;
 
        // We wait for the animation to finish before progressing to the new line
        yield return transition.AnimateTransitionIn();
 
        yield return new WaitForSeconds(1f);
 
        // Activate the scene, i.e. to show the users the next scene
        scene.allowSceneActivation = true;

        // Make sure we dont transition out too fast, ensure that the scene is fully build then we "load"
        // that scene
        while (!scene.isDone)
        {
            yield return null;
        }
 
        // To complete the transition out animation
        yield return transition.AnimateTransitionOut();
    }
}