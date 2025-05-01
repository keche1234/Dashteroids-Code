using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class UIActionBuffer : MonoBehaviour
{
    protected float timer = 0;
    [SerializeField] protected float delay = 1f/60f;
    [SerializeField] protected bool setSelectedOnEnable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    private void OnEnable()
    {
        GetComponent<EventTrigger>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < delay)
        {
            timer += Time.unscaledDeltaTime;
        }
        else
        {
            if (!GetComponent<EventTrigger>().enabled)
            {
                GetComponent<EventTrigger>().enabled = true;
                if (setSelectedOnEnable)
                {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
        }
    }
}
