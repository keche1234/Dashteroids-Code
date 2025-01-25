using UnityEngine;

public class DespawnTimer : MonoBehaviour
{
    public float time;

    private float timer = 0.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time)
            gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        timer = 0.0f;
    }
}
