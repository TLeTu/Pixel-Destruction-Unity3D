using UnityEngine;

public class PixelObjectManager : MonoBehaviour
{
    public GameObject pixelPrefab;
    private int width;
    private int height;
    private bool[,] grid;
    private GameObject[,] pixelObjects;
    private void Start()
    {
        Initialize(10, 10);
    }

    private void Update()
    {
        
    }

    private void Initialize(int w, int h)
    {
        width = w;
        height = h;
        grid = new bool[width, height];
        pixelObjects = new GameObject[width, height];
        float offsetX = (width - 1) / 2f;
        float offsetY = (height - 1) / 2f;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x - offsetX, y - offsetY, 0);
                GameObject pixel = Instantiate(pixelPrefab, Vector3.zero, Quaternion.identity, transform);
                pixel.transform.SetParent(transform, false);
                pixel.transform.localPosition = position;
                pixelObjects[x, y] = pixel;
                grid[x, y] = true;
            }
        }
    }
}
