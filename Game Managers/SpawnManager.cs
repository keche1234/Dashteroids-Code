using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public ObjectPool asteroidPool;
    public float asteroidSpeed;
    public float maxAsteroidSize;

    public float spawnTime;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float size = Asteroid.GetTotalSize();
        if (size < maxAsteroidSize && timer > spawnTime * (1.0f + (0.2f * size)))
        {
            SpawnAsteroid();
            timer -= spawnTime;// * (1.0f + (0.2f * size));
        }
        timer += Time.deltaTime;
    }

    public GameObject SpawnAsteroid()
    {
        GameObject newAsteroid = asteroidPool.GetPooledObject();
        if (newAsteroid)
        {
            newAsteroid.SetActive(true);
            int asteroidLevel = Random.Range(2, 4);
            float asteroidSize = Asteroid.GetBaseSize() * Mathf.Pow(2.0f, asteroidLevel - 1);

            // Transform initialization
            float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
            float screenHeight = Camera.main.orthographicSize * 2;
            newAsteroid.transform.position = new Vector2((screenWidth / 2) + asteroidSize + 1, (screenHeight / 2) + asteroidSize + 1);
            if (Random.Range(0, 1f) < 0.5f)
                newAsteroid.transform.position = new Vector3(newAsteroid.transform.position.x * -1, newAsteroid.transform.position.y);
            if (Random.Range(0, 1f) < 0.5f)
                newAsteroid.transform.position = new Vector3(newAsteroid.transform.position.x, newAsteroid.transform.position.y * -1);

            newAsteroid.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.zero - newAsteroid.transform.position);
            newAsteroid.transform.Rotate(0.0f, 0.0f, Random.Range(-30f, 30f));

            newAsteroid.transform.localScale = Vector3.one * asteroidSize;

            // Asteroid aspects
            newAsteroid.GetComponent<Asteroid>().SetLevel(asteroidLevel);
            newAsteroid.GetComponent<Asteroid>().SetAsteroidPool(asteroidPool);

            // Screen wrap aspects
            newAsteroid.GetComponent<ScreenWrapper>().SetBaseMargin(asteroidSize + 1);

            // Physics
            Rigidbody2D asteroidRb = newAsteroid.GetComponent<Rigidbody2D>();
            asteroidRb.AddForce(newAsteroid.transform.up * asteroidSpeed, ForceMode2D.Impulse);
        }
        return newAsteroid;
    }
}
