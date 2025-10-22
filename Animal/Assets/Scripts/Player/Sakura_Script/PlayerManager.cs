using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    // ▼▼▼ 【変数を追加】 ▼▼▼
    // 生成したプレイヤー（のInputPlayer）を覚えておくリスト
    private List<InputPlayer> spawnedPlayers = new List<InputPlayer>();

    // カメラ設定が完了したかどうかを覚えておくフラグ
    private bool isCameraSetupDone = false;
    // ▲▲▲

    void Start()
    {
        // 1. メインカメラ無効化
        if (Camera.main != null)
        {
            Camera.main.gameObject.SetActive(false);
            Debug.Log("Main Camera を無効化しました。");
        }

        // 2. プレイヤー人数の決定
        var gamepads = Gamepad.all;
        int gamepadCount = gamepads.Count;
        int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);
        int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);

        if (playersToSpawn < 2)
        {
            Debug.LogWarning($"接続されたコントローラーが {playersToSpawn} 個です。2個以上必要です。");
            return;
        }
        if (playersToSpawn > 4)
        {
            playersToSpawn = 4;
        }

        Debug.Log($"コントローラー {gamepadCount} 個を検知。{playersToSpawn} 人のプレイヤーを生成します。");

        // 3. プレイヤーを生成
        for (int i = 0; i < playersToSpawn; i++)
        {
            // PlayerInput.Instantiate を使う
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerPrefab[i],
                playerIndex: i,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[i]
            );

            // スポーン地点に配置
            if (PlayerTransforms[i] != null)
            {
                newPlayer.transform.position = PlayerTransforms[i].position;
                newPlayer.transform.rotation = PlayerTransforms[i].rotation;
            }

            // 4. InputPlayer を取得
            InputPlayer inputPlayer = newPlayer.GetComponent<InputPlayer>();
            if (inputPlayer != null)
            {
                // 5. リストに追加
                spawnedPlayers.Add(inputPlayer);
            }
            else
            {
                Debug.LogError($"P{i + 1} ({newPlayer.name}) のプレハブに InputPlayer スクリプトがありません！");
            }
        }
    }

    /// <summary>
    /// プレイヤーのカメラの表示領域 (Viewport Rect) を設定します
    /// </summary>
    private void SetCameraViewport(Camera cam, int playerIndex, int totalPlayers)
    {
        // (中身は変更なし)
        if (totalPlayers == 2)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0f, 0.5f, 1f); }
            else { cam.rect = new Rect(0.5f, 0f, 0.5f, 1f); }
        }
        else if (totalPlayers == 3)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 1) { cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); }
            else { cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); }
        }
        else if (totalPlayers == 4)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 1) { cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 2) { cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); }
            else { cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); }
        }
    }

    // (Update はコードのロジックをそのまま採用)
    void Update()
    {
        // 1. もし、設定完了済みなら、何もしない
        if (isCameraSetupDone)
        {
            return;
        }

        // 2. もし、プレイヤーがまだリストに追加されていなければ（生成前）、
        //    何もしない
        if (spawnedPlayers.Count == 0)
        {
            return;
        }

        // 3. 【準備チェック】
        //    リスト内のプレイヤー全員のカメラが準備OKか（nullじゃないか）確認
        int totalPlayers = spawnedPlayers.Count;
        for (int i = 0; i < totalPlayers; i++)
        {
            // InputPlayer を取得
            InputPlayer inputPlayer = spawnedPlayers[i];

            // InputPlayer の GetPlayerCamera() を呼んでみる
            Camera playerCamera = inputPlayer.GetPlayerCamera();

            // 4. もし、一人でもカメラが null (準備できていない) だったら...
            if (playerCamera == null)
            {
                // （プレハブで設定し忘れの可能性が高い）
                // このフレームの Update はあきらめる
                return;
            }
        }

        // 5. 【設定実行】
        //    (この行までたどり着いた ＝ 全員のカメラが null じゃなかった！)
        Debug.Log("全員のカメラ準備完了！画面分割を実行します。");

        for (int i = 0; i < totalPlayers; i++)
        {
            // もう一度カメラを取得（さっき null じゃないことは確認済み）
            Camera cam = spawnedPlayers[i].GetPlayerCamera();

            // (念のため) カメラを有効化
            if (!cam.gameObject.activeSelf)
            {
                cam.gameObject.SetActive(true);
            }

            // 画面分割を実行
            SetCameraViewport(cam, i, totalPlayers);
        }

        // 6. 【完了フラグ】
        //    設定が終わったので、フラグを true にする
        isCameraSetupDone = true;
    }
}