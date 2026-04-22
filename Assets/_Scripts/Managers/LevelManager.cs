using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public GameObject pixelBlockPrefab;
    public LevelConfig levelConfig;
    public GameObject spawnPoint;
    public float spawnTime = 2f;
    private readonly HashSet<PixelBlockController> registeredBlocks = new HashSet<PixelBlockController>();
    private float spawnTimer = 0f;
    private bool isSpawning = false;
    private bool levelFinbished = false;
    private int blockSpawned = 0;
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
        if (levelConfig == null || levelConfig.blocksToSpawn.Count == 0 || isSpawning == false || levelFinbished)
        {
            if (blockSpawned >= levelConfig.targetDestroyCount && registeredBlocks.Count == 0 && !levelFinbished)
            {
                levelFinbished = true;
                Debug.Log("Level Completed!");
                GameManager.instance.SetGameState(GameState.GameWin);

            }
            return;
        }
        SpawnBlock();
        if (blockSpawned >= levelConfig.targetDestroyCount)
        {
            isSpawning = false;
        }
    }
    public void LoadLevel(LevelConfig config)
    {
        levelConfig = config;
        InputManager.instance.SetTapDamage(levelConfig.damageRadius, levelConfig.maxTapDamage, levelConfig.minTapDamage);
        spawnTimer = 0f;
        blockSpawned = 0;
        isSpawning = true;
        levelFinbished = false;
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
    }
    public void CleanUpLevel()
    {
        levelConfig = null;
        isSpawning = false;
        blockSpawned = 0;
        spawnTimer = 0f;
        levelFinbished = false;
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        registeredBlocks.Clear();
    }
    public void PauseLevel()
    {
        isSpawning = false;
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                block.PauseBlock();
            }
        }
    }
    private void SpawnBlock()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            blockSpawned++;
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
