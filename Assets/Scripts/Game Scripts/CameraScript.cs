using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public Transform CameraTarget;
    public float edgeScrollSpeed;

    private int lastViewingQuadrant = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCameraTargetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if QuadrantView has changed values, if so we adjust the camera accordingly
        if (GameManager.Instance != null && GameManager.Instance.ViewingQuadrant != lastViewingQuadrant)
        {
            UpdateCameraTargetPosition();
            lastViewingQuadrant = GameManager.Instance.ViewingQuadrant;
        }

        // Only do edge scrolling when we're viewing a quadrant
        if (GameManager.Instance != null && GameManager.Instance.ViewingQuadrant != 0)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 currentPosition = CameraTarget.position;
            bool inScrollableArea = mousePosition.y > 125.0f && mousePosition.y < Screen.height - 100.0f;

            // Only do edge-scrolling if the mouse is in the scrollable area
            if (inScrollableArea)
            {
                // Check to see if the mouse is at the left-most or right-most side of the screen
                if (mousePosition.x < 50.0f)
                {
                    currentPosition.x -= edgeScrollSpeed * Time.deltaTime;
                }
                else if (mousePosition.x > Screen.width - 50.0f)
                {
                    currentPosition.x += edgeScrollSpeed * Time.deltaTime;
                }
            }

            // Clamp the camera to the boundaries set below
            currentPosition.x = Mathf.Clamp(currentPosition.x, -50.0f, 50.0f);
            CameraTarget.position = currentPosition;
        }
    }

    // Set the camera target's initial position depending on whether the player is viewing a quadrant or not
    private void UpdateCameraTargetPosition()
    {
        if (GameManager.Instance == null || CameraTarget == null)
            return;

        if (GameManager.Instance.ViewingQuadrant == 0)
        {
            CameraTarget.position = new Vector3(0f, 5f, 0f);
        }
        else
        {
            CameraTarget.position = new Vector3(0f, 0f, 0f);
        }
    }
}