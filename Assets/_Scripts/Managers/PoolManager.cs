using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public GameObject pixelPrefab;
    public GameObject pixelPoolContainer;

    private List<GameObject> pixelPool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pixelPool = new List<GameObject>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
