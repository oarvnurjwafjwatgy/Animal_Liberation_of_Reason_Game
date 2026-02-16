using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    //プレイヤーのプレファブを設定 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    //プレイヤーの出現位置 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    public int playerCount;

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
        playerCount = playersToSpawn;

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
        for (int i = 1; i < 4; i++)
        {
            // プレイヤー選択で NONE が選ばれている場合はスキップ
            if (Animal_Select.playerChoices[i] == Character_Status.CharacterType.NONE)
                continue;

            //コントローラーが物理的に繋がっているか確認
            int padIndex = i - 1; // 1PはGamepad.all[0]
            if (padIndex >= gamepads.Count)
            {
				Debug.LogWarning($"{i}Pのキャラは選ばれていますが、コントローラーが足りません。");
				break;
			}


			//プレイヤーの生成
			PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerPrefab[padIndex],         // padIndex番目のプレハブ (P1=Lion, P2=Rhino...)
                playerIndex: padIndex,                  // プレイヤー番号 (0, 1, 2, 3)
                controlScheme: "Gamepad",               // "Gamepad" スキーマを使う
                pairWithDevice: gamepads[padIndex]      // padIndex番目のコントローラーを割り当て
            );

			//スポーン位置の設定
			// 生成したプレイヤーを、i番目のスポーン地点に移動・回転させる
			if (PlayerTransforms[padIndex] != null)
            {
                newPlayer.transform.position = PlayerTransforms[padIndex].position;
                newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
            }
            else
            {
                Debug.LogWarning($"P{i + 1} のスポーン地点が設定されていません。");
            }
        }
    }
}