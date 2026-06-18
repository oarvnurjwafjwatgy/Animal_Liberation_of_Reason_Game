using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public enum UI_ID { GAUGE_HP, GAUGE_REASON, LION_RAGE, BUFF_CONTAINER }
	public List<GameObject> ui_list = new List<GameObject>();
	public Transform canvasParent;

    // キャラごとのRenderTexture(0～3)
    [SerializeField] private RenderTexture[] loseCharaRT;

    // 順位表示用RawImage(0:2位, 1:3位, 2:4位)
    [SerializeField] private RawImage[] rankImage;

	// タイトルへ戻るボタン
	[SerializeField] private GameObject titleButton;

	// 最初に選択されるボタン
    [SerializeField] private GameObject firstSelectedButton;

	[SerializeField] private GameObject victoryGroup; // VictoryUIをアサイン

	[Header("開始演出用UI")]
	[SerializeField] private TMPro.TextMeshProUGUI countdownText; // 中央のテキスト
	[Header("紹介演出用UI")]
	[SerializeField] private GameObject introductionPanel; // 全画面を隠す黒いPanel
	[Header("ゲーム画面UIグループ")]
[SerializeField] private GameObject inGameUIGroup; // 各プレイヤーのHPゲージなどがまとまった親オブジェクト



	void Start()
	{
		// シーン開始時に確実に隠す
		victoryGroup.SetActive(false);
	}

	// UIの生成
	public Slider CreateUI(UI_ID ui_id, Transform pos, int pID) // pIDを追加
	{
		GameObject prefab = null;
		if (ui_id == UI_ID.GAUGE_HP) prefab = Resources.Load("Prefab/UI/HP_ber") as GameObject;
        else if (ui_id == UI_ID.GAUGE_REASON) prefab = Resources.Load("Prefab/UI/Reason_ber") as GameObject;
        else if (ui_id == UI_ID.LION_RAGE) prefab = Resources.Load("Prefab/UI/Lion_Rage_Icon") as GameObject;
		else if (ui_id == UI_ID.BUFF_CONTAINER)prefab = Resources.Load("Prefab/UI/Buff_Container") as GameObject;

		if (prefab != null)
		{
			GameObject uiObj = GameObject.Instantiate(prefab);
			uiObj.transform.SetParent(canvasParent, false);
			uiObj.transform.SetAsLastSibling();

			RectTransform rect = uiObj.GetComponent<RectTransform>();
			if (rect != null)
			{
				// 全体のサイズを少し小さくして画面分割に対応
				rect.localScale = new Vector3(0.8f, 0.8f, 1.0f);
				rect.position = pos.position;

				// 理性ゲージ（Reason）ならHPバーの上に少し重ねる
				if (ui_id == UI_ID.GAUGE_REASON)
				{
					rect.anchoredPosition += new Vector2(0f, 19f);
					rect.localScale = new Vector3(0.8f, 0.6f, 1.0f); // 少し細長く
				}
				else if (ui_id == UI_ID.LION_RAGE)
				{
					// バーの左側にずらす。
					rect.anchoredPosition += new Vector2(-200f, -120f);
					rect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				}
				else if (ui_id == UI_ID.BUFF_CONTAINER)
				{
					rect.anchoredPosition += new Vector2(-80f, -100f);
					rect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				}
			}
			ui_list.Add(uiObj);
			return uiObj.GetComponent<Slider>(); // Sliderコンポーネントを返す
		}
		return null;
	}

	public void ShowCountdown(string text)
	{
		if (countdownText != null)
		{
			countdownText.text = text;
			countdownText.enabled = true;
			// 文字サイズを一旦小さくして、バウンドさせる演出をここに入れる
			StartCoroutine(AnimateCountdown());
		}
	}

	// カウントダウンの非表示
	public void HideCountdown()
	{
		if (countdownText != null)
		{
			countdownText.enabled = false;
		}
	}

	// 紹介パネルの表示
	public void ShowIntroductionPanel()
	{
		if (introductionPanel != null) introductionPanel.SetActive(true);
	}

	// 紹介パネルの非表示
	public void HideIntroductionPanel()
	{
		if (introductionPanel != null) introductionPanel.SetActive(false);
	}

	// カウントダウンの色を変更する関数
	public void SetCountdownColor(Color color)
	{
		if (countdownText != null) // textCountDown ではなく countdownText にする
		{
			countdownText.color = color;
		}
	}

	private IEnumerator AnimateCountdown()
	{
		RectTransform rect = countdownText.GetComponent<RectTransform>();
		rect.localScale = Vector3.zero; // 小さいところから

		// ドカンと大きくする
		float elapsed = 0f;
		float duration = 0.2f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, t);
			yield return null;
		}
		rect.localScale = Vector3.one * 1.5f;
		yield return new WaitForSeconds(0.5f);
		rect.localScale = Vector3.one; // 元のサイズに
	}


	// 勝利グラフィックの表示
	public void ShowVictoryGraphic()
	{
		if (victoryGroup != null)
		{
			victoryGroup.SetActive(true);
			// 演出開始！
			StartCoroutine(AnimateVictoryUI());
		}

	}
	private IEnumerator AnimateVictoryUI()
	{
		// 演出対象のRectTransformを取得（victoryGroup自身か、その中のテキスト）
		RectTransform rect = victoryGroup.GetComponent<RectTransform>();

		// --- 1. ドカンと登場（ポップアップ） ---
		rect.localScale = Vector3.zero; // 最初はサイズ0
		float elapsed = 0f;
		float duration = 0.3f; // 0.3秒で巨大化

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime; // スロー中でも動くようにunscaled
			float t = elapsed / duration;

			// 勢いよく出て、少しだけバウンドするような動き
			float bounce = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.2f;
			if (t > 0.8f) bounce = 1.0f + (1.0f - t) * 0.5f; // 最後は1.0に落ち着く

			rect.localScale = new Vector3(bounce, bounce, 1f);
			yield return null;
		}
		rect.localScale = Vector3.one;

		// --- 2. ふわふわと動く（ループ演出） ---
		float timer = 0f;
		Vector2 initialPos = rect.anchoredPosition;

		while (true) // 終了するまでずっと動かす
		{
			timer += Time.unscaledDeltaTime;

			// わずかに拡大縮小
			float pulse = 1.0f + Mathf.Sin(timer * 2f) * 0.05f;
			rect.localScale = new Vector3(pulse, pulse, 1f);

			// わずかに上下に揺れる
			float yOffset = Mathf.Sin(timer * 1.5f) * 10f;
			rect.anchoredPosition = initialPos + new Vector2(0, yOffset);

			yield return null;
		}
	}

	//勝利時のUIを非表示にする
	public void HideAllInGameUI()
	{
		foreach (GameObject ui in ui_list)
		{
			if (ui != null) ui.SetActive(false);
		}
	}

	// リザルトの敗北キャラの設定
	// ranking_index	順位(昇順)
	// player_chara_id	キャラクターのID
	public void ShowResult(int[] ranking_index, Character_Status.CharacterType[] player_chara_id)
	{
		// ranking[0] は1位なのでスキップ
		for (int i = 1; i < ranking_index.Length; i++)
		{
			int player_index = ranking_index[i];                    // 何番プレイヤーか
			int chara_id = (int)player_chara_id[player_index] - 1;  // その人のキャラID

			// テクスチャを適用する
			rankImage[i - 1].texture = loseCharaRT[chara_id];
			rankImage[i - 1].gameObject.SetActive(true);
		}
	}

	public void ShowTitleButtonWithFade()
	{
		if (titleButton != null)
		{
			titleButton.SetActive(true);
			// CanvasGroupコンポーネントがアタッチされている前提
			CanvasGroup cg = titleButton.GetComponent<CanvasGroup>();
			if (cg == null) cg = titleButton.AddComponent<CanvasGroup>();

			StartCoroutine(FadeIn(cg));
		}
	}

	// フェードインのコルーチン
	private IEnumerator FadeIn(CanvasGroup cg)
	{
		float elapsed = 0f;
		float duration = 1.0f; // 1秒かけて表示
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			cg.alpha = Mathf.Clamp01(elapsed / duration);
			yield return null;
		}
		cg.alpha = 1f;

		// ボタンにフォーカスを当てる
		SetTitleButton();
	}

	// タイトルへ戻るボタンのアクティブフラグの設定
	public void SetTitleButton()
	{
		titleButton.SetActive(true);

		// 最初に選択されるボタンを設定する
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

	/********バフ・デバフのアイコンUI**********************************/

	// プレイヤーごとのバフアイコンを管理する辞書(プレイヤーID,バフ管理)
	private Dictionary<int, Dictionary<Character_Status.BuffType, BuffIcon>>
	playerActiveBuffs = new Dictionary<int, Dictionary<Character_Status.BuffType, BuffIcon>>();

	[Header("バフUI用画像（UIManager側で管理）")]
	[SerializeField] private Sprite spdBuffSprite;
	[SerializeField] private Sprite spdDebuffSprite;
	[SerializeField] private Sprite atkBuffSprite;
	[SerializeField] private Sprite atkDebuffSprite;
	[SerializeField] private Sprite lionBurstSpdSprite;

	// バフアイコンの生成・更新関数
	public void CreateOrUpdateBuffUI(int pID, Character_Status.BuffType type,
	float duration, Transform container)
	{
		// pID: プレイヤーID,  type: バフの種類,
		// duration: 残り時間, container: アイコンを配置する親Transform
		if (container == null) return;

		// プレイヤーのバフ管理がなければ作る
		if (!playerActiveBuffs.ContainsKey(pID))
			playerActiveBuffs[pID] = new Dictionary<Character_Status.BuffType, BuffIcon>();

		//適切なスプライトを選択
		Sprite targetSprite = null;
		if (type == Character_Status.BuffType.SpeedBuff) targetSprite = spdBuffSprite;
		if(type ==Character_Status.BuffType.SpeedDebuff) targetSprite = spdDebuffSprite;
		if(type ==Character_Status.BuffType.AttackBuff) targetSprite = atkBuffSprite;
		if(type ==Character_Status.BuffType.AttackDebuff) targetSprite = atkDebuffSprite;

		//サイのダッシュの技もスピードバフのアイコンを流用
		if (type ==Character_Status.BuffType.RhinoDash) targetSprite = spdBuffSprite;

		// まだアイコンがない場合は新規作成/存在する場合は更新
		if (playerActiveBuffs[pID].ContainsKey(type)
		&& playerActiveBuffs[pID][type] != null)
			playerActiveBuffs[pID][type].Setup(targetSprite, duration);
		else
		{
			GameObject go = new GameObject(type.ToString(), typeof(Image), typeof(BuffIcon));
			go.transform.SetParent(container, false);
			go.GetComponent<RectTransform>().localScale =new Vector3(1.3f, 1.3f, 1.3f);

			BuffIcon script = go.GetComponent<BuffIcon>();
			script.Setup(targetSprite, duration);
			playerActiveBuffs[pID][type] = script;
		}
	}

	// バフアイコンの削除関数
	public void RemoveBuffUI(int pID, Character_Status.BuffType type)
	{
		// バフアイコンが存在する場合は削除
		if (playerActiveBuffs.ContainsKey(pID)&&playerActiveBuffs[pID].ContainsKey(type)
		&&playerActiveBuffs[pID][type] !=null)
		{
			playerActiveBuffs[pID][type].ForceDestroy();
			playerActiveBuffs[pID].Remove(type);
		}
	}
}