using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    [SerializeField] protected GameObject target;
    [SerializeField] protected Vector2 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = (Vector2) target.transform.position + offset;
    }
}
