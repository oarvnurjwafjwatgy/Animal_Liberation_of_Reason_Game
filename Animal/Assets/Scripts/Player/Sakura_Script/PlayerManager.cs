using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
	[SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
	[SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

	public int playerCount;

	// 生成されたプレイヤーを監視するためのリスト
	private List<Character_Status> spawnedPlayers = new List<Character_Status>();

	//初期化
	void Start()
	{
		var gamepads = Gamepad.all;
		int gamepadCount = gamepads.Count;
		spawnedPlayers.Clear(); // リストを初期化

		int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);
		int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);
		playerCount = playersToSpawn;

		if (playersToSpawn < 1)
		{
			Debug.LogWarning($"接続されたコントローラーが {playersToSpawn} 個です。2個以上必要です。");
			return;
		}

		if (playersToSpawn > 4) { playersToSpawn = 4; }

		Debug.Log($"コントローラー {gamepadCount} 個を検知。{playersToSpawn} 人のプレイヤーを生成します。");

		// キャラ選択の数に基づいてプレイヤーを生成
		for (int i = 1; i <= 4; i++)
		{
			//キャラを選択したPlayer以外ならスキップ
			if (Animal_Select.playerChoices[i] == Character_Status.CharacterType.NONE)
				continue;

			int padIndex = i - 1;

			// コントローラーの数が足りない場合は警告を出してループを抜ける
			if (padIndex >= gamepads.Count)
			{
				Debug.LogWarning($"{i}Pのキャラは選ばれていますが、コントローラーが足りません。");
				break;
			}

			// 生成するプレイヤーのインスタンスを作成
			PlayerInput newPlayer = PlayerInput.Instantiate(
				prefab: PlayerPrefab[padIndex],
				playerIndex: padIndex,
				controlScheme: "Gamepad",
				pairWithDevice: gamepads[padIndex]
			);

			// 生成したプレイヤーを指定の位置に配置
			if (PlayerTransforms[padIndex] != null)
			{
				newPlayer.transform.position = PlayerTransforms[padIndex].position;
				newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
			}

			//生成したキャラのステータスをリストに保存
			Character_Status status = newPlayer.GetComponent<Character_Status>();
			//情報が取れたらリストに追加
			if (status != null)
			{
				spawnedPlayers.Add(status);
			}
		}
	}

	//更新
	void Update()
	{
		// 1. まず生存人数を数える
		int aliveCount = 0;

		//生存リストにいるプレイヤーを毎時確認し数を数える
		foreach (var player in spawnedPlayers)
		{
			//生存者のみを数える
			if (player != null && !player.IsDead)
			{
				aliveCount++;	//カウントアップ
			}
		}

		playerCount = aliveCount;   //生存人数を更新


		// 2. ここで「残り1人」になった時の判定をする
		// playerCountが 1 かつ、最初から1人プレイでない場合（複数人で始めた場合）
		if (playerCount == 1)
		{
			Debug.Log("決着！残り1人になりました。");

			// ここに「リザルト画面へ行く」などの処理を書きます
			SceneManager.LoadScene("ResultScene"); 
		}
		else if (playerCount == 0)
		{
			Debug.Log("全員死亡（引き分け）");
			//わんちゃんサドンデス式をここに書くかも
		}
	}
}