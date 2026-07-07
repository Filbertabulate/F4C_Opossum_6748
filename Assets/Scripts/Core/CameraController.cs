using UnityEngine;
// Need to include the InputSystem namespace to access the new input system features, 
// such as reading mouse position and scroll input.
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
     public float moveSpeed = 10f;

    // Background boundaries found from trial and error
    public float minX = 0f;
    public float maxX = 40f;

    // edgeSize refers to the distance from the edge of the screen at which the camera will start moving when 
    // the mouse cursor is near the edge.
    // Values for edgeSize were found through trial and error to find the best value for the game, 
    // with 50f being the value that provided a good balance between responsiveness and ease of use for the
    // player when moving the camera around the map.
    public float edgeSize = 50f;

    // This are the settings for the zooming in and out of the map,
    // with all values being obtain via trial and error to find the best values for the game
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 8f;

    // These variables are used to store the initial Y and Z positions of the camera, 
    // which will be used to lock the camera's movement along these axes. 
    // The groundY variable is calculated based on the camera's orthographic size to ensure that the 
    // camera remains anchored to the ground when zooming in and out.
    private Camera cam;
    private float fixedY;
    private float fixedZ;
    private float groundY;

    // Utilising Awake to ensure the camera component is assigned before any Start methods are called
    private void Awake()
    {
        // Get the camera component and store initial positions of the camera
        cam = GetComponent<Camera>();
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        groundY = transform.position.y - cam.orthographicSize;
    }

    // Update is called once per frame
    private void Update()
    {
        // These update call should help to pick up if the user wants to move the camara by using
        // the mouse curser position
        // or using the scroll wheel to zoom in and out of the map
        Movement();
        Zoom();
    }

    // This method handles the movement of the camera based on the mouse position.
    // We only care about moving left and right for this method
    private void Movement()
    {
        // To get the current x, y and z position of the camara and store it in a variable called pos, 
        // which will be modified based on the mouse position and then applied back to the camera's transform.
        Vector3 pos = transform.position;

        // Get the current mouse position in screen coordinates
        // we use Vector2 because we only care about the x and y position of the mouse for movement.
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Move left
        if (mousePos.x <= edgeSize)
        {
            // If the cursor is within the edgeSize distance from the left edge of the screen, 
            // we move the camera to the left by decreasing the x position of the camera's transform.
            // The movement is scaled by moveSpeed and Time.deltaTime to ensure smooth and consistent movement 
            // regardless of frame rate.
            pos.x -= moveSpeed * Time.deltaTime;
        }

        // Move right
        // We use screen.width to get the width of the screen in pixels, 
        // and we check if the mouse position is within the edgeSize distance from the right edge of the screen.
        if (mousePos.x >= Screen.width - edgeSize)
        {
            // If the cursor is within the edgeSize distance from the right edge of the screen, 
            // we move the camera to the right by increasing the x position of the camera's transform.
            // The movement is scaled by moveSpeed and Time.deltaTime to ensure smooth and consistent movement 
            // regardless of frame rate.
            pos.x += moveSpeed * Time.deltaTime;
        }

        // Camera size
        // We calculate the camera's width and height based on its orthographic size and aspect ratio.
        // Note that the cam.orthographicSize represents half of the camera's height in world units.
        float camHeight = cam.orthographicSize;
        // The camera width should be from the minX value to the middle of the camera view.
        float camWidth = camHeight * cam.aspect;

        // Since this handleMovement method is only responsible for moving the camera left and right, 
        // we need to ensure that the camera does not move outside the boundaries of the background.
        // Clamp inside background
        // the Mathf.Clamp function is used to restrict the x position of the camera within the specified 
        // minimum and maximum values, where .Clamp(value, min, max) takes the value to be clamped and 
        // the minimum and maximum limits as parameters.
        // Since the value in .Clamp referces the the centre position of the camera, I need to ensure
        // that center does not hit pass the external boundaries, i.e. we factor in the distace from the
        // wall to the center of the camera, which is the camWidth variable, 
        // to ensure that the camera does not go past the boundaries of the background.
        pos.x = Mathf.Clamp(pos.x, minX + camWidth, maxX - camWidth);

        // I will only lock the z axis for the camera position as I want 
        // the camera to be able to zoom in and out at the same time while moving left and right, thus the 
        // y axis value should not be fixed.
        pos.z = fixedZ;

        // Once the new position has been calculated based on the mouse position and clamped within the boundaries,
        // we apply the new position back to the camera's transform to update its position in the game world.
        transform.position = pos;
    }

    // This method handles the zooming of the camera based on the scroll wheel input.
    private void Zoom()
    {
        // Get the scroll input from the mouse scroll wheel, which is a Vector2 where the y component 
        // represents the vertical scroll.
        // This helps to mimic the scrolling up to zoom in and scrolling down to zoom out functionality 
        // for the camera.
        float scroll = Mouse.current.scroll.ReadValue().y;

        // If there is no scroll input, we can return early from the method to avoid unnecessary 
        // calculations and updates to the camera's orthographic size.
        if (scroll == 0)
            return;

        // Zoom
        /**
        * For zooming in and out, we adjust the camera's orthographic size based on the scroll input 
        * and the zoomSpeed.
        * All the values for the zooming in and out of the map, with all values being obtain via trial and error 
        * to find the best values for the gameplay experience.
        *
        * Also for this line, we minus the scroll * zoomSpeed * 0.01f because we want to zoom in when scrolling up 
        * (positive scroll value) and zoom out when scrolling down (negative scroll value).
        * This is because in Unity, the orthographic size of the camera determines how much of the game world is 
        * visible on the screen, and a smaller orthographic size means a closer zoom (more zoomed in), while a 
        * larger orthographic size means a farther zoom (more zoomed out).
        *
        * The zoom values are clamped between minZoom and maxZoom to prevent the player from zooming in too 
        * closely or too far away, which may negatively affect gameplay visibility and overall aesthetics.
        * 
        * After zooming, the camera position must also be re-clamped because changing the orthographic size 
        * changes the visible width and height of the camera, which may otherwise cause the camera to reveal 
        * areas outside the intended map boundaries.
        */
        cam.orthographicSize -= scroll * zoomSpeed * 0.01f;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        // Obtain the current camera position so that we can modify
        // and reassign the new y axis value after adjusting the zoom behaviour.
        Vector3 pos = transform.position;

        // Update the y position of the camera to ensure that it remains anchored to the ground when zooming in 
        // and out.
        // This is because for an orthographic camera in unity, the bottom edge of the visible screen is 
        // calculated as: cameraY - orthographicSize
        //
        // As such, to counter balance the change in ortogoraphicSize value when it grow bigger or smaller, 
        // it based value must be mimimumal at the ground level, ensuring we wont go below that value
        // Therefore, by setting: cameraY = groundY + orthographicSize
        // we enforce that zooming out only reveals more area upwards instead of exposing the underground areas.
        pos.y = groundY + cam.orthographicSize;

        // We calculate the camera's width and height based on its orthographic size and aspect ratio.
        // Note that the cam.orthographicSize represents half of the camera's height in world units.
        float camHeight = cam.orthographicSize;
        // The camera width should be from the minX value to the middle of the camera view.
        float camWidth = camHeight * cam.aspect;

        // Same idea form the Movement() method, we need to ensure that the camera does not move 
        // outside the boundaries of the background after zooming in and out.
        pos.x = Mathf.Clamp(pos.x, minX + camWidth, maxX - camWidth);

        pos.z = fixedZ;

        transform.position = pos;
    }
}