using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    //public static ObjectPool SharedInstance;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;
    private void Awake()
    {
        //SharedInstance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            tmp.transform.parent = transform;

            pooledObjects.Add(tmp);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }

    public int GetCapacity()
    {
        return amountToPool;
    }

    public int GetStoredObjectCount()
    {
        int storage = 0;
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                storage++;
            }
        }
        return storage;
    }

    public int GetActiveObjectCount()
    {
        return GetCapacity() - GetStoredObjectCount();
    }
}
