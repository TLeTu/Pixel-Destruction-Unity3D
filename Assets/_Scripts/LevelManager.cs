using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject pixelBlockPrefab;
    [SerializeField] private int startWidth = 10;
    [SerializeField] private int startHeight = 10;

    private readonly HashSet<PixelBlockManager> registeredBlocks = new HashSet<PixelBlockManager>();

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

        RegisterBlock(blockManager);
        blockManager.Initialize(startWidth, startHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        foreach (var block in registeredBlocks)
        {
            if (block != null)
            {
                block.OnChunkCreated -= HandleChunkCreated;
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

        GameObject chunkObj = Instantiate(pixelBlockPrefab, transferData.worldCenter, Quaternion.identity, transform);
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
}
