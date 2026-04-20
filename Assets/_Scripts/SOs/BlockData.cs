using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockData", menuName = "Game Config/Block Data")]
public class BlockData : ScriptableObject
{
    [Header("Block Settings")]
    public Sprite sprite;
}