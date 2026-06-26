// https://www.youtube.com/watch?v=hF1mkGENOS4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=4
// This is to Move a Circle object accross the Screen

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// Should be imported alr
using DG.Tweening;
 
public class CircleWipe : SceneTransition
{
    // The black circle I want to use to create the wipe effect
    public Image circle;
 
     public override IEnumerator AnimateTransitionIn()
    {
        // Getting X position that is fully outside the left side of the screen.
        // Next, from the negative width of the screen, which is halfway,
        // we minus off the width of the circle as well so that the entire image starts
        // off-screen regardless of its size.
        float offscreenX = -Screen.width - circle.rectTransform.rect.width;

        // Position the circle just outside the left side of the screen.
        circle.rectTransform.anchoredPosition = new Vector2(offscreenX, 0f);

        // Tween the circle towards the centre of the screen over 1 second.
        var tweener = circle.rectTransform.DOAnchorPosX(0f, 1f);

        // Wait until the transition animation has completed before continuing.
        yield return tweener.WaitForCompletion();
    }
 
    public override IEnumerator AnimateTransitionOut()
    {
        // Getting X position that is fully outside the right side of the screen.
        // Next, from the width of the screen, which is halfway from the mid point
        // we add off the width of the circle as well so that the entire image starts
        // off-screen regardless of its size.
        float offscreenX = Screen.width + circle.rectTransform.rect.width;

        // Tween the circle from the centre to the right side of the screen.
        var tweener = circle.rectTransform.DOAnchorPosX(offscreenX, 1f);

        // Wait until the transition animation has completed before continuing.
        yield return tweener.WaitForCompletion();
    }
}
