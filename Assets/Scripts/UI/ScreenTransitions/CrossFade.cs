// https://www.youtube.com/watch?v=hF1mkGENOS4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=4
// This is to Fade the image in and Out
using System.Collections;
using UnityEngine;
using DG.Tweening;
 
[System.Serializable]
public class CrossFade : SceneTransition
{
    // Reference the crossFade Canvas Group
    public CanvasGroup crossFade;
    
    public override IEnumerator AnimateTransitionIn()
    {
        // During the transition, the user cannot click into anything else that might
        // be loading behind
        crossFade.blocksRaycasts = true;
        crossFade.interactable = true;
        
        // Do Tween to fade to 1, i.e. fade to black
        var tweener = crossFade.DOFade(1f, 1f);
        // Wait for it to finish
        yield return tweener.WaitForCompletion();
    }
 
    public override IEnumerator AnimateTransitionOut()
    {
        // Fade to transparent
        var tweener = crossFade.DOFade(0f, 1f);
        yield return tweener.WaitForCompletion();


        // Only after it load finish, then the user can click through the fade
        crossFade.blocksRaycasts = false;
        crossFade.interactable = false;
    }
}