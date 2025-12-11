using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelID;

    public int width;
    public int height = 12;

    [Header("Score Settings")]
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };

    [Header("Layout Prefab")]
    public GameObject layoutPrefab;   // để spawn tile, obstacle, starting piece
}
