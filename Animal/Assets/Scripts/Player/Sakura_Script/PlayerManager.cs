using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class PlayerManager : MonoBehaviour
{
	//生成したプレイヤーを管理するリスト
	private List<Character_Status> spawnedPlayers = new List<Character_Status>();

	[Header("UI設定")]
	[SerializeField] private UIManager uiManager;// UIマネージャーの参照
	[SerializeField] private List<Transform> uiPositions = new List<Transform>();// 1P~4PのUI位置

	[HideInInspector] // インスペクターには出さなくて良い場合はこれをつける
    public int playerCount;

    [Header("プレイヤーの土台プレハブ")]
    [SerializeField] private GameObject PlayerBasePrefab;

    [Header("動物プレハブ設定 (Element 0=LION, 1=OSTRICH...)")]
    [SerializeField] private List<GameObject> AnimalPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> AnimalReasonPrefabs = new List<GameObject>();

    [Header("出現位置")]
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    void Start()
    {
        int playersToSpawn = GameDataManager.SelectedPlayerCount;
        playerCount = playersToSpawn;
        var gamepads = Gamepad.all;

        spawnedPlayers.Clear(); // 既存のプレイヤーリストをクリア

        // プレイヤーの生成ループ
        for (int i = 1; i <= playersToSpawn; i++)
        {
            // 1. 選択された動物のタイプを取得 (1Pなら index 1)
            Character_Status.CharacterType selectedType = Animal_Select.playerChoices[i];

            // NONE（未選択）の場合は生成をスキップ
            if (selectedType == Character_Status.CharacterType.NONE) continue;

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
                    spawnedPlayers.Add(status);
                    status.SetUIComponents(hp, rs);
                }

                // 5. 初期位置へ移動
                if (PlayerTransforms[padIndex] != null)
                {
                    newPlayer.transform.position = PlayerTransforms[padIndex].position;
                    newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
                }
            }
        }
    }

    private void SetupPlayer(GameObject playerObj, int pID, GameObject normal, GameObject reason)
    {
        // カメラは子オブジェクトの0番目
        if (playerObj.transform.childCount > 0)
        {
            GameObject camObj = playerObj.transform.GetChild(0).gameObject;

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
    int aliveCount = 0;     // 生存しているプレイヤーの数をカウント

		// spawnedPlayersリストをループして、生存しているプレイヤーをカウント
		foreach (var player in spawnedPlayers)
        {
			// playerがnullでなく、かつ死亡していない場合はaliveCountを増やす
			if (player != null && !player.IsDead)
                aliveCount++;
        }

        playerCount = aliveCount; // 生存しているプレイヤーの数をplayerCountに反映

		// 残り1人になったらリザルトへ（複数人で始めた場合）
		// GameDataManager.SelectedPlayerCount が 1 より大きいときのみ判定
		if (GameDataManager.SelectedPlayerCount > 1 && playerCount == 1)
		{
			Debug.Log("決着！リザルトシーンへ移動します。");
			//SceneManager.LoadScene("ResultScene");
		}
	}
}