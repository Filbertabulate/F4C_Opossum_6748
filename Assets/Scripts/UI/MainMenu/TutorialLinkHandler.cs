using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// To allow the web build in unity to openup my github repo that contains a read me that shows all the units
// statistics as well as turrent damage and all, since I did not manage to add in a tooltip function in time
// during this project.

public class TutorialLinkHandler : MonoBehaviour, IPointerClickHandler
{
    // To find the exact text that was clicked, we need to use the TMP_TextUtilities.FindIntersectingLink method.
    [SerializeField]
    private TMP_Text tutorialText;

    // To define the URL we want to open when the player clicks on the link, we can use a serialized field.
    [SerializeField]
    private string statsUrl;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check whether the player clicked on a TMP link.
        // documentation: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/TMP_TextUtilities.html
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tutorialText,eventData.position, eventData.pressEventCamera);

        // The player did not click a link.
        if (linkIndex == -1)
        {
            return;
        }

        // Get the ID stored inside <link="...">.
        string linkId = tutorialText.textInfo.linkInfo[linkIndex].GetLinkID();

        // Open the statistics page.
        if (linkId == "stats")
        {
            Application.OpenURL(statsUrl);
        }
    }
}