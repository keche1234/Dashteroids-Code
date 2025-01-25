using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("triggercollide");
        GameObject target = collision.gameObject;
        Asteroid asteroid = target.GetComponent<Asteroid>();
        if (asteroid)
        {
            Vector2 normal = (transform.position - target.transform.position).normalized;
            asteroid.BreakAsteroid(normal);
            gameObject.SetActive(false);
        }
    }
}
