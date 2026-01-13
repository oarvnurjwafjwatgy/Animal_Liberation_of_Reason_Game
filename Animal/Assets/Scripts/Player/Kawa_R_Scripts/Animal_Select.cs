using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Animal_Select : MonoBehaviour
{
	// 静的配列：全プレイヤー(1~4P)の選んだ動物を保存
	public static Character_Status.CharacterType[] playerChoices = new Character_Status.CharacterType[5];
	// 静的配列：各プレイヤーが今どのボタン(0~3)にいるかを記録
	public static int[] playerPositions = new int[] { 0, 0, 0, 0, 0 };

	/******UnityのInspectorで設定可能な項目********/

	//ボタンにある動物タイプ
	[Header("この選択肢の動物タイプ")]
	public Character_Status.CharacterType animalType;

	//シーン
	[Header("設定")]
	public string mainSceneName = "SampleScene";

	//準備完了イラスト
	[Header("準備完了イラスト")]
	public static GameObject readyImage;

	//プレイヤーの選択マーク
	[Header("Playerマーク")]
	public GameObject[] pFrames;

	//動物選択ボタンのインデックス
	[Header("ボタンのインデックス (0~3)")]
	public int buttonIndex;

	private int dynamicRequiredPlayers;         // 動的参加人数
	private bool allPlayersReady = false;       // 全員決定済みフラグ
	private bool isTransitioning = false;       // シーン遷移中フラグ


	void Awake()
	{
		// 全ボタン共通で1回だけ探せばOK
		if (readyImage == null)
		{
			foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (obj.name == "ReadyImage")
				{
					readyImage = obj;
					break;
				}
			}
		}

		// シーン開始時に全ての情報をリセット
		// buttonIndex 0 のボタンが代表して 1回だけログを出す
		if (buttonIndex == 0)
		{
			for (int i = 0; i < playerChoices.Length; i++)
			{
				playerChoices[i] = Character_Status.CharacterType.NONE;
				playerPositions[i] = 0;
			}
			Debug.Log("<color=white>Selection Data Reset.</color>");
		}
	}


	//更新
	void Update()
	{
	//シーン遷移中は以下の処理を通さない
		if (isTransitioning) return;

		// 接続されているコントローラー数を確認
		dynamicRequiredPlayers = Mathf.Clamp(Gamepad.all.Count, 2, 4);

		//移動入力処理 (Index 0 のボタンが代表して計算)
		if (buttonIndex == 0)
		{
			for (int pID = 1; pID <= 4; pID++)
			{
				//選択中のプレイヤーが決定済みなら移動不可
				if (playerChoices[pID] != Character_Status.CharacterType.NONE) continue;

				//全体の参戦人数より多いプレイヤーIDは無視
				if (pID <= Gamepad.all.Count)
				{
					var pad = Gamepad.all[pID - 1];     //推論にて参戦人数をpadに格納

					// 右移動 (3の次は0に戻るループ)
					if (pad.leftStick.right.wasPressedThisFrame
					|| pad.dpad.right.wasPressedThisFrame)
					{
						playerPositions[pID] = (playerPositions[pID] + 1) % 4;
						Debug.Log($"<color=yellow>{pID}P Move Right: Index {playerPositions[pID]}</color>");
					}
					// 左移動 (0の次は3に回るループ)
					if (pad.leftStick.left.wasPressedThisFrame
					|| pad.dpad.left.wasPressedThisFrame)
					{
						playerPositions[pID] = (playerPositions[pID] + 3) % 4;
						Debug.Log($"<color=yellow>{pID}P Move Left: Index {playerPositions[pID]}</color>");
					}
				}

				// キーボード2P移動(デバック用)
				if (pID == 2)
				{
					if (Input.GetKeyDown(KeyCode.RightArrow))
					{
						playerPositions[2] = (playerPositions[2] + 1) % 4;
						Debug.Log("<color=yellow>2P (KB) Move Right</color>");
					}
					if (Input.GetKeyDown(KeyCode.LeftArrow))
					{
						playerPositions[2] = (playerPositions[2] + 3) % 4;
						Debug.Log("<color=yellow>2P (KB) Move Left</color>");
					}
				}
			}
		}

		/*********各ボタンの表示と「決定・キャンセル」判定************/
		//1Pから現在の最大参加人数まで処理
		for (int pID = 1; pID <= dynamicRequiredPlayers; pID++)
		{
			// プレイヤーのフレームが設定されていなければスキップ
			if (pID > pFrames.Length || pFrames[pID - 1] == null) continue;

			// このプレイヤーが「このボタン」にいるか、またはここで決定済みか
			bool isHere = (playerPositions[pID] == buttonIndex);
			bool isDecidedHere = (playerChoices[pID] ==
			animalType && animalType != Character_Status.CharacterType.NONE);


			// マークの表示切替 (ここにいない時は強制的に消すことで「全員出現」を防ぐ)
			pFrames[pID - 1].SetActive(isDecidedHere || (playerChoices[pID] == Character_Status.CharacterType.NONE && isHere));


			// 【決定判定】そのボタンの上にいる時だけ
			if (isHere && playerChoices[pID] == Character_Status.CharacterType.NONE)
			{
				//選択マークがボタンの上にある・ボタン未決定の時
				if (pID <= Gamepad.all.Count && Gamepad.all[pID - 1].buttonSouth.wasPressedThisFrame)
				{
					SetChoice(pID);             //決定処理
					CheckAllPlayersReady();     //全員決定済みかチェック
				}
				//キーボード2P用決定ボタン(デバック用)
				else if (pID == 2 && Input.GetKeyDown(KeyCode.Return))
				{
					SetChoice(2);
					CheckAllPlayersReady();
				}
			}
			// 【キャンセル判定】そのボタンで決定済みの時だけ
			else if (isDecidedHere)
			{
				//参戦中のプレイヤーかつキャンセルボタンが押されたら
				if (pID <= Gamepad.all.Count && Gamepad.all[pID - 1].buttonEast.wasPressedThisFrame)
				{
					CancelChoice(pID);      //キャンセル処理
				}
				//キーボード2P用キャンセルボタン(デバック用)
				else if (pID == 2 && Input.GetKeyDown(KeyCode.Backspace))
				{
					CancelChoice(2);
				}
			}
		}

		// 開始判定
		//startPadに参加人数者の誰かがスタートボタンを押したか判定したら
		//このフラグはtrueになる
		bool startPad = (Gamepad.all.Count > 0 && Gamepad.all[0].startButton.wasPressedThisFrame);

		//全員決定済みかつスタートボタンorスペースキーが押されたらバトル開始
		if (allPlayersReady && (startPad || Input.GetKeyDown(KeyCode.Space))) StartBattle();
	}

	//決定処理
	void SetChoice(int pID)
	{
		playerChoices[pID] = animalType;    //選んだ動物を配列に保存
		Debug.Log($"<color=cyan>{pID}P 決定:</color> {animalType}");
	}

	//キャンセル処理
	void CancelChoice(int pID)
	{
		playerChoices[pID] = Character_Status.CharacterType.NONE;   //選択状態をリセット
		allPlayersReady = false;                                    //全員決定済みフラグをリセット
		if (readyImage != null) readyImage.SetActive(false);        //準備完了イラスト非表示
		Debug.Log($"<color=red>{pID}P キャンセル</color>");
	}

	//全員決定済みチェック
	void CheckAllPlayersReady()
	{
		int count = 0;      //準備完了プレイヤー数カウント

		//参加プレイヤー全員分ループ
		for (int i = 1; i <= dynamicRequiredPlayers; i++)
			//選択済みならカウントアップ
			if (playerChoices[i] != Character_Status.CharacterType.NONE) count++;

		//全員決定済みならフラグを立てる
		if (count >= dynamicRequiredPlayers)
		{
			allPlayersReady = true;     //Areyouready？
			if (readyImage != null) readyImage.SetActive(true); //準備完了イラスト表示
			Debug.Log("<color=orange>ALL PLAYERS READY!</color>");
		}
	}

	//バトルシーンへ移行
	void StartBattle()
	{
		Debug.Log("<color=green>Scene Transition Start.</color>");
		isTransitioning = true;                     // シーン遷移中フラグを立てる
		SceneManager.LoadScene(mainSceneName);      //シーン移行
	}
}