using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ReadyImageコンポーネントに自動的に追加されるようにする
[RequireComponent(typeof(RectTransform))]
public class ReadyImageController : MonoBehaviour
{
	private RectTransform rectTransform;
	private Vector2 offScreenPosition;
	private Vector2 targetPosition;
	private bool isReady = false;

	// フェード用CanvasGroupコンポーネント
	[Header("フェード用CanvasGroup (FadeImageにアタッチ)")]
	public CanvasGroup fadeCanvasGroup;

	// 画面の幅に合わせて移動先を調整するため、Awakeで使用
	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();

		// --- 設定値 ---
		// 画面外の位置
		offScreenPosition = new Vector2(2000f, rectTransform.anchoredPosition.y);
		// 画面内の定位置
		targetPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
		// --------------

		// 最初は画面外に隠す
		rectTransform.anchoredPosition = offScreenPosition;
		rectTransform.localScale = Vector3.one;

		// --- ★修正箇所: fadeImage ではなく fadeCanvasGroup を使用 ---
		if (fadeCanvasGroup != null)
		{
			fadeCanvasGroup.gameObject.SetActive(false);
			fadeCanvasGroup.alpha = 0f; // 透明にして隠す
		}
		// ---------------------------------------------------------
	}

	// 全員Readyになった時に呼ぶメソッド
	public void SlideIn()
	{
		if (isReady) return;
		StopAllCoroutines();
		// 動作時間を少し長くして余裕を持たせることも可能です (0.3f -> 0.4fなど)
		StartCoroutine(MoveImage(offScreenPosition, targetPosition, 0.4f));
		isReady = true;
	}

	// キャンセルされた時に呼ぶメソッド
	public void SlideOut()
	{
		if (!isReady) return;
		StopAllCoroutines();
		StartCoroutine(MoveImage(targetPosition, offScreenPosition, 0.3f));
		isReady = false;
	}

	// スタートボタンが押された時に呼ぶメソッド
	public void ZoomAndStart()
	{
		StopAllCoroutines();
		StartCoroutine(ZoomAndFadeOut());
	}

	private IEnumerator MoveImage(Vector2 start, Vector2 end, float duration)
	{
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			rectTransform.anchoredPosition = Vector2.Lerp(start, end, timer / duration);
			yield return null;
		}
		rectTransform.anchoredPosition = end;
	}

	private IEnumerator ZoomAndFadeOut()
	{
		float timer = 0f;
		float duration = 0.3f;
		Vector3 startScale = rectTransform.localScale;
		Vector3 endScale = Vector3.one * 100f; // 100倍にズーム

		// --- ズーム演出 ---
		while (timer < duration)
		{
			timer += Time.deltaTime;
			rectTransform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);
			yield return null;
		}
		rectTransform.localScale = endScale;

		// --- イラストが消えるまでの待機 ---
		yield return new WaitForSeconds(0.2f);
		gameObject.SetActive(false); // イラストを非表示に

		// --- 暗転処理 ---
		if (fadeCanvasGroup != null)
		{
			fadeCanvasGroup.gameObject.SetActive(true);
			fadeCanvasGroup.alpha = 0f; // 透明からスタート

			float fadeTimer = 0f;
			float fadeDuration = 1.0f; // 暗転にかかる時間

			Debug.Log("★暗転開始：フェード処理開始");

			while (fadeTimer < fadeDuration)
			{
				fadeTimer += Time.deltaTime;

				// ★重要: アルファ値計算をより単純にしてみる
				float newAlpha = fadeTimer / fadeDuration;
				fadeCanvasGroup.alpha = newAlpha;

				// ログを毎フレーム出さず、特定のタイマーになった時だけ出す
				if (fadeTimer > 0.5f && fadeTimer - Time.deltaTime <= 0.5f)
					Debug.Log("★フェード中間点通過 (0.5秒経過)");

				yield return null;
			}

			fadeCanvasGroup.alpha = 1f; // 完全に不透明にする
			Debug.Log("★暗転処理が完了しました");
		}
	} // ★修正箇所: カッコが足りていない場合があるので、ここで閉じる
} // ReadyImageControllerクラスを閉じるカッコ