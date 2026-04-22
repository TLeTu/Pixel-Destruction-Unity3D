using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_00", menuName = "Game Config/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level")]
    public int targetDestroyCount = 50;
    public int scoreThreshold = 500;
    public int initialWeaponCount = 1;

    [Header("Blocks to Spawn")]
    public List<BlockData> blocksToSpawn = new List<BlockData>(); 

    [Header("Tap damage radius")]
    public float damageRadius = 5;
    public int maxTapDamage = 3;
    public int minTapDamage = 1;

    [Header("Obstacles Settings")]
    public Vector3[] obstaclePositions;
    public GameObject weaponPrefab;
}