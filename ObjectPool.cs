using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    //public static ObjectPool SharedInstance;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //SharedInstance = this;
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

    /*
     * Attempts to retreive an object from the object pool.
     * Returns null if there is no object.
     */
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
