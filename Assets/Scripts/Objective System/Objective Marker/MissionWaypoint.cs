using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWaypoint : MonoBehaviour
{
    // Indicator icon
    public Camera cam;
    public Image image;
    // The target (location, enemy, etc..)
    public Transform objectiveTransform;
    // UI Text to display the distance
    //public Text meter;
    // To adjust the position of the icon
    public Vector3 offset;

    private bool markerOn;

    private void Update()
    {
        if (markerOn)
        {
            if (!objectiveTransform)
            {
                Destroy(image.gameObject);
                Destroy(this);
                return;
            }

            // Giving limits to the icon so it sticks on the screen
            // Below calculations witht the assumption that the icon anchor point is in the middle
            // Minimum X position: half of the icon width
            float minX = image.GetPixelAdjustedRect().width / 2;
            // Maximum X position: screen width - half of the icon width
            float maxX = Screen.width - minX;

            // Minimum Y position: half of the height
            float minY = image.GetPixelAdjustedRect().height / 2;
            // Maximum Y position: screen height - half of the icon height
            float maxY = Screen.height - minY;

            // Temporary variable to store the converted position from 3D world point to 2D screen point
            Vector2 pos = cam.WorldToScreenPoint(objectiveTransform.position + offset);

            // Check if the target is behind us, to only show the icon once the target is in front
            if (Vector3.Dot((objectiveTransform.position - transform.position), transform.forward) < 0)
            {
                // Check if the target is on the left side of the screen
                if (pos.x < Screen.width / 2)
                {
                    // Place it on the right (Since it's behind the player, it's the opposite)
                    pos.x = maxX;
                }
                else
                {
                    // Place it on the left side
                    pos.x = minX;
                }
            }

            // Limit the X and Y positions
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            // Update the marker's position
            image.transform.position = pos;
            // Change the meter text to the distance with the meter unit 'm'
            //meter.text = ((int)Vector3.Distance(target.position, transform.position)).ToString() + "m";
        }
    }

    public void SetData(Camera targetCam, Image img, Transform target, Vector3 markerOffset)
    {
        cam = targetCam;
        image = img;
        objectiveTransform = target;
        offset = markerOffset;

        markerOn = true;
    }
}