// https://www.youtube.com/watch?v=hF1mkGENOS4&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7&index=4
using System.Collections;
using UnityEngine;


// For classes to inherite the animations in and out methods
public abstract class SceneTransition : MonoBehaviour
{
    public abstract IEnumerator AnimateTransitionIn();
    public abstract IEnumerator AnimateTransitionOut();
}