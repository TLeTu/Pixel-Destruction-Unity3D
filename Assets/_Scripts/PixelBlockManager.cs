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
        
        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject pixel = Instantiate(pixelPrefab, transform);
                pixel.transform.localPosition = new Vector2(x - offsetX, y - offsetY);
                pixelObjects[x, y] = pixel;
                grid[x, y] = true;
            }
        }
    }
}
