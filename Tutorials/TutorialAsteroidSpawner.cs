using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAsteroidSpawner : MonoBehaviour
{
    [SerializeField] protected List<Vector3> asteroidSpawnPositions;
    [SerializeField] protected float spawnDelay;
    [SerializeField] protected float spaceNeeded; // will not spawn an asteroid unless the player is this far away from the spawn point
    [SerializeField] protected GameObject myShip;
    [SerializeField] protected TutorialAsteroidChecker checker;

    protected ObjectPool pool;
    protected Asteroid currentAsteroid;

    protected Coroutine spawningCoroutine = null;
    [SerializeField] protected bool pauseSpawning;
    void Awake()
    {
        pool = GameObject.Find("Asteroid Pool").GetComponent<ObjectPool>();
        if (checker)
            checker.AddSpawner(this);
        SpawnAsteroidAtPositionIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseSpawning)
        {
            // I have an asteroid, there should be no spawn coroutine running
            if (currentAsteroid)
            {
                spawningCoroutine = null;
                if (!currentAsteroid.gameObject.activeSelf) // ...but the asteroid was "destroyed"
                {
                    currentAsteroid = null;
                    checker?.AsteroidDestroyed(this);
                }
            }

            // No asteroid, or the asteroid was destroyed, and I haven't started spawning yet
            if ((!currentAsteroid || !currentAsteroid.gameObject.activeSelf) && spawningCoroutine == null)
            {
                currentAsteroid = null;
                spawningCoroutine = StartCoroutine(AsteroidDelayCoroutine());
            }
        }
    }

    public void SetChecker(TutorialAsteroidChecker c)
    {
        checker?.RemoveSpawner(this);
        checker = c;
        checker?.AddSpawner(this);
    }

    public void SetShip(GameObject ship)
    {
        myShip = ship;
    }

    // Adds positions that the spawner could spawn an asteroid
    public void AddSpawnPosition(Vector3 spawnPosition)
    {
        if (asteroidSpawnPositions == null)
            asteroidSpawnPositions = new List<Vector3>();
        asteroidSpawnPositions.Add(spawnPosition);
    }

    public void SpawnAsteroidAtPositionIndex(int i)
    {
        if (asteroidSpawnPositions == null || asteroidSpawnPositions.Count <= 0 || myShip == null)
            return;

        if (i < 0 || i >= asteroidSpawnPositions.Count)
            return;

        Vector3 position = asteroidSpawnPositions[i];
        if (myShip.transform.position == position || (myShip.transform.position - position).magnitude < spaceNeeded)
            return;
        SpawnAsteroidAtPosition(position);
    }

    public void SpawnAsteroidAtRandomPosition()
    {
        if (asteroidSpawnPositions == null || asteroidSpawnPositions.Count <= 0 || myShip == null)
            return;

        Vector3 position = asteroidSpawnPositions[Random.Range(0, asteroidSpawnPositions.Count)];
        if (myShip.transform.position == position || (myShip.transform.position - position).magnitude < spaceNeeded)
            return;

        SpawnAsteroidAtPosition(position);
    }

    protected void SpawnAsteroidAtPosition(Vector3 position)
    {
        GameObject asteroid;
        asteroid = pool.GetPooledObject();
        if (asteroid == null)
            return;

        asteroid.SetActive(true);
        currentAsteroid = asteroid.GetComponent<Asteroid>();
        currentAsteroid.SetLevel(1);

        asteroid.transform.localScale = Vector3.one;
        asteroid.transform.rotation = Quaternion.Euler(0, 0, 0);
        asteroid.transform.position = position;

        Rigidbody2D asteroidRb = asteroid.GetComponent<Rigidbody2D>();
        asteroidRb.angularVelocity = 180f;
    }

    public void SetSpawnDelay(float t)
    {
        spawnDelay = t;
    }

    public bool SpawnAsteroidWithDelay()
    {
        if (spawningCoroutine != null)
            return false;
        spawningCoroutine = StartCoroutine(AsteroidDelayCoroutine());
        return true;
    }

    // Spawns an asteroid with this script's delay
    protected IEnumerator AsteroidDelayCoroutine()
    {
        if (asteroidSpawnPositions == null || asteroidSpawnPositions.Count <= 0 || myShip == null)
            yield return null;
        else
        {
            GameObject asteroid;
            Vector3 position;

            do
            {
                // increment time
                for (float t = 0; t < spawnDelay; t += Time.deltaTime)
                    yield return null;

                // select position
                position = asteroidSpawnPositions[Random.Range(0, asteroidSpawnPositions.Count)];

                // wait for player to leave position
                while (myShip.transform.position == position || (myShip.transform.position - position).magnitude < spaceNeeded)
                    yield return null;

                asteroid = pool.GetPooledObject();
            } while (asteroid == null);
            asteroid.SetActive(true);
            currentAsteroid = asteroid.GetComponent<Asteroid>();
            currentAsteroid.SetLevel(1);

            asteroid.transform.localScale = Vector3.one;
            asteroid.transform.rotation = Quaternion.Euler(0, 0, 0);
            asteroid.transform.position = position;

            Rigidbody2D asteroidRb = asteroid.GetComponent<Rigidbody2D>();
            asteroidRb.angularVelocity = 180f;

            yield return null;
        }
    }

    public void PauseSpawning()
    {
        pauseSpawning = true;
    }

    public void ResumeSpawning()
    {
        pauseSpawning = false;
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
