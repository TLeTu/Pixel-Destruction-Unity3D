using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PixelBlockController : MonoBehaviour
{
    public GameObject pixelPrefab;
    public float damageRadius = 5;    
    public int maxDamage = 3;
    public int minDamage = 1;
    private int width;
    private int height;
    private bool[,] grid;
    private GameObject[,] pixelObjects;
    private int activePixelCount = 0;

    static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(1, 0)    // Right
    };

    public struct PixelTransferData
    {
        public int x;
        public int y;
        public GameObject pixel;

        public PixelTransferData(int x, int y, GameObject pixel)
        {
            this.x = x;
            this.y = y;
            this.pixel = pixel;
        }
    }

    public class ChunkTransferData
    {
        public Vector2 worldCenter;
        public int width;
        public int height;
        public List<PixelTransferData> pixels;

        public ChunkTransferData()
        {
            pixels = new List<PixelTransferData>();
        }
    }

    public event Action<PixelBlockController, List<Vector2Int>> OnChunkCreated;
    public event Action<PixelBlockController> OnBlockDestroyed;

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
                GameObject pixel = PoolManager.instance.GetFromPool();
                pixel.GetComponent<PixelController>()?.Attach();
                pixel.transform.SetParent(transform, false);
                // float z = UnityEngine.Random.Range(-0.5f, 0.5f);
                pixel.transform.localPosition = new Vector3(x - offsetX, y - offsetY);
                pixelObjects[x, y] = pixel;
                grid[x, y] = true;
            }
        }

        activePixelCount = width * height;
    }

    public void InitiateEmptyBlock(int w, int h)
    {
        width = w;
        height = h;

        grid = new bool[width, height];
        pixelObjects = new GameObject[width, height];
        activePixelCount = 0;
    }

    public void ConfigBlock(float damageRadius, int maxDamage, int minDamage)
    {
        this.damageRadius = damageRadius;
        this.maxDamage = maxDamage;
        this.minDamage = minDamage;
    }

    public void AddPixel(int x, int y, GameObject pixel)
    {
        grid[x, y] = true;
        activePixelCount++;
        pixelObjects[x, y] = pixel;

        pixel.transform.SetParent(transform, true);
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

        float radiusSquared = damageRadius * damageRadius;

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            Debug.Log($"Hit at local grid position: ({x}, {y})");
            CalculateTapArea(x, y, damageRadius, out int startX, out int endX, out int startY, out int endY);
            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    if (i >= 0 && i < width && j >= 0 && j < height)
                    {
                        int distX = i - x;
                        int distY = j - y;
                        float sqrDistance = distX * distX + distY * distY;

                        if (sqrDistance <= radiusSquared)
                        {
                            TakeDamage(i, j);
                        }
                    }
                }
            }

            CheckSplitChunks();
        }
    }

    public void HitArea(Bounds shredderBounds)
    {
        Vector2[] worldCorners = new Vector2[4]
        {
        new Vector2(shredderBounds.min.x, shredderBounds.min.y),
        new Vector2(shredderBounds.min.x, shredderBounds.max.y),
        new Vector2(shredderBounds.max.x, shredderBounds.min.y),
        new Vector2(shredderBounds.max.x, shredderBounds.max.y)
        };

        float minLocalX = float.PositiveInfinity;
        float maxLocalX = float.NegativeInfinity;
        float minLocalY = float.PositiveInfinity;
        float maxLocalY = float.NegativeInfinity;

        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector2 local = transform.InverseTransformPoint(worldCorners[i]);

            if (local.x < minLocalX) minLocalX = local.x;
            if (local.x > maxLocalX) maxLocalX = local.x;
            if (local.y < minLocalY) minLocalY = local.y;
            if (local.y > maxLocalY) maxLocalY = local.y;
        }

        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        int startX = Mathf.FloorToInt(minLocalX + offsetX);
        int endX = Mathf.CeilToInt(maxLocalX + offsetX);
        int startY = Mathf.FloorToInt(minLocalY + offsetY);
        int endY = Mathf.CeilToInt(maxLocalY + offsetY);

        startX = Mathf.Clamp(startX, 0, width - 1);
        endX = Mathf.Clamp(endX, 0, width - 1);
        startY = Mathf.Clamp(startY, 0, height - 1);
        endY = Mathf.Clamp(endY, 0, height - 1);

        if (startX > endX || startY > endY)
        {
            return;
        }


        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (!grid[x, y]) continue;
                if (shredderBounds.Contains(transform.TransformPoint(new Vector2(x - offsetX, y - offsetY))))
                {
                    TakeDamage(x, y);
                }
            }
        }
        CheckSplitChunks();
    }

    private void TakeDamage(int x, int y)
    {
        grid[x, y] = false;
        GameObject pixel = pixelObjects[x, y];
        if (pixel == null)
        {
            return;
        }

        pixel.transform.SetParent(null);
        
        pixel.GetComponent<PixelController>()?.Detach();

        activePixelCount--;
        CheckEmpty();
    }

    private void CalculateTapArea(int hitx, int hity, float damageRadius, out int startX, out int endX, out int startY, out int endY)
    {
        startX = hitx - Mathf.FloorToInt(damageRadius);
        endX = hitx + Mathf.CeilToInt(damageRadius);
        startY = hity - Mathf.FloorToInt(damageRadius);
        endY = hity + Mathf.CeilToInt(damageRadius) ;
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
                Debug.Log($"Requesting chunk spawn with {chunks[i].Count} pixels");
                OnChunkCreated?.Invoke(this, chunks[i]);
                activePixelCount -= chunks[i].Count;
            }
        }
    }

    public ChunkTransferData DetachChunk(List<Vector2Int> chunkPixels)
    {
        ChunkTransferData chunkData = new ChunkTransferData();
        if (chunkPixels == null || chunkPixels.Count == 0)
        {
            return chunkData;
        }

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
        chunkData.worldCenter = transform.TransformPoint(chunkCenterLocalPos);

        chunkData.width = maxX - minX + 1;
        chunkData.height = maxY - minY + 1;

        foreach (var pos in chunkPixels)
        {
            GameObject pixel = pixelObjects[pos.x, pos.y];
            if (pixel != null)
            {
                int newX = pos.x - minX;
                int newY = pos.y - minY;
                chunkData.pixels.Add(new PixelTransferData(newX, newY, pixel));
                pixelObjects[pos.x, pos.y] = null;
                grid[pos.x, pos.y] = false;
            }
        }

        return chunkData;
    }

    private void FloodFill(int x, int y, bool[,] visited, List<Vector2Int> chunk)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (visited[x, y] || !grid[x, y]) return;

        visited[x, y] = true;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));


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

    private void CheckEmpty()
    {
        if (activePixelCount <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        OnBlockDestroyed?.Invoke(this);
    }
}
