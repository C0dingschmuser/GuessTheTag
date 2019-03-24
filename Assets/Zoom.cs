using UnityEngine;

public class Zoom : MonoBehaviour
{
    public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
    public Camera cameraObj;
    public GameObject cameraGameObj;
    bool dragAllowed = true;
    float offsetX, offsetY;

    public void BeginDrag()
    {
        Vector3 mp = Input.mousePosition;
        mp = cameraObj.ScreenToWorldPoint(mp);
        offsetX = transform.position.x - mp.x;
        offsetY = transform.position.y - mp.y;
    }

    public void OnDrag()
    {
        if(!dragAllowed)
        {
            return;
        }

        Vector3 mp = Input.mousePosition;
        mp = cameraObj.ScreenToWorldPoint(mp);

        float newX = mp.x + offsetX;
        float newY = mp.y + offsetY;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    void Update()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            dragAllowed = false;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // If the camera is orthographic...
            if (cameraObj.orthographic)
            {
                // ... change the orthographic size based on the change in distance between the touches.
                cameraObj.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Make sure the orthographic size never drops below zero.
                cameraObj.orthographicSize = Mathf.Max(cameraObj.orthographicSize, 0.1f);
            }
        } else
        { //drag
            dragAllowed = true;
        }
    }
}
   