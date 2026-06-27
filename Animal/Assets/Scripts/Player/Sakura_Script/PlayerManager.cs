using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CharaType = CharacterType;

/*全体の基礎処理担当者:
 古澤 桜
 
*ゲームスタート関連の処理・SEの再生タイミングの調整・勝利演出(とどめの演出も含む)などといった
一部の担当者:
リファクタリング作業:川上 流輝 */

public partial class PlayerManager : MonoBehaviour
{
    //生成したプレイヤーを管理するリスト
    private List<Character_Status> spawnedPlayers = new List<Character_Status>();
    // ★追加: プレイヤーごとのカメラオブジェクトを保持するリスト
    private List<GameObject> playerCameras = new List<GameObject>();

    [Header("UI設定")]
    [SerializeField] private UIManager uiManager;// UIマネージャーの参照
    [SerializeField] private List<Transform> uiPositions = new List<Transform>();// 1P~4PのUI位置
    [SerializeField] private EndManager endManager;     // エンドマネージャーの参照

    [Header("演出用カメラ位置")]
    [SerializeField] private List<Transform> introCameraPositions = new List<Transform>();
    [SerializeField] private Camera introCamera;// 演出専用カメラの参照

    [HideInInspector] // インスペクターには出さなくて良い場合はこれをつける
    public int playerCount;
    public bool isGameEnd;  // ゲーム終了フラグ
    public int lastPlayer;  // 最後に残ったプレイヤー
    private List<int> diedPlayer = new List<int>(); // 死んだプレイヤーを順番に格納

    [Header("プレイヤーの土台プレハブ")]
    [SerializeField] private GameObject PlayerBasePrefab;

    [Header("動物プレハブ設定 (Element 0=LION, 1=OSTRICH...)")]
    [SerializeField] private List<GameObject> AnimalPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> AnimalReasonPrefabs = new List<GameObject>();

    [Header("出現位置")]
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    [SerializeField] private GameObject backTitleAuto;

    // システムの初期化
    private void InitializeSystem()
    {
        Time.timeScale = 1.0f;// ゲーム開始時にタイムスケールをリセット
        Time.fixedDeltaTime = 0.02f;
        playerCount = GameDataManager.SelectedPlayerCount;
        isGameEnd = false;
        lastPlayer = 0;
        spawnedPlayers.Clear();// 既存のプレイヤーリストをクリア
        playerCameras.Clear();
        diedPlayer.Clear();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(AudioManager.Instance.battleBGM);
    }

    //出現位置をシャッフルする関数
    private void ShuffleSpawnPoints()
    {
        for (int i = PlayerTransforms.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            if (i == j) continue;
            Transform temp = PlayerTransforms[i];
            PlayerTransforms[i] = PlayerTransforms[j];
            PlayerTransforms[j] = temp;
        }
    }

    //プレイヤー全体の生成ループ関数
    private void SpawnAllPlayers()
    {
        var gamepads = Gamepad.all;
        int playersToSpawn = GameDataManager.SelectedPlayerCount;
        for (int i = 1; i <= playersToSpawn; i++)
        {
            CharaType selectedType = Animal_Select.playerChoices[i];
            if (selectedType == CharaType.NONE) continue;
            Gamepad targetPad = (i - 1 < gamepads.Count) ? gamepads[i - 1] : null;
            SpawnSinglePlayer(i, targetPad, selectedType);
        }
    }

    // プレイヤー1人分の生成・配置の関数
    private void SpawnSinglePlayer(int pID, Gamepad pad, CharaType selectedType)
    {
        int padIndex = pID - 1;
        int animalIndex = (int)selectedType - 1;

        //土台生成
        PlayerInput newPlayer = PlayerInput.Instantiate(
        prefab: PlayerBasePrefab,
        playerIndex: padIndex,
        controlScheme: "Gamepad",
        pairWithDevice: pad
        );

        //モデル生成&子登録
        GameObject normalModel = Instantiate(AnimalPrefabs[animalIndex], newPlayer.transform);
        GameObject reasonModel = Instantiate(AnimalReasonPrefabs[animalIndex], newPlayer.transform);
        reasonModel.SetActive(false);
        SetupPlayerReferences(newPlayer.gameObject, pID, normalModel, reasonModel);//SetupPlayerReferencesを呼ぶ

		//UIの生成とStatusへの紐付け
		if (uiManager != null && uiPositions.Count >= pID)
        {
            var status = newPlayer.GetComponent<Character_Status>();
            Slider hp = uiManager.CreateUI(UIManager.UI_ID.GAUGE_HP, uiPositions[padIndex], pID);
            Slider rs = uiManager.CreateUI(UIManager.UI_ID.GAUGE_REASON, uiPositions[padIndex], pID);

            if (status != null)
            {
                status.ReInitialize(pID);
                status.SetUIComponents(hp, rs, uiManager, uiPositions[padIndex]);
                spawnedPlayers.Add(status);
            }
        }

        //位置＆回転の初期化
        if (PlayerTransforms[padIndex] != null)
        {
            newPlayer.transform.position = PlayerTransforms[padIndex].position;
            newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
        }
    }

    //初期化
    void Start()
    {
        InitializeSystem();
        ShuffleSpawnPoints();
        SpawnAllPlayers();
        StartCoroutine(BattleStartSequence()); // プレイヤー生成後、演出を開始
    }

    // 戦闘開始の演出を行うコルーチン
    IEnumerator BattleStartSequence()
    {
        // 1. 全プレイヤーの入力を一時的に無効化
        foreach (var p in spawnedPlayers) { p.GetComponent<InputPlayer>().enabled = false; }

        // 2. 演出開始
        if (uiManager != null) uiManager.ShowIntroductionPanel();
        if (introCamera != null) introCamera.enabled = true;// 演出カメラを有効化

		for (int i = 0; i < spawnedPlayers.Count; i++)
        {
            if (i < introCameraPositions.Count && introCameraPositions[i] != null)
            {
                // 演出カメラを定位置に移動
                introCamera.transform.position = introCameraPositions[i].position;
                introCamera.transform.rotation = introCameraPositions[i].rotation;

                Debug.Log($"演出カメラ移動: プレイヤー{i + 1}");
                yield return new WaitForSeconds(2.0f); // 1人あたり2秒表示
            }
        }
        if (introCamera != null) introCamera.enabled = false;// 演出カメラをオフにする

		// 3. カウントダウン処理（UI表示、SE再生）                
		SetCountDownPreparation("ShowYour Instincts?", Color.cyan, 25, 2f);  //全員準備完了のナレーション（25番）
        yield return new WaitForSeconds(2.0f);
        SetCountDownPreparation("3", Color.green, 16); // カウントダウン音を鳴らす
        yield return new WaitForSeconds(1.0f);
        SetCountDownPreparation("2", Color.yellow);
        yield return new WaitForSeconds(1.0f);
        SetCountDownPreparation("1", Color.magenta);
        yield return new WaitForSeconds(1.0f);

        // GO! のタイミング文字を赤色にする
        if (uiManager != null) SetCountDownPreparation("GO!", Color.red);
        yield return new WaitForSeconds(1.0f);

        // 4. 演出終了後、UIを切り替える
        if (uiManager != null) uiManager.HideIntroductionPanel(); // 黒画面を消す
        uiManager.HideCountdown(); // カウントダウンを消す

        // 5. 全プレイヤーの入力を有効化
        foreach (var p in spawnedPlayers) { p.GetComponent<InputPlayer>().enabled = true; }
    }

    //カウントダウンに使用する設定(seNumber:27は無音)
    private void SetCountDownPreparation(string contents, Color color, int seNumber = 27, float seVol = 5f)
    {
        uiManager.SetCountdownColor(color);
        uiManager.ShowCountdown(contents);
        AudioManager.Instance.PlaySEByIndex(seNumber, seVol);
    }

    private void SetupPlayerReferences(GameObject playerObj, int pID, GameObject normal, GameObject reason)
    {
        // カメラは子オブジェクトの0番目
        if (playerObj.transform.childCount > 0)
        {
            GameObject camObj = playerObj.transform.GetChild(0).gameObject;
            playerCameras.Add(camObj);  // リストに追加
			var v1 = camObj.GetComponent<ChangeViewport1p>();// Viewportスクリプトの制御
			var v2 = camObj.GetComponent<ChangeViewport2p>();
            if (v1 != null) v1.enabled = (pID == 1);
            if (v2 != null) v2.enabled = (pID == 2);
        }

        // ステータスの設定
        var status = playerObj.GetComponent<Character_Status>();
        if (status != null) status.playerID = pID;

        // 入力スクリプトの設定
        var input = playerObj.GetComponent<InputPlayer>();
        if (input != null) { input.SetupDynamicReferences(normal, reason); }// 先にモデルを紐付ける
	}

    //更新
    void Update()
    {
        int aliveCount = 0; // 生存しているプレイヤーの数をカウント
        int last_player_id = 0;  // 最後まで残ったプレイヤーの番号を保持;
        Character_Status survivorStatus = null;//生き残っているプレイヤーのコンポーネントを保持する変数

		foreach (var player in spawnedPlayers)
        {
            if (player != null && !player.IsDead)
            {
                aliveCount++;
                last_player_id = player.playerID;
                survivorStatus = player; // 生きているプレイヤーのStatusを上書きして保持
            }
        }
        playerCount = aliveCount;

        // プレイヤーが1人になった瞬間、ゲーム終了の処理を開始
        if (GameDataManager.SelectedPlayerCount > 1 && playerCount == 1 && !isGameEnd)
        {
            Debug.Log("決着！リザルトシーンへ移動します。");

            isGameEnd = true;
            lastPlayer = last_player_id;

            // ここで最後の一人のGameObjectを取得できます
            if (survivorStatus != null)
            {
                GameObject winnerObject = survivorStatus.gameObject;
                Debug.Log("優勝したオブジェクトの名前: " + winnerObject.name);
                // スロー演出からズーム、リザルト表示までの全流れを開始
                StartCoroutine(VictorySequenceRoutine(survivorStatus));
            }
        }
    }
    public void SetDiePlayerList(int player_id) { diedPlayer.Add(player_id); }

    //決着からリザルト表示までの一連の演出を行うコルーチン
    private IEnumerator VictorySequenceRoutine(Character_Status survivor)
    {
        // --- 1. トドメの瞬間：スロー開始 ---

        // 全プレイヤーの入力を無効化して、スロー演出の準備
        foreach (var player in spawnedPlayers) { player.GetComponent<InputPlayer>().enabled = false; }

        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM();//静寂の演出のためにBGMを止める
        Time.timeScale = 0.02f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;    // 物理演算の更新間隔もスローに同期させる
		AudioManager.Instance.PlaySEByIndex(15, 10);     // トドメのSE（15番）を大きめの音量で鳴らす

        //カメラ関連の演出準備：全カメラの設定を保存しておく
        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        CameraClearFlags[] originalFlags = new CameraClearFlags[allCameras.Length];
        Color[] originalBgColors = new Color[allCameras.Length];
        Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        List<ParticleSystem> pausedParticles = new List<ParticleSystem>();
        List<Renderer> hiddenRenderers = new List<Renderer>(); // 一時的に消す地面やステージオブジェクトのリスト
		//演出
		ApplyFinishingVisuals(allCameras, originalFlags, originalBgColors, originalMaterials, hiddenRenderers);
        yield return new WaitForSecondsRealtime(1.5f);

        // ---  復活：すべて元通りにする ---
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.victoryBGM);// BGMを勝利用に切り替える
            yield return new WaitForSeconds(1.5f);
        }
        //元に戻す
        RevertFinishingVisuals(allCameras, originalFlags, originalBgColors, originalMaterials, hiddenRenderers);

        // ---  ポーズ演出 ---
        if (uiManager != null)
        {
            uiManager.ShowVictoryGraphic();        // UIを切り替える
            uiManager.HideAllInGameUI();
        }
        survivor.GetComponent<InputPlayer>()?.Win();    // 勝者のプレイヤーに勝利演出をさせる

        // --- リザルトへ ---
        CharaType winnerType = survivor.CharaAnim;
        AudioManager.Instance.PlaySEByIndex(26, 2.0f); //勝者エニモは・・・（ナレーション）

        switch (survivor.CharaAnim)//エニモごとの鳴らすタイミングを変える
        {
            case CharaType.RHINOCELOS: yield return new WaitForSecondsRealtime(1.6f); break;
            case CharaType.RATEL: yield return new WaitForSecondsRealtime(1.1f); break;
            default: yield return new WaitForSecondsRealtime(1.4f); break;
        }

        diedPlayer.Add(lastPlayer);
        int[] ranking = diedPlayer.ToArray();
        Array.Reverse(ranking);
        uiManager.ShowResult(ranking, Animal_Select.playerChoices);

        if (AudioManager.Instance != null)PlayWinnerVoice(survivor.CharaAnim);// 勝者の動物タイプに応じた勝利ボイスを鳴らす
		if (backTitleAuto != null) backTitleAuto.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.0f);

        if (endManager != null) endManager.SetEndFlag(true);

        // 最後に残ったプレイヤーの入力を有効化して、エンドマネージャーに遷移フラグを渡す
        foreach (var p in spawnedPlayers) { p.GetComponent<InputPlayer>().enabled = true; }
    }

    // 特殊演出処理
    private void ApplyFinishingVisuals(Camera[] cameras, CameraClearFlags[] oFlags,
    Color[] oColors, Dictionary<Renderer, Material[]> oMats, List<Renderer> hidden)
    {
        Shader standardShader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
        Material blackMat = new Material(standardShader) { color = Color.black };   //黒のマテリアル

        // カメラ背景を赤にする
        for (int i = 0; i < cameras.Length; i++)
        {
            oFlags[i] = cameras[i].clearFlags;
            oColors[i] = cameras[i].backgroundColor;
            cameras[i].clearFlags = CameraClearFlags.SolidColor;
            cameras[i].backgroundColor = Color.red;
        }

        //ステージ非表示＆キャラ黒化
        Renderer[] allRenderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var r in allRenderers)
        {
            if (!r.enabled) continue;
            bool isPlayer = false;
            foreach (var p in spawnedPlayers)
            {
                if (p != null && r.transform.IsChildOf(p.transform)) isPlayer = true;
            }

            if (isPlayer)
            {
                oMats[r] = r.materials;//プレイヤーを黒くさせる
                Material[] blackMats = new Material[r.materials.Length];
                for (int j = 0; j < blackMats.Length; j++) blackMats[j] = blackMat;
                r.materials = blackMats;
            }
            else { r.enabled = false; hidden.Add(r); }
        }

        //パーティクルの停止
        foreach (var p in spawnedPlayers)
        {
            if (p == null) continue;
            foreach (var particle in p.GetComponentsInChildren<ParticleSystem>())
            {
                if (particle.isPlaying || particle.particleCount > 0)
                {
                    if (!particle.main.loop)
                    {
                        particle.Pause();
                        var psr = particle.GetComponent<ParticleSystemRenderer>();
                        if (psr != null) psr.enabled = false;
                    }
                    else Destroy(particle.gameObject);
                }
            }

		}
    }

    //特殊演出を戻す処理
    private void RevertFinishingVisuals(Camera[] cameras, CameraClearFlags[] oFlags,
    Color[] oColors, Dictionary<Renderer, Material[]> oMats, List<Renderer> hidden)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].clearFlags = oFlags[i];
            cameras[i].backgroundColor = oColors[i];
        }

        foreach (var kvp in oMats)
        {
            if (kvp.Key != null) kvp.Key.materials = kvp.Value;
        }

        foreach (var r in hidden)
        {
            if (r != null) r.enabled = true;
        }

        //プレイヤーの子オブジェクトにあるパーティクルも非表示解除して再生させる
        foreach (var p in spawnedPlayers)
        {
            if (p == null) continue;
            foreach (var particle in p.GetComponentsInChildren<ParticleSystem>())
            {
                var psr = particle.GetComponent<ParticleSystemRenderer>();
                if (psr != null) psr.enabled = true;
                particle.Play();
            }
        }
    }

    // 勝者の動物タイプからボイスを判別して鳴らすメソッド
    private void PlayWinnerVoice(CharaType winnerType)
    {
        int voiceIndex = -1;

        // 動物タイプに応じてSEインデックスを指定
        switch (winnerType)
        {
            case CharaType.LION:
                voiceIndex = 21; // ライオン勝利
                break;
            case CharaType.OSTRICH:
                voiceIndex = 22; // ダチョウ勝利
                break;
            case CharaType.RHINOCELOS:
                voiceIndex = 23; // サイ勝利
                break;
            case CharaType.RATEL:
                voiceIndex = 24; // ラーテル勝利
                break;
        }
        if (voiceIndex != -1) { AudioManager.Instance.PlaySEByIndex(voiceIndex, 2.0f); }// 勝利ボイス
    }
}