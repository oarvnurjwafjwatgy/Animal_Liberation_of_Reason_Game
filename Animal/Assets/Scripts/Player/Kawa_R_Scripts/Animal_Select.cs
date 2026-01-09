using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Animal_Select : MonoBehaviour
{
	// 静的配列で全プレイヤーの選択を管理 (インデックス0は未使用)
	public static Character_Status.CharacterType[] playerChoices = new Character_Status.CharacterType[5];
	
	[Header("この選択肢の動物タイプ")]
	public Character_Status.CharacterType animalType;

	[Header("設定")]
	public string mainSceneName = "MainGameScene";

	private int dynamicRequiredPlayers;     // 必要なプレイヤー数 (接続されているゲームパッド数に基づく)
	private bool allPlayersReady = false;	// 全員がキャラを選んだか
	private bool isTransitioning = false;	// シーン移動中か


	void Awake()
	{
		//初期化処理（配列を1ずつチェックしてく）:全てNONEにする
		for (int i = 0; i < playerChoices.Length; i++)
			playerChoices[i] = Character_Status.CharacterType.NONE;
	}

	void Update()
	{
		// シーン移動中は入力を受け付けない
		if (isTransitioning) return;

		/* 動的に必要なプレイヤー数を設定 (2～4人)
		Gamepad.all.Countは現在PCに繋がってる数をカウントしてくれる。
		Math.Clamp(値,最小,最大)　範囲に収まるようにしてくれるもの*/
		dynamicRequiredPlayers = Mathf.Clamp(Gamepad.all.Count, 2, 4);

		/*セレクト中ではこのオブジェクトが選択されている場合のみ入力を受け付ける
		これにより処理が個別化せず共通して動かすことが可能かと*/
		if (EventSystem.current.currentSelectedGameObject == this.gameObject)
		{
			// --- 全プレイヤーの入力をループでチェック ---
			for (int i = 0; i < Gamepad.all.Count; i++)
			{
				var pad = Gamepad.all[i];   //推論にてGamepad.all[i]がi番目のコントローラーを指す
				int pID = i + 1;            // プレイヤーIDはi+1 (P1=1, P2=2, ...)

				// 1. キャラ決定 (Aボタン押された際にまだキャラを未選択の場合)
				if (pad.buttonSouth.wasPressedThisFrame && !allPlayersReady)
				{
					SetChoice(pID);
					CheckAllPlayersReady(); // 準備状況を確認
				}

				// 2. キャンセル (Bボタン)
				if (pad.buttonEast.wasPressedThisFrame)
				{
					CancelChoice(pID);
					allPlayersReady = false; //1人はキャンセルしたので全員準備完了はfalseに戻す
				}

				// 3. バトル開始 (全員Readyの時にStartボタンまたはMenuボタン)
				if (allPlayersReady && (pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame))
				{
					StartBattle();
				}
			}

			// --- デバッグ用キーボード入力 ---
			// 決定(Z/Enter) / キャンセル(X/Backspace)
			if (Input.GetKeyDown(KeyCode.Z)) { SetChoice(1); CheckAllPlayersReady(); }
			if (Input.GetKeyDown(KeyCode.Return)) { SetChoice(2); CheckAllPlayersReady(); }
			if (Input.GetKeyDown(KeyCode.X)) { CancelChoice(1); allPlayersReady = false; }
			if (Input.GetKeyDown(KeyCode.Backspace)) { CancelChoice(2); allPlayersReady = false; }

			// キーボードでのバトル開始 (全員Ready時に Space)
			if (allPlayersReady && Input.GetKeyDown(KeyCode.Space))
			{
				StartBattle();
			}
		}
	}

	// プレイヤーの選択を設定
	void SetChoice(int playerId)
	{
		playerChoices[playerId] = animalType;//予約表に選択を登録
		Debug.Log($"<color=cyan>{playerId}P 決定:</color> {animalType}");
	}

	// プレイヤーのキャラ選択をキャンセル
	void CancelChoice(int playerId)
	{
		//配列の中身がNONE以外ならキャンセル処理実行
		if (playerChoices[playerId] != Character_Status.CharacterType.NONE)
		{
			//そのプレイヤーが選んだキャラクターをNONEに戻す
			playerChoices[playerId] = Character_Status.CharacterType.NONE;
			Debug.Log($"<color=red>{playerId}P キャンセルしました</color>"); // 赤文字ログ
		}
	}

	// 全プレイヤーが準備完了か確認
	void CheckAllPlayersReady()
	{
		int readyCount = 0;//準備完了したプレイヤー数カウント

		// 予約表をチェックして準備完了数をカウント
		for (int i = 1; i <= dynamicRequiredPlayers; i++)
		{
			// NONE以外が選ばれていれば準備完了とみなす
			if (playerChoices[i] != Character_Status.CharacterType.NONE)
				readyCount++;
		}

		//予約表の準備完了数が必要数に達したら全員準備完了とする
		if (readyCount >= dynamicRequiredPlayers)
		{
			allPlayersReady = true; // 全員準備完了
			Debug.Log("<color=yellow>READY? (StartボタンかSpaceキーで開始！)</color>");
		}
	}

	// バトルシーンへ移動
	void StartBattle()
	{
		Debug.Log("<color=orange>GO!! シーン移動開始</color>");
		isTransitioning = true;
		SceneManager.LoadScene(mainSceneName);
	}
}