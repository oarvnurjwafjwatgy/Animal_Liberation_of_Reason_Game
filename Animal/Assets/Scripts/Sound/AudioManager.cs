using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public AudioSource bgmSource;    // BGM用スピーカー
	public AudioSource seSource;     // SE用スピーカー

	[Header("BGMリスト")]
	public AudioClip menuBGM;    // タイトル～キャラ選択
	public AudioClip battleBGM;  // 戦闘中
	public AudioClip victoryBGM; // 勝利時

	[Header("SEリスト (0:決定, 1:移動, 2:攻撃...)")]
	public AudioClip[] seClips;

	void Awake()
	{
		// シーンを跨いでもこのオブジェクトを消さない
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// 初回の音量強制設定
			ApplyForceVolume();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		// 再生中に勝手に音量を変えられないよう、毎フレーム監視して固定する
		ApplyForceVolume();
	}

	// 音量を強制適用する関数（1.5fはかなりの爆音設定です）
	private void ApplyForceVolume()
	{
		if (bgmSource != null && bgmSource.volume != 0.1f) bgmSource.volume = 0.1f;
		if (seSource != null && seSource.volume != 1.5f) seSource.volume = 1.5f;
	}

	// --- BGM再生 ---
	public void PlayBGM(AudioClip clip)
	{
		if (clip == null) return;

		// 「同じ曲」かつ「既に再生中」なら何もしない
		// これにより、停止している状態（動画明け）なら同じ曲でも再生されます
		if (bgmSource.clip == clip && bgmSource.isPlaying) return;

		bgmSource.clip = clip;
		bgmSource.Play();
	}

	public void StopBGM()
	{
		bgmSource.Stop();
	}

	// --- SE再生（通常：重なりOK） ---
	public void PlaySE(AudioClip clip)
	{
		if (clip == null || seSource == null) return;
		seSource.PlayOneShot(clip);
	}

	// --- SE再生（番号指定：重なりOK） ---
	public void PlaySEByIndex(int index)
	{
		if (seClips == null || index < 0 || index >= seClips.Length || seClips[index] == null) return;

		// PlayOneShotで再生（これが標準）
		seSource.PlayOneShot(seClips[index]);

		// 【デバッグ用】もし音が小さすぎるなら、BGMと同じ方式も同時に試す（不要なら消してOK）
		// seSource.clip = seClips[index];
		// seSource.Play();
	}
}