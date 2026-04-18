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
                // float z = UnityEngine.Random.Range(-0.5f, 0.5f);
                pixel.transform.localPosition = new Vector3(x - offsetX, y - offsetY);
                pixelObjects[x, y] = pixel;
                grid[x, y] = true;
            }
        }
    }

    public void InitiateEmptyBlock(int w, int h)
    {
        width = w;
        height = h;

        grid = new bool[width, height];
        pixelObjects = new GameObject[width, height];
    }

    public void AddPixel(int x, int y, GameObject pixel)
    {
        grid[x, y] = true;
        pixelObjects[x, y] = pixel;

        pixel.transform.SetParent(transform);
        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        pixel.transform.localPosition = new Vector2(x - offsetX, y - offsetY);
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

        CheckSplitChunks();
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
            chunks.Sort((a, b) => b.Count.CompareTo(a.Count));
            for (int i = 1; i < chunks.Count; i++)
            {
                CreateChunk(chunks[i]);
            }
        }
    }

    private void CreateChunk(List<Vector2Int> chunkPixels)
    {
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (var pos in chunkPixels)
        {
            minX = Math.Min(minX, pos.x);
            maxX = Math.Max(maxX, pos.x);
            minY = Math.Min(minY, pos.y);
            maxY = Math.Max(maxY, pos.y);
        }
        
        float oldOffSetX = (width - 1f) / 2f;
        float oldOffSetY = (height - 1f) / 2f;

        float chunkCenterX = (minX + maxX) / 2f;
        float chunkCenterY = (minY + maxY) / 2f;

        Vector2 chunkCenterLocalPos = new Vector2(chunkCenterX - oldOffSetX, chunkCenterY - oldOffSetY);
        Vector2 chunkCenterWorldPos = transform.TransformPoint(chunkCenterLocalPos);

        int newWidth = maxX - minX + 1;
        int newHeight = maxY - minY + 1;

        GameObject chunkObj = Instantiate(chunkPrefab, chunkCenterWorldPos, Quaternion.identity);
        chunkObj.GetComponent<PixelBlockManager>().InitiateEmptyBlock(newWidth, newHeight);

        foreach (var pos in chunkPixels)
        {
            GameObject pixel = pixelObjects[pos.x, pos.y];
            if (pixel != null)
            {
                int newX = pos.x - minX;
                int newY = pos.y - minY;
                chunkObj.GetComponent<PixelBlockManager>().AddPixel(newX, newY, pixel);
                pixelObjects[pos.x, pos.y] = null;
            }
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
