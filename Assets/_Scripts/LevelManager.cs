using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject pixelBlockPrefab;
    public LevelConfig levelConfig;
    public GameObject spawnPoint;
    public float spawnTime = 2f;
    private readonly HashSet<PixelBlockManager> registeredBlocks = new HashSet<PixelBlockManager>();
    private float spawnTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject pixelBlock = Instantiate(pixelBlockPrefab, transform.position, Quaternion.identity, transform);
        PixelBlockManager blockManager = pixelBlock.GetComponent<PixelBlockManager>();
        if (blockManager == null)
        {
            Debug.LogError("pixelBlockPrefab is missing PixelBlockManager component.");
            return;
        }
        pixelBlock.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        RegisterBlock(blockManager);
        blockManager.Initialize(10, 10);
    }

    // Update is called once per frame
    void Update()
    {
        // Call spawn block every spawnTime seconds
        if (levelConfig == null || levelConfig.blocksToSpawn.Count == 0)
        {
            return;
        }
        // SpawnBlock();
    }

    private void SpawnBlock()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            BlockData blockData = levelConfig.blocksToSpawn[Random.Range(0, levelConfig.blocksToSpawn.Count)];
            GameObject blockObj = Instantiate(pixelBlockPrefab, spawnPoint.transform.position, Quaternion.identity);
            PixelBlockManager blockManager = blockObj.GetComponent<PixelBlockManager>();
            if (blockManager == null)            {
                Debug.LogError("pixelBlockPrefab is missing PixelBlockManager component.");
                Destroy(blockObj);
                return;
            }
            RegisterBlock(blockManager);
            blockManager.Initialize(blockData.width, blockData.height);
        }
    }

    private void OnDestroy()
    {
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                block.OnChunkCreated -= HandleChunkCreated;
                block.OnBlockDestroyed -= HandleBlockDestroyed;
            }
        }

        registeredBlocks.Clear();
    }

    private void RegisterBlock(PixelBlockManager block)
    {
        if (block == null || registeredBlocks.Contains(block))
        {
            return;
        }

        block.OnChunkCreated += HandleChunkCreated;
        block.OnBlockDestroyed += HandleBlockDestroyed;
        registeredBlocks.Add(block);
    }

    private void HandleChunkCreated(PixelBlockManager sourceBlock, List<Vector2Int> chunkPixels)
    {
        if (sourceBlock == null || chunkPixels == null || chunkPixels.Count == 0)
        {
            return;
        }

        PixelBlockManager.ChunkTransferData transferData = sourceBlock.DetachChunk(chunkPixels);
        if (transferData.pixels.Count == 0)
        {
            return;
        }

        GameObject chunkObj = Instantiate(pixelBlockPrefab, transferData.worldCenter, sourceBlock.transform.rotation);
        Rigidbody2D sourceRb = sourceBlock.GetComponent<Rigidbody2D>();
        Rigidbody2D chunkRb = chunkObj.GetComponent<Rigidbody2D>();
        if (sourceRb != null && chunkRb != null)
        {
            chunkRb.linearVelocity = sourceRb.linearVelocity;
            chunkRb.angularVelocity = sourceRb.angularVelocity;
        }

        PixelBlockManager chunkBlock = chunkObj.GetComponent<PixelBlockManager>();
        if (chunkBlock == null)
        {
            Debug.LogError("pixelBlockPrefab is missing PixelBlockManager component.");
            Destroy(chunkObj);
            return;
        }

        RegisterBlock(chunkBlock);
        chunkBlock.InitiateEmptyBlock(transferData.width, transferData.height);

        foreach (var pixelData in transferData.pixels)
        {
            chunkBlock.AddPixel(pixelData.x, pixelData.y, pixelData.pixel);
        }
    }

    private void HandleBlockDestroyed(PixelBlockManager destroyedBlock)
    {
        registeredBlocks.Remove(destroyedBlock);
    }

}
