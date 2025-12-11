using System;
using System.Collections.Generic;

[Serializable]
public class LevelProgress
{
    public int levelID;
    public bool unlocked;
    public int stars;      // 0–3
    public int bestScore;
}

[Serializable]
public class GameProgress
{
    public List<LevelProgress> levels = new List<LevelProgress>();
}
