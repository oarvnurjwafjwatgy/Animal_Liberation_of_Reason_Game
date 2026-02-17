using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ReturnToMenu : MonoBehaviour
{
	[Header("設定")]
	public string menuSceneName = "PlayerCountSelect";   // 人数選択シーンの名前
	public float holdDuration = 1.0f;                    // 何秒押し続けるか

	[Header("UI設定（マスク方式）")]
	[SerializeField] private GameObject uiRoot;          // Arrow_Root を入れる
	[SerializeField] private Image fillImage;            // 子要素の「白い画像（Fill_Image）」を入れる

	private float timer = 0f;                            // 押し時間を計るタイマー

	void Start()
	{
		// 初期状態ではUIを非表示にする
		if (uiRoot != null) uiRoot.SetActive(false);
	}

	//更新
	void Update()
	{
		//長押しフラグ
		bool isHolding = false;

		// 全コントローラーをチェック
		foreach (var pad in Gamepad.all)
		{
			// Bボタン（East）が押し続けられているか
			if (pad.buttonEast.isPressed)
			{
				isHolding = true;
				break;
			}
		}

		// キーボードの Escキー も長押し対象にする
		if (Keyboard.current.escapeKey.isPressed) isHolding = true;

		//長押しをしてる際の処理
		if (isHolding)
		{
			timer += Time.deltaTime; // 押している間タイマーを加算

			if (uiRoot != null) uiRoot.SetActive(true);
		}
		else
		{
			// 押していないときはタイマーを減算（2倍速で減る）
			timer -= Time.deltaTime * 2.0f;

			// 0以下になったらUIを隠す
			if (timer <= 0f)
			{
				timer = 0f;
				if (uiRoot != null) uiRoot.SetActive(false);
			}
		}
		// 値のクランプ（0〜holdDurationの間に収める）
		timer = Mathf.Clamp(timer, 0f, holdDuration);

		// 中身のFill画像の FillAmount を更新
		if (fillImage != null)
		{
			fillImage.fillAmount = timer / holdDuration;
		}

		// 指定した時間を超えたら戻る
		if (timer >= holdDuration)
		{
			Return();
		}


		//人数選択画面に戻る
		void Return()
		{
			// 次のシーンでの誤作動を防ぐため、データをリセット
			for (int i = 0; i < Animal_Select.playerChoices.Length; i++)
			{
				Animal_Select.playerChoices[i] = Character_Status.CharacterType.NONE;
				Animal_Select.playerPositions[i] = 0;
			}

			// 遷移フラグなどの初期化
			if (Animal_Select.readyImage != null) Animal_Select.readyImage.SetActive(false);

			Debug.Log("<color=red>長押し検知：人数選択に戻ります</color>");

			// タイマーをリセットしてからシーン移動
			timer = 0f;
			SceneManager.LoadScene(menuSceneName);
		}
	}
}