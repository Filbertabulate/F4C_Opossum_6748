// https://www.youtube.com/watch?v=B40xBPXK97A&list=PLIvwrsXuTVRCMOtbhN-oRf8U8wba-Y0t7
// Where to build this effect
using UnityEngine;
// Using the new input system, so we need to add this namespace
using UnityEngine.InputSystem;

public class MenuParallaxBehaviour : MonoBehaviour
{
    // The multiplier for the offset of the object, this will determine how much the object moves in relation \
    // to the camera
    public float offsetMultiplier = 1f;

    // The smooth time is used for the smooth damp function, which will make the movement of the 
    // obejct smoother and less jittery
    public float smoothTime = 0.3f;
    
    // Store the initial position of the object, using vector 2 since we are focsuing on the x and y axis
    private Vector2 startPos;

    // Since we are using the smooth damp function, we need to store the velocity of the object
    private Vector2 velocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store the current innitial position of the object
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // If we cannot get the mouse position, then we will not be able to calculate the offset, 
        // so we will return early
        if (Mouse.current == null)
        {
            return;
        }
        // Get the mouse position, and convert it to the viewport space
        Vector2 offset = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());

        // To make the offset more smoother
        transform.position = Vector2.SmoothDamp(transform.position, startPos + (offset * offsetMultiplier),
                                                ref velocity, smoothTime);
        
    }
}