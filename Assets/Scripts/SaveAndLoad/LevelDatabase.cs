using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> allLevels;

    [ContextMenu("Auto Assign Level IDs")]
    public void AutoAssignLevelIDs()
    {
        for (int i = 0; i < allLevels.Count; i++)
        {
            if (allLevels[i] != null)
            {
                allLevels[i].levelID = i + 1;
            }
        }

        Debug.Log("LevelDatabase: Đã auto assign LevelID theo thứ tự list.");
    }
}
