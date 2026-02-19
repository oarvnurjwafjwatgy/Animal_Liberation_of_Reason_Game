using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
	[Header("設定")]
	public string nextSceneName = "PlayerCountSelect";		 // キャラ選択シーンの名前
	public CanvasGroup pressAnyButtonCG;					 // 明滅させたいUIのCanvasGroup
	public float flashSpeed = 2.0f;							 // 明滅の速さ

	private bool isTransitioning = false;
	private bool canInput = false; // 入力許可フラグ
	private float inputTimer = 0f;

	// 動画が終わった時に外部（VideoHandler）から呼ばれる関数
	public void EnableInput()
	{
		canInput = true;
		inputTimer = 0.5f; // 0.5秒の猶予を設ける（動画終了直後の誤入力防止）
	}

	void Update()
	{
		if (!canInput) return; // 入力が許可されていない場合は何もしない

		// タイマーを減らす
		if (inputTimer > 0)
		{
			inputTimer -= Time.deltaTime;
			return;
		}


		// 1. 文字を明滅させる (サイン波を利用)
		if (pressAnyButtonCG != null && !isTransitioning)
		{
			// 0.3 〜 1.0 の間でふわふわさせる
			pressAnyButtonCG.alpha = 0.65f + Mathf.Sin(Time.time * flashSpeed) * 0.35f;
		}

		// 2. シーン遷移（ゲームパッドのボタンのみに限定）
		if (!isTransitioning)
		{
			bool gamepadButtonPressed = false;

			if (Gamepad.current != null)
			{
				// すべてのコントロールの中から「ボタン」かつ「今押された瞬間」だけを探す
				gamepadButtonPressed = Gamepad.current.allControls.Any(c =>
					c is UnityEngine.InputSystem.Controls.ButtonControl b &&
					b.wasPressedThisFrame &&
					!c.synthetic);
			}

			if (gamepadButtonPressed)
			{
				Debug.Log("ゲームパッドのボタン入力を検知！遷移します。");
				StartTransition();
			}
		}
	}

	void StartTransition()
	{
		isTransitioning = true;
		Debug.Log("Scene Transition Start!");

		// ここで決定音を鳴らす処理を入れる！

		// シーン移動
		SceneManager.LoadScene(nextSceneName);
	}
}