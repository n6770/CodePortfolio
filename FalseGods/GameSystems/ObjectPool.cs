using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector] public List<GameObject> pooledObjects;
    [SerializeField] private GameObject poolPrefab;
    [SerializeField] private int poolAmount;

    private void Awake()
    {
        pooledObjects = new List<GameObject>();
        StartCoroutine(FillPool());
    }

    private IEnumerator FillPool()
    {
        yield return null;
        GameObject tmpGO;
        tmpGO = Instantiate(poolPrefab);
        tmpGO.SetActive(false);
        tmpGO.transform.SetParent(transform);
        pooledObjects.Add(tmpGO);
        if (pooledObjects.Count < poolAmount) StartCoroutine(FillPool());
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
        return null;
    }
}
