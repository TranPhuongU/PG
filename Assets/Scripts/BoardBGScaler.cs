using UnityEngine;

public class BoardBGScaler : MonoBehaviour
{
    public float margin = 0.6f;          // board rộng hơn grid
    public float boardSpriteWidth = 1f;  // chiều rộng gốc của board sprite
    public float boardSpriteHeight = 1f; // nếu cần scale theo cao


    public void FitBoard(int width)
    {
        float gridWidth = width;

        float centerX = (gridWidth - 1) * 0.5f;

        float targetWidth = gridWidth + margin;
        float scaleX = targetWidth / boardSpriteWidth;

        transform.localScale = new Vector3(scaleX, transform.localScale.y, 1f);
        transform.position = new Vector3(centerX, transform.position.y, 0f);
    }

}
