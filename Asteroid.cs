using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private ObjectPool asteroidPool;

    [SerializeField] protected int level; // When breaking, send out two asteroids of size - 1.
    protected static float baseScore = 100;

    private Rigidbody2D asteroidRb;

    private static float baseSize = 1f;
    private static float totalSize;
    private static float spawnMercyTime = 2.0f; // asteroids need to be in play on screen for this amount of time before destroying the player
    private static float spawnTimer = 0.0f; // how long has the asteroid been in play?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        asteroidRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimer > 0 && IsOnScreen())
            spawnTimer -= Time.deltaTime;
        else if (!IsOnScreen())
            spawnTimer = spawnMercyTime;
    }

    public void SetLevel(int s)
    {
        totalSize -= baseSize * Mathf.Pow(2.0f, level-1);
        level = s;
        totalSize += baseSize * Mathf.Pow(2.0f, level - 1);
    }

    public static float GetBaseSize()
    {
        return baseSize;
    }

    public static float GetTotalSize()
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
                    child.GetComponent<Asteroid>().EndMercyTime();

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

    public bool IsOnScreen()
    {
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float screenHeight = Camera.main.orthographicSize * 2;

        float halfSize = Mathf.Pow(baseSize, level)/2;

        return (Mathf.Abs(transform.position.x) <= (screenWidth / 2) + halfSize || Mathf.Abs(transform.position.y) <= ((screenWidth/2) + halfSize));
    }

    public void EndMercyTime()
    {
        spawnTimer = 0;
    }

    public bool CanDestroyPlayer()
    {
        return spawnTimer <= 0;
    }

    public float GetScoreValue()
    {
        return baseScore * Mathf.Pow(2.0f, level-1);
    }

    public void OnEnable()
    {
        totalSize += baseSize * Mathf.Pow(2.0f, level-1);
        spawnTimer = spawnMercyTime;
    }

    public void OnDisable()
    {
        totalSize -= baseSize * Mathf.Pow(2.0f, level-1);
        if (asteroidRb)
            asteroidRb.AddForce(-transform.up * asteroidRb.linearVelocity.magnitude, ForceMode2D.Impulse);
    }
}
