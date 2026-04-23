using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public GameObject pixelBlockPrefab;
    public LevelConfig levelConfig;
    public GameObject spawnPoint;
    public float spawnTime = 2f;
    public int pixelSpawned = 300;
    private readonly HashSet<PixelBlockController> registeredBlocks = new HashSet<PixelBlockController>();
    private float spawnTimer = 0f;
    private bool isSpawning = false;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }

    void Update()
    {
        // Call spawn block every spawnTime seconds
        if (levelConfig == null || isSpawning == false)
        {
            return;
        }

        if (pixelSpawned > 0 && PoolManager.instance != null && PoolManager.instance.ActiveSpawnedPixelCount >= pixelSpawned)
        {
            spawnTimer = 0f;
            return;
        }

        SpawnBlock();
    }
    public void LoadLevel(LevelConfig config)
    {
        // LoadLevel only sets up data for a new level.
        // Cleanup of old blocks is handled by CleanUpLevel.
        levelConfig = config;
        spawnTimer = 0f;
        isSpawning = false;
    }
    public void CleanUpLevel()
    {
        // CleanUpLevel tears down all spawned runtime content from the active level.
        levelConfig = null;
        isSpawning = false;
        spawnTimer = 0f;
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                block.OnChunkCreated -= HandleBlockCreated;
                block.OnBlockDestroyed -= HandleBlockDestroyed;
                Destroy(block.gameObject);
            }
        }
        registeredBlocks.Clear();
    }
    public void PauseLevel(bool shouldPause)
    {
        isSpawning = !shouldPause;
        Debug.Log($"isSpawning = " + isSpawning);
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                block.PauseBlock(shouldPause);
            }
        }
    }
    private void SpawnBlock()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            BlockData blockData = levelConfig.blocksToSpawn[Random.Range(0, levelConfig.blocksToSpawn.Count)];
            GameObject blockObj = Instantiate(pixelBlockPrefab, spawnPoint.transform.position, Quaternion.identity);
            PixelBlockController blockController = blockObj.GetComponent<PixelBlockController>();
            if (blockController == null)
            {
                Debug.LogError("pixelBlockPrefab is missing PixelBlockController component.");
                Destroy(blockObj);
                return;
            }
            RegisterBlock(blockController);
            blockController.ConfigBlock(blockData);
            blockController.Initiate();
        }
    }
    private void RegisterBlock(PixelBlockController block)
    {
        if (block == null || registeredBlocks.Contains(block))
        {
            return;
        }

        block.OnChunkCreated += HandleBlockCreated;
        block.OnBlockDestroyed += HandleBlockDestroyed;
        registeredBlocks.Add(block);
    }

    private void HandleBlockCreated(PixelBlockController sourceBlock, List<Vector2Int> chunkPixels)
    {
        if (sourceBlock == null || chunkPixels == null || chunkPixels.Count == 0)
        {
            return;
        }

        PixelBlockController.ChunkTransferData transferData = sourceBlock.DetachChunk(chunkPixels);
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

        PixelBlockController chunkBlock = chunkObj.GetComponent<PixelBlockController>();
        if (chunkBlock == null)
        {
            Debug.LogError("pixelBlockPrefab is missing PixelBlockController component.");
            Destroy(chunkObj);
            return;
        }

        RegisterBlock(chunkBlock);
        chunkBlock.InitiateEmptyBlock(transferData.width, transferData.height);

        foreach (var pixelData in transferData.pixels)
        {
            chunkBlock.AddPixel(pixelData.x, pixelData.y, pixelData.health, pixelData.pixel);
        }
    }

    private void HandleBlockDestroyed(PixelBlockController destroyedBlock)
    {
        registeredBlocks.Remove(destroyedBlock);
        destroyedBlock.OnChunkCreated -= HandleBlockCreated;
        destroyedBlock.OnBlockDestroyed -= HandleBlockDestroyed;
    }

}
