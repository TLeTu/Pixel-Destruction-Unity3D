using System;
using System.Collections.Generic;
using UnityEngine;

public class PixelBlockManager : MonoBehaviour
{
    public GameObject pixelPrefab;
    public GameObject chunkPrefab;
    private int width;
    private int height;
    private bool[,] grid;
    private GameObject[,] pixelObjects;
    private void Start()
    {

    }

    private void Update()
    {

    }

    public void Initialize(int w, int h)
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
                float z = UnityEngine.Random.Range(-0.5f, 0.5f);
                pixel.transform.localPosition = new Vector3(x - offsetX, y - offsetY, z);
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
        // Change the material color to red to indicate damage
        Renderer rend = pixel.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.red;
        }
        // Scale down the pixel
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

    private void CheckSplitChunks()
    {
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> chunks = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] && !visited[x, y])
                {
                    List<Vector2Int> chunk = new List<Vector2Int>();
                    FloodFill(x, y, visited, chunk);
                    chunks.Add(chunk);
                }
            }
        }
        if (chunks.Count > 1)
        {
            // Later
        }
    }

    private void FloodFill(int x, int y, bool[,] visited, List<Vector2Int> chunk)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (visited[x, y] || !grid[x, y]) return;

        visited[x, y] = true;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));

        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            chunk.Add(new Vector2Int(current.x, current.y));

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny] && grid[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }
}
