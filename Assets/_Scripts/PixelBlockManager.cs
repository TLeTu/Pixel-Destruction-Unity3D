using System;
using UnityEngine;

public class PixelBlockManager : MonoBehaviour
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

    public void HitAtPoint(Vector2 worldHitPoint)
    {
        Vector2 localPoint = transform.InverseTransformPoint(worldHitPoint);
        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        int x = Mathf.RoundToInt(localPoint.x + offsetX);
        int y = Mathf.RoundToInt(localPoint.y + offsetY);
        
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            Debug.Log($"Hit at local grid position: ({x}, {y})");
            if (grid[x, y])
            {
                TakeDamage(x, y);
            }
        }
    }

    private void TakeDamage(int x, int y)
    {
        grid[x, y] = false;
        GameObject pixel = pixelObjects[x, y];
        if (pixel == null)
        {
            return;
        }
        // Change the material color to red to indicate damage, cube is 3d
        Renderer rend = pixel.GetComponent<Renderer>();
        if (rend != null)        {
            rend.material.color = Color.red;
        }
        pixel.transform.SetParent(null);
        BoxCollider2D col = pixel.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.compositeOperation = Collider2D.CompositeOperation.None;
        }
        Rigidbody2D rb = pixel.AddComponent<Rigidbody2D>();
        float randomX = UnityEngine.Random.Range(-2f, 2f); 
        rb.AddForce(new Vector2(randomX, 5f), ForceMode2D.Impulse);
    }
}
