using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeViewport2p : MonoBehaviour
{
    public Camera cam;
    public float x = 0f;
    public float y = 0f;
    public float w = 1f;
    public float h = 1f;
    [SerializeField] GameObject playerManager;

    void Start()
    {
        // 自身のカメラコンポーネントを取得する
        cam = GetComponent<Camera>();

        // プレイヤーマネージャー オブジェクトを探し、代入する
        playerManager = GameObject.Find("PlayerManager");
        // プレイヤーマネージャーより、参加人数を受け取る
        var pm_playerCount = playerManager.GetComponent<PlayerManager>().playerCount;

        // カメラの表示位置を変更する
        this.SetViewport(pm_playerCount);
    }

    void Update()
    {
        if (cam != null)
        {
            cam.rect = new Rect(x, y, w, h);
        }
    }

    private void SetViewport(int player_count)
    {
        // 2人モード：画面右側に表示
        if (player_count <= 2)
        {
            x = 0.5f;
            y = 0f;
            w = 0.5f;
            h = 1f;
        }
        // 3,4人モード：画面右上に表示
        else
        {
            x = 0.5f;
            y = 0.5f;
            w = 0.5f;
            h = 0.5f;
        }
    }
}
