using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public GameObject pixelPrefab;
    public GameObject pixelPoolContainer;
    public int initialPoolSize = 100;

    public static PoolManager instance;
    private List<GameObject> pixelPool;
    void Awake()
    {
        instance = this;
        pixelPool = new List<GameObject>();
    }
    void Start()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pixelObj = Instantiate(pixelPrefab, pixelPoolContainer.transform);
            pixelObj.SetActive(false);
            pixelPool.Add(pixelObj);
        }   
    }
    public GameObject GetFromPool()
    {
        if (pixelPool.Count > 0)
        {
            GameObject obj = pixelPool[pixelPool.Count - 1];
            pixelPool.RemoveAt(pixelPool.Count - 1);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Optionally, you can choose to instantiate a new object if the pool is empty
            GameObject pixelObj = Instantiate(pixelPrefab);
            return pixelObj;
        }
    }
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(pixelPoolContainer.transform);
        obj.GetComponent<PixelController>()?.RevertState();
        pixelPool.Add(obj);
    }
}
