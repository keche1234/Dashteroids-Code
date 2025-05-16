using UnityEngine;

public class ScreenBounder : MonoBehaviour
{
    [SerializeField] protected float margin;

    // Update is called once per frame
    void Update()
    {
        float xMargin = margin;
        float yMargin = margin;

        //this is the width of the screen in world units (it depends on the camera settings)
        //add a margin so the wrapping area is slightly larger than the camera view and the asteroids
        //exit the screen before teleporting on the other side
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float screenHeight = Camera.main.orthographicSize * 2;

        //I can't assign a vector component to a transform directly so I use a temporary variable
        //even if most of the times won't be changes
        Vector2 newPosition = transform.position;

        //check all the margin 
        if (transform.position.x > (screenWidth - xMargin) / 2)
        {
            newPosition.x = (screenWidth - margin) / 2;
        }

        if (transform.position.x < -(screenWidth - xMargin) / 2)
        {
            newPosition.x = -(screenWidth - margin) / 2;
        }

        if (transform.position.y > (screenHeight - yMargin) / 2)
        {
            newPosition.y = (screenHeight - margin) / 2;
        }

        if (transform.position.y < -(screenHeight - yMargin) / 2)
        {
            newPosition.y = -(screenHeight - margin) / 2;
        }

        //assign it to the transform
        transform.position = newPosition;
    }

    public float GetMargin()
    {
        return margin;
    }
}
