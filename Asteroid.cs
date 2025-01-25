using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private ObjectPool asteroidPool;
    [SerializeField] protected int level; // When breaking, send out two asteroids of size - 1.
    private Rigidbody2D asteroidRb;

    private static int baseSize = 2;
    private static int totalSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        asteroidRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(int s)
    {
        totalSize -= (int)Mathf.Pow(baseSize, level - 1);
        level = s;
        totalSize += (int)Mathf.Pow(baseSize, level - 1);
    }

    public static int GetTotalSize()
    {
        return totalSize;
    }

    public void SetAsteroidPool(ObjectPool pool)
    {
        asteroidPool = pool;
    }

    /*
     * Breaks an asteroid, and notes the `normal` on the surface of impact
     * to determine how to split the asteroid.
     */
    public void BreakAsteroid(Vector2 normal)
    {
        if (level > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject child = asteroidPool.GetPooledObject();

                // Transform
                if (child)
                {
                    child.SetActive(true);
                    child.transform.localScale = transform.localScale / 2;
                    child.transform.rotation = Quaternion.LookRotation(transform.forward, normal);
                    child.transform.Rotate(0, 0, 90f * ((i * 2) + 1));
                    child.transform.position = transform.position + (child.transform.up * transform.localScale.x / 4);

                    // Asteroid aspects
                    child.GetComponent<Asteroid>().SetLevel(level - 1);
                    child.GetComponent<Asteroid>().SetAsteroidPool(asteroidPool);

                    // Screen Warpper aspects
                    child.GetComponent<ScreenWrapper>().SetBaseMargin(child.transform.localScale.x + 1);

                    // Physics
                    Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
                    childRb.AddForce(child.transform.up * asteroidRb.linearVelocity.magnitude, ForceMode2D.Impulse);
                }
            }
        }

        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        totalSize += (int)Mathf.Pow(baseSize, level - 1);
    }

    public void OnDisable()
    {
        totalSize -= (int) Mathf.Pow(baseSize, level - 1);
        if (asteroidRb)
            asteroidRb.AddForce(-transform.up * asteroidRb.linearVelocity.magnitude, ForceMode2D.Impulse);
    }
}
