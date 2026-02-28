using System.Collections;
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


	// Animal_Select.cs のメンバー変数部分に追加
	public static GameObject[] normalModels_1P;    // インデックス0:ライオン, 1:ダチョウ...
	public static GameObject[] silhouetteModels_1P;

	public static GameObject[] normalModels_2P;
	public static GameObject[] silhouetteModels_2P;

	public static GameObject[] normalModels_3P;
	public static GameObject[] silhouetteModels_3P;

	public static GameObject[] normalModels_4P;
	public static GameObject[] silhouetteModels_4P;

	// 動的にモデルを割り当てるための配列（Inspectorで設定）
	[Header("【Index 0のボタンのみ設定】モデル登録用")]
	public GameObject[] setupNormals_1P;
	public GameObject[] setupSilhouettes_1P;
	public GameObject[] setupNormals_2P;
	public GameObject[] setupSilhouettes_2P;
	public GameObject[] setupNormals_3P;
	public GameObject[] setupSilhouettes_3P;
	public GameObject[] setupNormals_4P;
	public GameObject[] setupSilhouettes_4P;


	private int dynamicRequiredPlayers;         // 動的参加人数
	private bool allPlayersReady = false;       // 全員決定済みフラグ
	private bool isTransitioning = false;       // シーン遷移中フラグ


	public static ReadyImageController readyImageController;    // ReadyImageControllerへの参照

	void Awake()
    {
        // --- シーン開始時の初期化処理 ---
        //最初のボタンのみモデルを割り当てる（重複して割り当てないように）
        if (buttonIndex == 0)
        {
            normalModels_1P = setupNormals_1P;
            silhouetteModels_1P = setupSilhouettes_1P;
            normalModels_2P = setupNormals_2P;
            silhouetteModels_2P = setupSilhouettes_2P;
            normalModels_3P = setupNormals_3P;
            silhouetteModels_3P = setupSilhouettes_3P;
            normalModels_4P = setupNormals_4P;
            silhouetteModels_4P = setupSilhouettes_4P;
        }

		// 全ボタン共通で1回だけ探せばOK
		if (readyImageController == null)
		{
			foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (obj.name == "ReadyImage")
				{
					readyImageController = obj.GetComponent<ReadyImageController>();
					obj.SetActive(true);
					break;
				}
			}
		}

		// --- シーン開始時に全ての情報を「強制」リセット ---
		// どのボタンが担当してもいいですが、重複しないように buttonIndex == 0 の時だけ実行
		if (buttonIndex == 0)
        {
            for (int i = 0; i < playerChoices.Length; i++)
            {
                playerChoices[i] = Character_Status.CharacterType.NONE; // 選択をなしにする
                playerPositions[i] = 0; // カーソルを左端に戻す
            }

            // 準備完了フラグとイラストも初期化
            allPlayersReady = false;
            if (readyImage != null) readyImage.SetActive(false);

            Debug.Log("<color=white>Selection Data Forced Reset.</color>");
        }

	}


    //更新
    void Update()
	{
		// シーンが始まってから 0.1秒経つまでは、一切の入力を無視する
		if (Time.timeSinceLevelLoad < 0.1f) return;

		//シーン遷移中は以下の処理を通さない
		if (isTransitioning) return;

		// 参加人数を確認 (人数選択画面での決定を反映)
		dynamicRequiredPlayers = GameDataManager.SelectedPlayerCount;

		//移動入力処理 (Index 0 のボタンが代表して計算)
		if (buttonIndex == 0)
		{
			for (int pID = 1; pID <= dynamicRequiredPlayers; pID++) // 参戦人数分ループ
			{
				//選択中のプレイヤーが決定済みなら移動不可
				if (playerChoices[pID] != Character_Status.CharacterType.NONE) continue;

				//実際にコントローラーが接続されている場合のみ入力を受け取る
				if (pID <= Gamepad.all.Count)
				{
					var pad = Gamepad.all[pID - 1];
					bool moved = false;// 移動したかどうかのフラグ
					
					// 右移動 (3の次は0に戻るループ)
					if (pad.leftStick.right.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame)
					{
						playerPositions[pID] = (playerPositions[pID] + 1) % 4;
						moved = true;
						Debug.Log($"<color=yellow>{pID}P Move Right: Index {playerPositions[pID]}</color>");
					}
					// 左移動 (0の次は3に回るループ)
					if (pad.leftStick.left.wasPressedThisFrame || pad.dpad.left.wasPressedThisFrame)
					{
						playerPositions[pID] = (playerPositions[pID] + 3) % 4;
						moved = true;
						Debug.Log($"<color=yellow>{pID}P Move Left: Index {playerPositions[pID]}</color>");
					}
					// 移動音の再生&描画 (SE番号 1)
					if (moved && AudioManager.Instance != null)
					{
						AudioManager.Instance.PlaySEByIndex(1);
						UpdateDisplayModel(pID, playerPositions[pID], false); // カーソル移動のたびにモデル更新
					}
				}

				// キーボード2P移動(デバック用)
				if (pID == 2)
				{
					if (Input.GetKeyDown(KeyCode.RightArrow))
					{
						playerPositions[2] = (playerPositions[2] + 1) % 4;
					}
					if (Input.GetKeyDown(KeyCode.LeftArrow))
					{
						playerPositions[2] = (playerPositions[2] + 3) % 4;
					}
				}
			}
		}

		/*********各ボタンの表示と「決定・キャンセル」判定************/
		//1Pから現在の最大参加人数まで処理
		for (int pID = 1; pID <= dynamicRequiredPlayers; pID++)
		{
			// 枠（pFrames）の数を超えないように安全チェックを追加
			if (pID > pFrames.Length || pFrames[pID - 1] == null) continue;

			// このプレイヤーが「このボタン」にいるか、またはここで決定済みか
			bool isHere = (playerPositions[pID] == buttonIndex);
			bool isDecidedHere = (playerChoices[pID] == animalType && animalType != Character_Status.CharacterType.NONE);

			// マークの表示切替
			pFrames[pID - 1].SetActive(isDecidedHere || (playerChoices[pID] == Character_Status.CharacterType.NONE && isHere));

			// そのボタンの上にいる時だけ
			if (isHere && playerChoices[pID] == Character_Status.CharacterType.NONE)
			{
				// コントローラー接続チェックを厳密化
				if (pID <= Gamepad.all.Count && Gamepad.all[pID - 1] != null &&
				Gamepad.all[pID - 1].buttonSouth.wasPressedThisFrame)
				{
					if (AudioManager.Instance != null) AudioManager.Instance.PlaySEByIndex(0);
					SetChoice(pID);             //決定処理
					CheckAllPlayersReady();     //全員決定済みかチェック
				}

				//キーボード2P用決定ボタン(デバック用)
				else if (pID == 2 && Input.GetKeyDown(KeyCode.Return))
				{
					if (AudioManager.Instance != null) AudioManager.Instance.PlaySEByIndex(0);
					SetChoice(2);
					CheckAllPlayersReady();
				}
			}
			// 【キャンセル判定】そのボタンで決定済みの時だけ
			else if (isDecidedHere)
			{
				//参戦中のプレイヤーかつキャンセルボタンが押されたら
				if (pID <= Gamepad.all.Count && Gamepad.all[pID - 1] != null && Gamepad.all[pID - 1].buttonEast.wasPressedThisFrame)
				{
					AudioManager.Instance.PlaySEByIndex(19);//キャンセル音
					CancelChoice(pID);      //キャンセル処理
				}
				//キーボード2P用キャンセルボタン(デバック用)
				else if (pID == 2 && Input.GetKeyDown(KeyCode.Backspace))
				{
					AudioManager.Instance.PlaySEByIndex(19);//キャンセル音
					CancelChoice(2);
				}
			}
		}

		//誰のスタートボタンでも反応するように変更
		bool startPad = false;
		foreach (var pad in Gamepad.all)
		{
			if (pad.startButton.wasPressedThisFrame)
			{
				startPad = true;
				break;
			}
		}

		//全員決定済みかつスタートボタンorスペースキーが押されたらバトル開始
		if (allPlayersReady && (startPad || Input.GetKeyDown(KeyCode.Space))) StartBattle();
	}

	//カーソルが特定のボタンにいるとき、
	//通常モデルとシルエットモデルを切り替える処理
	private void UpdateDisplayModel(int pID,int animalIndex,bool isDecided)
	{
		GameObject[] normals = null;		//決定を押すと通常モデル
		GameObject[] silhouettes = null;    //決定前はシルエットモデル
		if (pID == 1)
		{
			normals = normalModels_1P;
			silhouettes = silhouetteModels_1P;
		}
		else if (pID == 2)
		{
			normals = normalModels_2P;
			silhouettes = silhouetteModels_2P;
		}
		else if (pID == 3)
		{
			normals = normalModels_3P;
			silhouettes = silhouetteModels_3P;
		}
		else if (pID == 4)
		{
			normals = normalModels_4P;
			silhouettes = silhouetteModels_4P;
		}

		// 配列が空、または animalIndex が範囲外なら何もしない
		if (normals == null || silhouettes == null || animalIndex < 0 || animalIndex >= normals.Length)
		{
			return;
		}

		// 全てのモデルを一旦オフにして、該当インデックスだけオンにする
		for (int i = 0; i < normals.Length; i++)
		{
			if (normals[i] != null) normals[i].SetActive(isDecided && i == animalIndex);
			if (silhouettes[i] != null) silhouettes[i].SetActive(!isDecided && i == animalIndex);
		}
	}

	//決定処理
	void SetChoice(int pID)
	{
		playerChoices[pID] = animalType;								 //選んだ動物を配列に保存
		select_saver.Instance.PlayerChoices[pID - 1] = animalType;
		UpdateDisplayModel(pID, playerPositions[pID], true);             // 決定したら通常モデルに切り替え
		PlayCharacterVoice(animalType);
		Debug.Log($"<color=cyan>{pID}P 決定:</color> {animalType}");
	}

	//キャンセル処理
	void CancelChoice(int pID)
	{
		playerChoices[pID] = Character_Status.CharacterType.NONE;
		select_saver.Instance.PlayerChoices[pID - 1] = Character_Status.CharacterType.NONE;
		UpdateDisplayModel(pID, playerPositions[pID], false);       // キャンセルしたらシルエットモデルに切り替え
		
		//選択状態をリセット
		allPlayersReady = false;                                    //全員決定済みフラグをリセット
		if (readyImageController != null) readyImageController.SlideOut();
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
			if (readyImage != null) readyImage.SetActive(true);   //準備完了イラスト表示
			if (readyImageController != null) readyImageController.SlideIn();
			Debug.Log("<color=orange>ALL PLAYERS READY!</color>");
		}
	}

	void PlayCharacterVoice(Character_Status.CharacterType type)
	{
		if (AudioManager.Instance == null) return;

		int voiceIndex = -1;

		// キャラクタータイプに合わせてAudioManagerのSEインデックスを指定
		// ※番号は実際のseClipsの登録順に合わせて調整
		switch (type)
		{
			case Character_Status.CharacterType.LION:
				voiceIndex = 21;
				break;
			case Character_Status.CharacterType.OSTRICH:
				voiceIndex = 22;
				break;
			case Character_Status.CharacterType.RHINOCELOS:
				voiceIndex = 23;
				break;
			case Character_Status.CharacterType.RATEL:
				voiceIndex = 24;
				break;
		}

		if (voiceIndex != -1)
		{
			// ナレーションなので大きめで再生
			AudioManager.Instance.PlaySEByIndex(voiceIndex, 2f);
		}
	}

	//バトルシーンへ移行
	void StartBattle()
	{
		Debug.Log("<color=green>Scene Transition Start.</color>");
		AudioManager.Instance.PlaySEByIndex(20, 5);         // 決定音（20番）を鳴らす
															//        コルーチンで待機してからシーンを切り替える
		if (readyImageController != null)
		{
			// ズームと移動を制御する
			StartCoroutine(StartBattleCoroutine());
		}
		else
		{
			// ReadyImageがない場合のフォールバック
			StartCoroutine(SceneTransitionCoroutine());
		}
		isTransitioning = true; // 遷移開始フラグを立てる
	}

	// 演出を待ってからシーンを遷移させるコルーチン
	private IEnumerator StartBattleCoroutine()
	{
		// ReadyImageのズーム演出を呼び出す
		readyImageController.ZoomAndStart();

		// 演出全体(ズーム0.3 + 待機0.2 + 暗転0.2) + シーン読み込み待ち時間
		yield return new WaitForSeconds(0.5f);

		SceneManager.LoadScene(mainSceneName); //シーン移行
	}

	//ReadyImageがない場合用の安全な遷移用コルーチン
	private IEnumerator SceneTransitionCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		SceneManager.LoadScene(mainSceneName);
	}
}