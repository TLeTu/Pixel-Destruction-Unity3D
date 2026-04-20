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
}
