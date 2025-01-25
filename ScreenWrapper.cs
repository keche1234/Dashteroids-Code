using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
    //public float maxX = 9.5f;
    //public float maxY = 5.5f;
    private bool hasRb;
    private Rigidbody2D rb;

    //0.1 of the view size
    public float baseMargin;

    // TODO: Try out speed dependent wrapping?


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
            hasRb = true;
    }

    // Update is called once per frame
    void Update()
    {
        float xMargin = baseMargin + (hasRb ? Mathf.Abs(rb.linearVelocity.x) * 0.1f : 0.0f);
        float yMargin = baseMargin + (hasRb ? Mathf.Abs(rb.linearVelocity.y) * 0.1f : 0.0f);

        //this is the width of the screen in world units (it depends on the camera settings)
        //add a margin so the wrapping area is slightly larger than the camera view and the asteroids
        //exit the screen before teleporting on the other side
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float screenHeight = Camera.main.orthographicSize * 2;

        //i can't assign a vector component to a transform directly so I use a temporary variable
        //even if most of the times won't be changes
        Vector2 newPosition = transform.position;

        //check all the margin 
        if (transform.position.x > (screenWidth + xMargin) / 2)
        {
            newPosition.x = -(screenWidth + baseMargin)/ 2;
        }

        if (transform.position.x < -(screenWidth + xMargin) / 2)
        {
            newPosition.x = (screenWidth + baseMargin) / 2;
        }

        if (transform.position.y > (screenHeight + yMargin) / 2)
        {
            newPosition.y = -(screenHeight + baseMargin)/ 2;
        }

        if (transform.position.y < -(screenHeight + yMargin) / 2)
        {
            newPosition.y = (screenHeight + baseMargin) / 2;
        }

        //assign it to the transform
        transform.position = newPosition;
    }

    public void SetBaseMargin(float bm)
    {
        baseMargin = bm;
    }
}
