using UnityEngine;

public class UniversalViewportHandler : MonoBehaviour
{
    private Camera cam;
    private PlayerManager playerManager;
    private Character_Status status;

    void Start()
    {
        cam = GetComponent<Camera>();
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

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
}