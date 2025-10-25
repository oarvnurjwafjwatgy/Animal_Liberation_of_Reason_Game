using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ★ PlayerInput を使うために必須です

public class PlayerManager : MonoBehaviour
{
    //プレイヤーのプレファブを設定 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    //プレイヤーの出現位置 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    void Start()
    {
        // 接続されているコントローラーの数を参照
        var gamepads = Gamepad.all;
        int gamepadCount = gamepads.Count;

        // スポーン可能な最大人数は、プレハブの数、またはスポーン地点の数の「少ない方」
        int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);

        // 実際にスポーンする人数は、「接続されたコントローラー数」と「最大人数」の「少ない方」
        // (例: コントローラーが5個でも、maxPlayersが4なら、4人まで)
        int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);

        // 要望: 2～4人の場合のみ生成する
        if (playersToSpawn < 1)
        {
            Debug.LogWarning($"接続されたコントローラーが {playersToSpawn} 個です。2個以上必要です。");
            return; // 2人未満なら処理を中断
        }

        // (もし4人より多くても4人に制限する場合)
        if (playersToSpawn > 4)
        {
            playersToSpawn = 4;
        }

        Debug.Log($"コントローラー {gamepadCount} 個を検知。{playersToSpawn} 人のプレイヤーを生成します。");

        // 決定した人数 (playersToSpawn) だけループ（これでエラーは起きません）
        for (int i = 0; i < playersToSpawn; i++)
        {
            // ★重要★ Instantiate の代わりに PlayerInput.Instantiate を使う
            // これにより、プレハブ生成とコントローラー割り当てを同時に行う
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerPrefab[i],         // i番目のプレハブ (P1=Lion, P2=Rhino...)
                playerIndex: i,                // プレイヤー番号 (0, 1, 2, 3)
                controlScheme: "Gamepad",      // "Gamepad" スキーマを使う
                pairWithDevice: gamepads[i]    // i番目のコントローラーを割り当て
            );

            // 生成したプレイヤーを、i番目のスポーン地点に移動・回転させる
            if (PlayerTransforms[i] != null)
            {
                newPlayer.transform.position = PlayerTransforms[i].position;
                newPlayer.transform.rotation = PlayerTransforms[i].rotation;
            }
            else
            {
                Debug.LogWarning($"P{i + 1} のスポーン地点が設定されていません。");
            }
        }
    }

    // Update は空のままでOK
    void Update()
    {

    }
}