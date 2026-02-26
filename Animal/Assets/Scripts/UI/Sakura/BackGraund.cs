using UnityEngine;

public class BackGraund : MonoBehaviour
{
    // スクロールの速さ（インスペクターから調整できます）
    public float scrollSpeed = 50f;

    // 画像がどれくらい動いたらループさせるか（画像の高さなど）
    public float resetPositionUpdate = 1080f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        // UIの座標を扱うためのRectTransformを取得
        rectTransform = GetComponent<RectTransform>();
        // 最初の位置を覚えておく
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 現在の座標を取得
        Vector2 currentPos = rectTransform.anchoredPosition;

        // Y軸（上方向）に移動させる
        currentPos.y += scrollSpeed * Time.deltaTime;

        // もし移動距離がリセット位置を超えたら、初期位置に戻す
        if (currentPos.y >= startPosition.y + resetPositionUpdate)
        {
            currentPos.y = startPosition.y;
        }

        // 座標を更新
        rectTransform.anchoredPosition = currentPos;
    }
}