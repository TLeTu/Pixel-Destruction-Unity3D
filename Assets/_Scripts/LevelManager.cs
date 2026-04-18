using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject pixelBlockPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject pixelBlock = Instantiate(pixelBlockPrefab, transform);
        pixelBlock.GetComponent<PixelBlockManager>().Initialize(10, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
