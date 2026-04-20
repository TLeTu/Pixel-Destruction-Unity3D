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
    private float damageRadius = 5f;
    private int maxTapDamage = 3;
    private int minTapDamage = 1;
    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.OnGameStarted += StartLevel;
        if (levelConfig != null)
        {
            damageRadius = levelConfig.damageRadius;
            maxTapDamage = levelConfig.maxTapDamage;
            minTapDamage = levelConfig.minTapDamage;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Call spawn block every spawnTime seconds
        if (levelConfig == null || levelConfig.blocksToSpawn.Count == 0 || isSpawning == false)
        {
            return;
        }
        SpawnBlock();
    }
    private void StartLevel()
    {
        isSpawning = true;
    }
    public void GetLevelTapDamage(out float radius, out int maxDamage, out int minDamage)
    {
        radius = damageRadius;
        maxDamage = maxTapDamage;
        minDamage = minTapDamage;
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

        block.OnChunkCreated += HandleChunkCreated;
        block.OnBlockDestroyed += HandleBlockDestroyed;
        registeredBlocks.Add(block);
    }

    private void HandleChunkCreated(PixelBlockController sourceBlock, List<Vector2Int> chunkPixels)
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
            chunkBlock.AddPixel(pixelData.x, pixelData.y, pixelData.pixel);
        }
    }

    private void HandleBlockDestroyed(PixelBlockController destroyedBlock)
    {
        registeredBlocks.Remove(destroyedBlock);
        destroyedBlock.OnChunkCreated -= HandleChunkCreated;
        destroyedBlock.OnBlockDestroyed -= HandleBlockDestroyed;
    }

}
