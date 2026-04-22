using System;
using System.Collections.Generic;
using UnityEngine;

public class PixelBlockController : MonoBehaviour
{
    public struct PixelTransferData
    {
        public int x;
        public int y;
        public float health;
        public GameObject pixel;

        public PixelTransferData(int x, int y, float health, GameObject pixel)
        {
            this.x = x;
            this.y = y;
            this.health = health;
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
    private Sprite sprite = null;
    private int width = 5;
    private int height = 5;
    private float[,] healthGrid = null;
    private GameObject[,] pixelObjects = null;
    private int[,] visitedStamp = null;
    private int visitToken = 0;
    private readonly Queue<Vector2Int> floodFillQueue = new Queue<Vector2Int>();
    private int activePixelCount = 0;
    private float initalPixelHealth = 100f;
    private Rigidbody2D pixelBlockRb = null;

    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0)
    };
    void Start()
    {
        pixelBlockRb = GetComponent<Rigidbody2D>();
    }

    public void ConfigBlock(BlockData data)
    {
        if (data == null)
        {
            Debug.LogError("BlockData is null. Cannot configure block.");
            return;
        }

        sprite = data.sprite;
        initalPixelHealth = data.pixelHealth;
    }

    public void Initiate()
    {
        width = Mathf.RoundToInt(sprite.rect.width);
        height = Mathf.RoundToInt(sprite.rect.height);

        healthGrid = new float[width, height];
        pixelObjects = new GameObject[width, height];
        visitedStamp = new int[width, height];
        visitToken = 0;
        activePixelCount = 0;

        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        int startX = Mathf.RoundToInt(sprite.rect.x);
        int startY = Mathf.RoundToInt(sprite.rect.y);

        Texture2D tex = sprite.texture;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = tex.GetPixel(startX + x, startY + y);

                if (pixelColor.a > 0.1f)
                {
                    GameObject pixel = PoolManager.instance.GetAttachedPixel();
                    pixel.transform.SetParent(transform, false);

                    // float z = UnityEngine.Random.Range(-0.5f, 0.5f);

                    pixel.transform.localPosition = new Vector3(x - offsetX, y - offsetY);

                    Renderer rend = pixel.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        propBlock.SetColor("_BaseColor", pixelColor);
                        rend.SetPropertyBlock(propBlock);
                    }

                    pixelObjects[x, y] = pixel;
                    healthGrid[x, y] = initalPixelHealth;
                    activePixelCount++;
                }
                else
                {
                    healthGrid[x, y] = 0;
                    pixelObjects[x, y] = null;
                }
            }
        }
    }

    public void InitiateEmptyBlock(int w, int h)
    {
        width = w;
        height = h;

        // grid = new bool[width, height];
        healthGrid = new float[width, height];
        pixelObjects = new GameObject[width, height];
        visitedStamp = new int[width, height];
        visitToken = 0;
        activePixelCount = 0;
    }
    public void AddPixel(int x, int y, float health, GameObject pixel)
    {
        healthGrid[x, y] = health;
        activePixelCount++;
        pixelObjects[x, y] = pixel;

        pixel.transform.SetParent(transform, true);
        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        pixel.transform.localPosition = new Vector2(x - offsetX, y - offsetY);
    }

    public void HitAtPoint(Vector2 worldHitPoint, float damageRadius, int maxDamage, int minDamage)
    {
        Vector2 localPoint = transform.InverseTransformPoint(worldHitPoint);
        float offsetX = (width - 1f) / 2f;
        float offsetY = (height - 1f) / 2f;

        int x = Mathf.RoundToInt(localPoint.x + offsetX);
        int y = Mathf.RoundToInt(localPoint.y + offsetY);

        float radiusSquared = damageRadius * damageRadius;

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
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
                        float damage = Mathf.Lerp(maxDamage, minDamage, Mathf.Sqrt(sqrDistance) / damageRadius);

                        if (sqrDistance <= radiusSquared)
                        {
                            TakeDamage(i, j, damage);
                        }
                    }
                }
            }

        }
        CheckSplitChunks();

    }
    public void HitArea(Collider2D weaponCollider, float damage)
    {
        if (weaponCollider == null) return;

        Bounds damageBounds = weaponCollider.bounds;

        Vector2[] worldCorners = new Vector2[4]
        {
            new Vector2(damageBounds.min.x, damageBounds.min.y),
            new Vector2(damageBounds.min.x, damageBounds.max.y),
            new Vector2(damageBounds.max.x, damageBounds.min.y),
            new Vector2(damageBounds.max.x, damageBounds.max.y)
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
                if (healthGrid[x, y] <= 0) continue;

                Vector2 worldPos = transform.TransformPoint(new Vector2(x - offsetX, y - offsetY));

                if (weaponCollider.OverlapPoint(worldPos))
                {
                    TakeDamage(x, y, damage);
                }
            }
        }
        CheckSplitChunks();
    }

    private void TakeDamage(int x, int y, float damage)
    {
        if (healthGrid[x, y] > 0)
        {
            healthGrid[x, y] = Mathf.Max(healthGrid[x, y] - damage, 0);
            if (healthGrid[x, y] > 0)
            {
                return;
            }
        }

        GameObject pixel = pixelObjects[x, y];
        if (pixel == null)
        {
            return;
        }

        Vector3 worldPos = pixel.transform.position;

        pixel.transform.SetParent(null);
        pixelObjects[x, y] = null;


        GameObject detachedPixel = PoolManager.instance.GetDetachedPixel();
        Rigidbody2D pixelRb = detachedPixel.GetComponent<Rigidbody2D>();
        if (pixelBlockRb != null && pixelRb != null)        {
            pixelRb.linearVelocity = pixelBlockRb.linearVelocity;
            pixelRb.angularVelocity = pixelBlockRb.angularVelocity;
        }
        detachedPixel.transform.position = worldPos;
        Renderer rend = pixel.GetComponent<Renderer>();
        Renderer detachedRend = detachedPixel.GetComponent<Renderer>();
        if (rend != null && detachedRend != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(propBlock);
            Color originalColor = propBlock.GetColor("_BaseColor");
            Color detachedColor = originalColor * 0.8f;
            propBlock.SetColor("_BaseColor", detachedColor);
            detachedRend.SetPropertyBlock(propBlock);
        }

        PoolManager.instance.ReturnToPool(pixel, true);


        activePixelCount--;
        CheckEmpty();
    }

    private void CalculateTapArea(int hitx, int hity, float damageRadius, out int startX, out int endX, out int startY, out int endY)
    {
        startX = hitx - Mathf.FloorToInt(damageRadius);
        endX = hitx + Mathf.CeilToInt(damageRadius);
        startY = hity - Mathf.FloorToInt(damageRadius);
        endY = hity + Mathf.CeilToInt(damageRadius);
    }

    private void CheckSplitChunks()
    {
        EnsureVisitedStampBuffer();
        int currentVisitToken = BeginVisitPass();
        List<List<Vector2Int>> chunks = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (healthGrid[x, y] > 0 && visitedStamp[x, y] != currentVisitToken)
                {
                    List<Vector2Int> chunk = new List<Vector2Int>();
                    FloodFill(x, y, currentVisitToken, chunk);
                    chunks.Add(chunk);
                }
            }
        }
        if (chunks.Count > 1)
        {
            chunks.Sort((a, b) => b.Count.CompareTo(a.Count));
            for (int i = 1; i < chunks.Count; i++)
            {
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
                chunkData.pixels.Add(new PixelTransferData(newX, newY, healthGrid[pos.x, pos.y], pixel));
                pixelObjects[pos.x, pos.y] = null;
                healthGrid[pos.x, pos.y] = 0;
            }
        }

        return chunkData;
    }

    private int BeginVisitPass()
    {
        if (visitToken == int.MaxValue)
        {
            Array.Clear(visitedStamp, 0, visitedStamp.Length);
            visitToken = 1;
            return visitToken;
        }

        visitToken++;
        return visitToken;
    }

    private void EnsureVisitedStampBuffer()
    {
        if (visitedStamp == null || visitedStamp.GetLength(0) != width || visitedStamp.GetLength(1) != height)
        {
            visitedStamp = new int[width, height];
            visitToken = 0;
        }
    }

    private void FloodFill(int x, int y, int currentVisitToken, List<Vector2Int> chunk)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (visitedStamp[x, y] == currentVisitToken || healthGrid[x, y] <= 0) return;

        visitedStamp[x, y] = currentVisitToken;

        floodFillQueue.Clear();
        floodFillQueue.Enqueue(new Vector2Int(x, y));


        while (floodFillQueue.Count > 0)
        {
            Vector2Int current = floodFillQueue.Dequeue();

            chunk.Add(new Vector2Int(current.x, current.y));

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && visitedStamp[nx, ny] != currentVisitToken && healthGrid[nx, ny] > 0)
                {
                    visitedStamp[nx, ny] = currentVisitToken;
                    floodFillQueue.Enqueue(new Vector2Int(nx, ny));
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
