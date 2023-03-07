using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int poolAmount;

    void Start()
    {
        if (objectToPool != null)
        {
            pooledObjects = new List<GameObject>();
            GameObject tmp;
            for (int i = 0; i < poolAmount; i++)
            {
                tmp = Instantiate(objectToPool);
                tmp.SetActive(false);
                pooledObjects.Add(tmp);
                tmp.transform.SetParent(transform);
            }
        }
    }
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < poolAmount; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        print(gameObject.name + " empty!");
        return null;
    }

    public void SetAllDisabled()
    {
        foreach (GameObject obj in pooledObjects) obj.SetActive(false);
    }
}
