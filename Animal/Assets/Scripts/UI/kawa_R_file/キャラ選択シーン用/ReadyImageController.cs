using System.Collections;
using UnityEngine;

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
		rectTransform = GetComponent<RectTransform>();  // RectTransformを取得

		// --- 設定値 ---
		// 画面外の位置
		offScreenPosition = new Vector2(2000f, rectTransform.anchoredPosition.y);
		// 画面内の定位置
		targetPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
		// --------------

		// 最初は画面外に隠す
		rectTransform.anchoredPosition = offScreenPosition;
		rectTransform.localScale = Vector3.one;

		// --- フェード用CanvasGroupの初期設定 ---
		if (fadeCanvasGroup != null)
		{
			fadeCanvasGroup.gameObject.SetActive(false);
			fadeCanvasGroup.alpha = 0f; // 透明にして隠す
		}
	}

	// 全員Readyになった時に呼ぶメソッド
	public void SlideIn()
	{
		// すでにReady状態なら何もしない
		if (isReady) return;
		StopAllCoroutines();    // もし前の動作が残っていたら止める
		StartCoroutine(MoveImage(offScreenPosition, targetPosition, 0.4f)); // 画面外から定位置までスライドインする
		isReady = true; // Ready状態にする
	}

	// キャンセルされた時に呼ぶメソッド
	public void SlideOut()
	{
		// すでにReady状態でないなら何もしない
		if (!isReady) return;
		StopAllCoroutines();    // もし前の動作が残っていたら止める
		StartCoroutine(MoveImage(targetPosition, offScreenPosition, 0.3f)); // 定位置から画面外までスライドアウトする
		isReady = false;    // Ready状態を解除する
	}

	// スタートボタンが押された時に呼ぶメソッド
	public void ZoomAndStart()
	{
		StopAllCoroutines();    // もし前の動作が残っていたら止める
		StartCoroutine(ZoomAndFadeOut());   // ズームしてフェードアウトする
	}

	// 画像をスムーズに移動させるコルーチン
	private IEnumerator MoveImage(Vector2 start, Vector2 end, float duration)
	{
		float timer = 0f;   // タイマーをリセット

		// --- スライド演出 ---
		while (timer < duration)
		{
			timer += Time.deltaTime;    // タイマーを進める
			rectTransform.anchoredPosition = Vector2.Lerp(start, end, timer / duration);    // 位置を更新する
			yield return null;  // 次のフレームまで待つ
		}
		rectTransform.anchoredPosition = end;   // 最終的な位置を確実にセットする
	}

	// 画像をズームしてフェードアウトさせるコルーチン
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
		// (※暗転まで実装出来なかった。変にコメントアウトして処理が壊れるのを防ぐためこのままにします)
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
	}
}