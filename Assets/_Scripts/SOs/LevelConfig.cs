using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_00", menuName = "Game Config/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Target")]
    public int targetDestroyCount = 50;

    [Header("Blocks to Spawn")]
    public List<BlockData> blocksToSpawn = new List<BlockData>(); 

    [Header("Tap damage radius")]
    public int maxTapDamageRadius = 3;
    public int minTapDamage = 1;
}