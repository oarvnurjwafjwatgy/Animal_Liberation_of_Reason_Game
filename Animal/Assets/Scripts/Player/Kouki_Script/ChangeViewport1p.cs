using UnityEngine;
using CharaType = CharacterType;

public class ChangeViewport1p : MonoBehaviour
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

        // 参加人数はNONEになるまでで分かるようなので、NONEでないプレイヤー(参加者)を数える
        var pm_playerCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (Animal_Select.playerChoices[i] == CharaType.NONE)
                continue;

            pm_playerCount++;
        }

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
        // 1人モード：全画面表示
        if (player_count == 1)
        {
            x = 0f;
            y = 0f;
            w = 1f;
            h = 1f;
        }
        // 2人モード：画面左側に表示
        else if (player_count == 2)
        {
            x = 0f;
            y = 0f;
            w = 0.5f;
            h = 1f;
        }
        // 3,4人モード：画面左上に表示
        else if (player_count >= 3)
        {
            x = 0f;
            y = 0.5f;
            w = 0.5f;
            h = 0.5f;
        }
    }
}
