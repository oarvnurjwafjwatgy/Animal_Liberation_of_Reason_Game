using UnityEngine;

public class UniversalViewportHandler : MonoBehaviour
{
    private Camera cam;
    private PlayerManager playerManager;
    private Character_Status status;
    private float moveViewportTimer;
    private float moveViewportTime;

    void Start()
    {
        cam = GetComponent<Camera>();
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        moveViewportTimer = 0f;
        moveViewportTime = 2f;

        // 親（または自身）からCharacter_Statusを取得して、自分が何Pかを知る
        status = GetComponentInParent<Character_Status>();

        if (playerManager != null && status != null)
        {
            SetViewport(status.playerID, playerManager.playerCount);
        }
    }

    private void SetViewport(int id, int total)
    {
        // total: 全体人数, id: 自分の番号(1~4)
        if (total == 1)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (total == 2)
        {
            // 2人：左右分割
            if (id == 1) cam.rect = new Rect(0f, 0f, 0.5f, 1f);
            if (id == 2) cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            // 3~4人：田の字分割
            float w = 0.5f; float h = 0.5f;
            if (id == 1) cam.rect = new Rect(0.0f, 0.5f, w, h); // 左上
            if (id == 2) cam.rect = new Rect(0.5f, 0.5f, w, h); // 右上
            if (id == 3) cam.rect = new Rect(0.0f, 0.0f, w, h); // 左下
            if (id == 4) cam.rect = new Rect(0.5f, 0.0f, w, h); // 右下
        }
    }

    // 更新
    void Update()
    {
        this.UpdateViewportGameEnd();
    }

    // 勝利時のビューポート短形の更新
    private void UpdateViewportGameEnd()
    {
        // ゲームがまだ終了していない場合は処理しない
        if (!playerManager.isGameEnd) return;

        if (moveViewportTimer > moveViewportTime + 1f) return;

        //Debug.Log("<color=#ffffff>GAME SET</color");

        // 更新量
        float change_rect = 0.5f * moveViewportTimer / moveViewportTime;

        //// 以下、ビューポート短形の数値の更新 ////
        // プレイヤー1が勝利
        if (playerManager.lastPlayer == 1)
        {
            if (status.playerID == 1)       // プレイヤー1のカメラ
            {
                // 2分割の時は高さは触らず、4分割の時は高さも変更
                if (GameDataManager.SelectedPlayerCount <= 2)
                    cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f + change_rect, cam.rect.height);
                else
                    cam.rect = new Rect(cam.rect.x, 0.5f - change_rect, 0.5f + change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 2)  // プレイヤー2のカメラ
            {
                // 2分割の時は高さは触らず、4分割の時は高さも変更
                if (GameDataManager.SelectedPlayerCount <= 2)
                    cam.rect = new Rect(0.5f + change_rect, cam.rect.y, 0.5f - change_rect, cam.rect.height);
                else
                    cam.rect = new Rect(0.5f + change_rect, 0.5f - change_rect, 0.5f - change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 3)  // プレイヤー3のカメラ
            {
                cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f + change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 4)  // プレイヤー4のカメラ
            {
                cam.rect = new Rect(0.5f + change_rect, cam.rect.y, 0.5f - change_rect, 0.5f - change_rect);
            }
        }
        // プレイヤー2が勝利
        else if (playerManager.lastPlayer == 2)
        {
            if (status.playerID == 1)       // プレイヤー1のカメラ
            {
                // 2分割の時は高さは触らず、4分割の時は高さも変更
                if (GameDataManager.SelectedPlayerCount <= 2)
                    cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f - change_rect, cam.rect.height);
                else
                    cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f - change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 2)  // プレイヤー2のカメラ
            {
                // 2分割の時は高さは触らず、4分割の時は高さも変更
                if (GameDataManager.SelectedPlayerCount <= 2)
                    cam.rect = new Rect(0.5f - change_rect, cam.rect.y, 0.5f + change_rect, cam.rect.height);
                else
                    cam.rect = new Rect(0.5f - change_rect, 0.5f - change_rect, 0.5f + change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 3)  // プレイヤー3のカメラ
            {
                cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f - change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 4)  // プレイヤー4のカメラ
            {
                cam.rect = new Rect(0.5f - change_rect, cam.rect.y, 0.5f + change_rect, 0.5f - change_rect);
            }
        }
        // プレイヤー3が勝利
        else if (playerManager.lastPlayer == 3)
        {
            if (status.playerID == 1)       // プレイヤー1のカメラ
            {
                cam.rect = new Rect(cam.rect.x, 0.5f + change_rect, 0.5f + change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 2)  // プレイヤー2のカメラ
            {
                cam.rect = new Rect(0.5f + change_rect, 0.5f + change_rect, 0.5f - change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 3)  // プレイヤー3のカメラ
            {
                cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f + change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 4)  // プレイヤー4のカメラ
            {
                cam.rect = new Rect(0.5f + change_rect, cam.rect.y, 0.5f - change_rect, 0.5f + change_rect);
            }
        }
        // プレイヤー4が勝利
        else if (playerManager.lastPlayer == 4)
        {
            if (status.playerID == 1)       // プレイヤー1のカメラ
            {
                cam.rect = new Rect(cam.rect.x, 0.5f + change_rect, 0.5f - change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 2)  // プレイヤー2のカメラ
            {
                cam.rect = new Rect(0.5f - change_rect, 0.5f + change_rect, 0.5f + change_rect, 0.5f - change_rect);
            }
            else if (status.playerID == 3)  // プレイヤー3のカメラ
            {
                cam.rect = new Rect(cam.rect.x, cam.rect.y, 0.5f - change_rect, 0.5f + change_rect);
            }
            else if (status.playerID == 4)  // プレイヤー4のカメラ
            {
                cam.rect = new Rect(0.5f - change_rect, cam.rect.y, 0.5f + change_rect, 0.5f + change_rect);
            }
        }

        // タイマーの更新
        moveViewportTimer += Time.deltaTime;
    }

}