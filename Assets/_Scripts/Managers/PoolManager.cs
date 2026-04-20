using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public GameObject attachedPixelPrefab;
    public GameObject detachedPixelPrefab;
    public GameObject attachedPixelPoolContainer;
    public GameObject detachedPixelPoolContainer;
    public int initialPoolSize = 100;

    public static PoolManager instance;
    private List<GameObject> attachedPixelPool;
    private List<GameObject> detachedPixelPool;
    void Awake()
    {
        instance = this;
        attachedPixelPool = new List<GameObject>();
        detachedPixelPool = new List<GameObject>();
    }
    void Start()
    {
        if (attachedPixelPrefab == null || attachedPixelPoolContainer == null)
        {
            Debug.LogError("PoolManager is missing attachedPixelPrefab or attachedPixelPoolContainer.");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject attachedPixel = Instantiate(attachedPixelPrefab, attachedPixelPoolContainer.transform);
            attachedPixel.SetActive(false);
            attachedPixelPool.Add(attachedPixel);
            GameObject detachedPixel = Instantiate(detachedPixelPrefab, detachedPixelPoolContainer.transform);
            detachedPixel.SetActive(false);
            detachedPixelPool.Add(detachedPixel);
        }
    }
    public GameObject GetAttachedPixel()
    {
        foreach (GameObject pixel in attachedPixelPool)
        {
            if (!pixel.activeInHierarchy)
            {
                pixel.SetActive(true);
                return pixel;
            }
        }
        GameObject newPixel = Instantiate(attachedPixelPrefab, attachedPixelPoolContainer.transform);
        newPixel.SetActive(true);
        attachedPixelPool.Add(newPixel);
        return newPixel;
    }
    public GameObject GetDetachedPixel()
    {
        foreach (GameObject pixel in detachedPixelPool)
        {
            if (!pixel.activeInHierarchy)
            {
                pixel.SetActive(true);
                return pixel;
            }
        }
        GameObject newPixel = Instantiate(detachedPixelPrefab, detachedPixelPoolContainer.transform);
        newPixel.SetActive(true);
        detachedPixelPool.Add(newPixel);
        return newPixel;
    }
    public void ReturnToPool(GameObject pixel, bool isAttached)
    {
        if (isAttached)
        {
            if (attachedPixelPool.Contains(pixel))
            {
                pixel.SetActive(false);
                pixel.transform.position = attachedPixelPoolContainer.transform.position;
                pixel.transform.rotation = Quaternion.identity;
                pixel.transform.SetParent(attachedPixelPoolContainer.transform);
            }
            else
            {
                Debug.LogWarning("Returned attached pixel does not belong to the pool.");
                Destroy(pixel);
            }
        }
        else
        {
            if (detachedPixelPool.Contains(pixel))
            {
                pixel.SetActive(false);
                pixel.transform.position = detachedPixelPoolContainer.transform.position;
                pixel.transform.rotation = Quaternion.identity;
                pixel.transform.SetParent(detachedPixelPoolContainer.transform);
            }
            else
            {
                Debug.LogWarning("Returned detached pixel does not belong to the pool.");
                Destroy(pixel);
            }

        }
    }
}