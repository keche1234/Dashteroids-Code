using UnityEngine;

public class SubMenu : MonoBehaviour
{
    [SerializeField] protected GameObject firstOption;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject GetFirstOption()
    {
        return firstOption;
    }
}
