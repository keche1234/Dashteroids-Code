using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] protected GameObject target;
    [SerializeField] protected Vector2 offset;

    [SerializeField] protected bool bounding;
    [SerializeField] protected float xMargin;
    [SerializeField] protected float yMargin;
    [SerializeField] protected float pushX;
    [SerializeField] protected float pushY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target)
        {
            transform.position = (Vector2)target.transform.position + offset;
            transform.rotation = Quaternion.Euler(0, 0, 0);

            if (bounding)
            {
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
                    newPosition.x = (screenWidth - pushX) / 2;
                }

                if (transform.position.x < -(screenWidth - xMargin) / 2)
                {
                    newPosition.x = -(screenWidth - pushX) / 2;
                }

                if (transform.position.y > (screenHeight - yMargin) / 2)
                {
                    newPosition.y = (screenHeight - pushY) / 2;
                }

                if (transform.position.y < -(screenHeight - yMargin) / 2)
                {
                    newPosition.y = -(screenHeight - pushY) / 2;
                }

                transform.position = newPosition;
            }
        }
    }
}
