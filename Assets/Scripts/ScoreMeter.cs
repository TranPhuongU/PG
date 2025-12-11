using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreMeter : MonoBehaviour
{
    [SerializeField] Image image;
    public ScoreStar[] scoreStars = new ScoreStar[3];
    public Image[] scoreImage = new Image[3];
    GameManager m_levelGoal;

    int m_maxScore;
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    //public void SetupStars(GameManager levelGoal)
    //{
    //    if (levelGoal == null)
    //    {
    //        return;
    //    }

    //    m_levelGoal = levelGoal;

    //    m_maxScore = m_levelGoal.scoreGoals[m_levelGoal.scoreGoals.Length - 1];

    //    float sliderHeight = image.GetComponent<RectTransform>().rect.height;

    //    if (m_maxScore > 0)
    //    {
    //        for (int i = 0; i < levelGoal.scoreGoals.Length; i++)
    //        {
    //            if (scoreStars[i] != null)
    //            {
    //                float newY = (sliderHeight * levelGoal.scoreGoals[i] / m_maxScore) - (sliderHeight * 0.5f);
    //                RectTransform starRectXform = scoreStars[i].GetComponent<RectTransform>();
    //                if (starRectXform != null)
    //                {
    //                    starRectXform.anchoredPosition = new Vector2(starRectXform.anchoredPosition.x, newY);
    //                }
    //            }
    //        }
    //    }
    //}

    public void SetupVStars(GameManager levelGoal)
    {
        if (levelGoal == null)
        {
            return;
        }

        m_levelGoal = levelGoal;

        m_maxScore = m_levelGoal.scoreGoals[m_levelGoal.scoreGoals.Length - 1];

        float sliderHeight = image.GetComponent<RectTransform>().rect.height;

        if (m_maxScore > 0)
        {
            for (int i = 0; i < levelGoal.scoreGoals.Length; i++)
            {
                if (scoreImage[i] != null)
                {
                    float newY = (sliderHeight * levelGoal.scoreGoals[i] / m_maxScore) - (sliderHeight * 0.5f);
                    RectTransform starRectXform = scoreImage[i].GetComponent<RectTransform>();
                    if (starRectXform != null)
                    {
                        starRectXform.anchoredPosition = new Vector2(starRectXform.anchoredPosition.x, newY);
                    }
                }
            }
        }
    }

    public void UpdateScoreMeter(int starCount)
    {
        //if (m_levelGoal != null)
        //{
        //    image.fillAmount = (float)score / (float)m_maxScore;
        //}

        for (int i = 0; i < starCount; i++)
        {
            if (scoreStars[i] != null)
            {
                scoreStars[i].Activate();
            }
        }
    }
}
