using UnityEngine;
using System.IO;

public static class ProgressManager
{
    private static string filePath = Path.Combine(Application.persistentDataPath, "progress.json");

    public static GameProgress Load(LevelDatabase _levelDatabase)
    {
        if (!File.Exists(filePath))
        {
            GameProgress p = new GameProgress();

            int total = _levelDatabase.allLevels.Count; // HOẶC gán trong inspector

            for (int i = 1; i <= total; i++)
            {
                p.levels.Add(new LevelProgress
                {
                    levelID = i,
                    unlocked = (i == 1),
                    stars = 0,
                    bestScore = 0
                });
            }

            Save(p);
            return p;
        }

        return JsonUtility.FromJson<GameProgress>(File.ReadAllText(filePath));
    }


    public static void Save(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(filePath, json);
    }

    // Gọi khi thắng level
    public static void SetLevelResult(int levelID, int stars, int score, LevelDatabase _levelDatabase)
    {
        GameProgress progress = Load(_levelDatabase);

        LevelProgress level = progress.levels.Find(l => l.levelID == levelID);
        if (level == null) return;

        // update sao & điểm
        level.stars = Mathf.Max(level.stars, stars);
        level.bestScore = Mathf.Max(level.bestScore, score);

        // mở khóa level tiếp theo
        LevelProgress next = progress.levels.Find(l => l.levelID == levelID + 1);
        if (next != null)
        {
            next.unlocked = true;
        }

        Save(progress);
    }

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[SAVE] Đã xóa file progress.json");
        }
        else
        {
            Debug.Log("[SAVE] Không tìm thấy file để xóa");
        }
    }

}
