using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelMenuManager : MonoBehaviour
{
    public Transform buttonContainer;
    public GameObject levelButtonPrefab;   // Prefab LevelButton
    public LevelDatabase database;

    void Start()
    {
        GameProgress progress = ProgressManager.Load(database);

        int total = database.allLevels.Count;

        for (int i = 0; i < total; i++)
        {
            int levelID = i + 1;

            LevelData dataDB = database.allLevels[i];
            LevelProgress dataSave = progress.levels.Find(l => l.levelID == levelID);

            LevelButton btn = Instantiate(levelButtonPrefab, buttonContainer).GetComponent<LevelButton>();

            btn.SetLevelNumber(levelID);
            btn.UpdateView(dataSave);

            btn.SetClickAction(() =>
            {
                SelectedLevel.levelID = levelID;
                SceneManager.LoadScene("Gameplay");
            });
        }
    }

}


public static class SelectedLevel
{
    public static int levelID;
}
