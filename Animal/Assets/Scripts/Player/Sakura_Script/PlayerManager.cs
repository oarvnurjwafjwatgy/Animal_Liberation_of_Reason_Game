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
一部の担当者:川上 流輝 */

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
    // ★追加: 演出専用カメラの参照
    [SerializeField] private Camera introCamera;

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

    //InputPlayer input_player;

    void Start()
    {
        Time.timeScale = 1.0f; // 念のため、ゲーム開始時にタイムスケールをリセット
        Time.fixedDeltaTime = 0.02f; // ★物理演算もリセット
        int playersToSpawn = GameDataManager.SelectedPlayerCount;
        playerCount = playersToSpawn;
        isGameEnd = false;
        lastPlayer = 0;
        var gamepads = Gamepad.all;

        spawnedPlayers.Clear(); // 既存のプレイヤーリストをクリア
        playerCameras.Clear(); // ★リストをクリア

        // スポーン位置をシャッフルする
        for (int i = PlayerTransforms.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            if (i == j) continue;
            Transform temp = PlayerTransforms[i];
            PlayerTransforms[i] = PlayerTransforms[j];
            PlayerTransforms[j] = temp;
        }

        // 戦闘シーンが始まったら、BGMを戦闘用に切り替える
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.battleBGM);
        }

        // プレイヤーの生成ループ
        for (int i = 1; i <= playersToSpawn; i++)
        {
            // 1. 選択された動物のタイプを取得 (1Pなら index 1)
            CharaType selectedType = Animal_Select.playerChoices[i];

            // NONE（未選択）の場合は生成をスキップ
            if (selectedType == CharaType.NONE) continue;

            // Enumをintに変換してプレハブのインデックスとして使用
            int animalIndex = (int)selectedType - 1;

            // 2. プレイヤーの土台（カメラや移動スクリプト入り）を生成
            int padIndex = i - 1;
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerBasePrefab,
                playerIndex: padIndex,
                controlScheme: "Gamepad",
                pairWithDevice: (padIndex < gamepads.Count) ? gamepads[padIndex] : null
            );

            // 3. 動物モデル（通常・理性）を生成し、プレイヤーの子にする
            GameObject normalModel = Instantiate(AnimalPrefabs[animalIndex], newPlayer.transform);
            GameObject reasonModel = Instantiate(AnimalReasonPrefabs[animalIndex], newPlayer.transform);
            reasonModel.SetActive(false); // 理性モデルは最初はオフ

            // 4. 各コンポーネントに生成したモデルを登録する
            SetupPlayer(newPlayer.gameObject, i, normalModel, reasonModel);

            if (uiManager != null && uiPositions.Count >= i)
            {
                // 生成したプレイヤーのステータスをリストに追加
                var status = newPlayer.GetComponent<Character_Status>();
                Slider hp = uiManager.CreateUI(UIManager.UI_ID.GAUGE_HP, uiPositions[padIndex], i);
                Slider rs = uiManager.CreateUI(UIManager.UI_ID.GAUGE_REASON, uiPositions[padIndex], i);
                if (status != null)
                {
                    status.ReInitialize(i); // プレイヤーIDを設定
                    spawnedPlayers.Add(status);
                    //実際のデータが入っている uiPositions[padIndex] を渡す
                    status.SetUIComponents(hp, rs, uiManager, uiPositions[padIndex]);
                }

                // 5. 初期位置へ移動
                if (PlayerTransforms[padIndex] != null)
                {
                    newPlayer.transform.position = PlayerTransforms[padIndex].position;
                    newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
                }
            }
        }

        // プレイヤー生成後、演出を開始
        StartCoroutine(BattleStartSequence());
    }

    // 戦闘開始の演出を行うコルーチン
    IEnumerator BattleStartSequence()
    {
        // 1. 全プレイヤーの入力を一時的に無効化
        foreach (var p in spawnedPlayers)
        {
            p.GetComponent<InputPlayer>().enabled = false;
        }

        // 2. 演出開始
        if (uiManager != null) uiManager.ShowIntroductionPanel();

        // 演出カメラを有効化
        if (introCamera != null) introCamera.enabled = true;

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

        // 演出カメラをオフにする
        if (introCamera != null) introCamera.enabled = false;

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
        foreach (var p in spawnedPlayers)
        {
            p.GetComponent<InputPlayer>().enabled = true;
        }
    }

    //カウントダウンに使用する設定(seNumber:27は無音)
    private void SetCountDownPreparation(string contents, Color color, int seNumber =27, float seVol = 5f)
    {
        uiManager.SetCountdownColor(color);
        uiManager.ShowCountdown(contents);
        AudioManager.Instance.PlaySEByIndex(seNumber, seVol);
    }

    private void SetupPlayer(GameObject playerObj, int pID, GameObject normal, GameObject reason)
    {
        // カメラは子オブジェクトの0番目
        if (playerObj.transform.childCount > 0)
        {
            GameObject camObj = playerObj.transform.GetChild(0).gameObject;
            // ★追加: リストに追加
            playerCameras.Add(camObj);

            // Viewportスクリプトの制御
            var v1 = camObj.GetComponent<ChangeViewport1p>();
            var v2 = camObj.GetComponent<ChangeViewport2p>();

            if (v1 != null) v1.enabled = (pID == 1);
            if (v2 != null) v2.enabled = (pID == 2);
        }

        // ステータスの設定
        var status = playerObj.GetComponent<Character_Status>();
        if (status != null) status.playerID = pID;

        // 入力スクリプトの設定
        var input = playerObj.GetComponent<InputPlayer>();
        if (input != null)
        {
            // 先にモデルを紐付ける
            input.SetupDynamicReferences(normal, reason);
        }
    }

    //更新
    void Update()
    {
        int aliveCount = 0; // 生存しているプレイヤーの数をカウント
        int last_player_id = 0;  // 最後まで残ったプレイヤーの番号を保持;
                                 // ★追加: 生き残っているプレイヤーのコンポーネントを保持する変数
        Character_Status survivorStatus = null;

        foreach (var player in spawnedPlayers)
        {
            if (player != null && !player.IsDead)
            {
                aliveCount++;
                last_player_id = player.playerID;
                // ★追加: 生きているプレイヤーのStatusを上書きして保持
                survivorStatus = player;
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

    public void SetDiePlayerList(int player_id){ diedPlayer.Add(player_id); }

    //決着からリザルト表示までの一連の演出を行うコルーチン
    private IEnumerator VictorySequenceRoutine(Character_Status survivor)
    {
        // --- 1. トドメの瞬間：スロー開始 ---

        // 全プレイヤーの入力を無効化して、スロー演出の準備
        foreach (var player in spawnedPlayers)
        {
            player.GetComponent<InputPlayer>().enabled = false;
        }

        //静寂の演出のためにBGMを止める
        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM();

        Time.timeScale = 0.02f;
        // ビルド対策：物理演算の更新間隔もスローに同期させる
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        AudioManager.Instance.PlaySEByIndex(15, 10);     // トドメのSE（15番）を大きめの音量で鳴らす

        //カメラ関連の演出準備：全カメラの設定を保存しておく
        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        CameraClearFlags[] originalFlags = new CameraClearFlags[allCameras.Length];
        Color[] originalBgColors = new Color[allCameras.Length];
        Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        List<ParticleSystem> pausedParticles = new List<ParticleSystem>();

        // 一時的に消す地面やステージオブジェクトのリスト
        List<Renderer> hiddenRenderers = new List<Renderer>();

        Shader standardShader = Shader.Find("Unlit/Color"); //Unit/Color シェーダーを探す

        //nullならビルドに入るシェーダーで代用
        if (standardShader == null)
        {
            // もし Unlit/Color が見つからなければ、絶対ビルドに入るシェーダーで代用
            standardShader = Shader.Find("Sprites/Default");
        }
        Material blackMat = new Material(standardShader) { color = Color.black };   // 黒いマテリアルを作成


        // --- 2. 演出：背景赤、キャラ黒、エフェクトと地面を消す ---
        for (int i = 0; i < allCameras.Length; i++)
        {
            originalFlags[i] = allCameras[i].clearFlags;
            originalBgColors[i] = allCameras[i].backgroundColor;
            allCameras[i].clearFlags = CameraClearFlags.SolidColor;
            allCameras[i].backgroundColor = Color.red;
        }

        // ステージ上の「地面」や「障害物」をタグや名前で探して非表示にする
        // "Stage"タグがついているか、名前が"Floor"などのものを想定
        Renderer[] allRenderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var r in allRenderers)
        {
            // プレイヤー関係のRenderer以外をすべて非表示にする
            bool isPlayer = false;
            foreach (var p in spawnedPlayers)
            {
                // プレイヤーのRendererかどうかを判定するために、プレイヤーオブジェクトの子かどうかをチェック
                if (p != null && r.transform.IsChildOf(p.transform)) isPlayer = true;
            }

            // プレイヤー関係のRenderer以外で、かつ有効なものを非表示にする
            if (!isPlayer && r.enabled)
            {
                r.enabled = false; // 地面や背景を消す！
                hiddenRenderers.Add(r); // 復活させるときのためにリストに追加
            }
        }

        // プレイヤーのキャラをすべて黒くして、エフェクトを停止する
        foreach (var p in spawnedPlayers)
        {
            if (p == null) continue;

            // キャラを黒くする
            Renderer[] rs = p.GetComponentsInChildren<Renderer>();
            foreach (var r in rs)
            {
                originalMaterials[r] = r.materials;
                Material[] blackMats = new Material[r.materials.Length];
                for (int j = 0; j < blackMats.Length; j++) blackMats[j] = blackMat;
                r.materials = blackMats;
            }

            // エフェクト停止
            ParticleSystem[] ps = p.GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in ps)
            {
                if (particle.isPlaying || particle.particleCount > 0)
                {
                    if (!particle.main.loop)
                    {
                        particle.Pause();
                        var psRenderer = particle.GetComponent<ParticleSystemRenderer>();
                        if (psRenderer != null) psRenderer.enabled = false;
                        //particle.GetComponent<Renderer>().enabled = false;
                        //pausedParticles.Add(particle);
                    }
                    else Destroy(particle.gameObject);  //ループ系のエフェクトは破壊
                }
            }
        }

        yield return new WaitForSecondsRealtime(1.5f);

        // --- 3. 復活：すべて元通りにする ---
        // ビルド対策：重い復元処理の前に時間を戻す
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        // BGMを勝利用に切り替える
        if (AudioManager.Instance != null)
        {
            //勝利BGM
            AudioManager.Instance.PlayBGM(AudioManager.Instance.victoryBGM);
            yield return new WaitForSeconds(1.5f);
        }

        // カメラの設定を元に戻す
        for (int i = 0; i < allCameras.Length; i++)
        {
            allCameras[i].clearFlags = originalFlags[i];
            allCameras[i].backgroundColor = originalBgColors[i];
        }

        // プレイヤーのキャラを元のマテリアルに戻す
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null) kvp.Key.materials = kvp.Value;
        }

        // エフェクトを再生する
        foreach (var particle in pausedParticles)
        {
            if (particle != null)
            {
                var psRenderer = particle.GetComponent<ParticleSystemRenderer>();
                if (psRenderer != null) psRenderer.enabled = true;
                //particle.GetComponent<Renderer>().enabled = true;
                particle.Play();
            }
        }

        // 消していた地面やステージを復活させる
        foreach (var r in hiddenRenderers)
        {
            if (r != null) r.enabled = true;
        }

        // --- 4. ポーズ演出 ---
        // UIを切り替える
        if (uiManager != null)
        {
            uiManager.ShowVictoryGraphic();
            uiManager.HideAllInGameUI();
        }
        survivor.GetComponent<InputPlayer>()?.Win();    // 勝者のプレイヤーに勝利演出をさせる

        // --- 5. リザルトへ ---
        CharaType winnerType = survivor.CharaAnim;
        //勝者エニモは・・・（ナレーション）
        AudioManager.Instance.PlaySEByIndex(26, 2.0f);

        //エニモごとの鳴らすタイミングを変える
        switch (survivor.CharaAnim)
        {
            case CharaType.RHINOCELOS:
                yield return new WaitForSecondsRealtime(1.6f);
                break;
            case CharaType.RATEL:
                yield return new WaitForSecondsRealtime(1.1f);
                break;
            default:
                yield return new WaitForSecondsRealtime(1.4f);
                break;
        }

        diedPlayer.Add(lastPlayer);
        int[] ranking = diedPlayer.ToArray();
        Array.Reverse(ranking);
        uiManager.ShowResult(ranking, Animal_Select.playerChoices);

        // 勝者の動物タイプに応じた勝利ボイスを鳴らす
        if (AudioManager.Instance != null)
            PlayWinnerVoice(survivor.CharaAnim);    //勝った動物の名前をナレーションするSEを鳴らす


        if (backTitleAuto != null)
            backTitleAuto.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(1.0f);
        if (endManager != null) endManager.SetEndFlag(true);

        // 最後に残ったプレイヤーの入力を有効化して、エンドマネージャーに遷移フラグを渡す
        foreach (var p in spawnedPlayers)
        {
            p.GetComponent<InputPlayer>().enabled = true;
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

		// 有効なインデックスが設定されていれば、勝利ボイスを鳴らす
		if (voiceIndex != -1)
        {
			// 勝利ボイス
			AudioManager.Instance.PlaySEByIndex(voiceIndex, 2.0f);
        }
    }
}