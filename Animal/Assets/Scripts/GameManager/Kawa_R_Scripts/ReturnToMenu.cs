using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 長押しゲージを表示したい場合は必要

public class ReturnToMenu : MonoBehaviour
{
	[Header("設定")]
	public string menuSceneName = "PlayerCountSelect";	 // 人数選択シーンの名前
	public float holdDuration = 1.0f;					 // 何秒押し続けるか

	private float timer = 0f;                            // 押し時間を計るタイマー

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

		if (isHolding)
		{
			timer += Time.deltaTime; // 押している間タイマーを加算

			// 指定した時間を超えたら戻る
			if (timer >= holdDuration)
			{
				Return();
			}
		}
		else
		{
			timer = 0f; // 離したらリセット
		}
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