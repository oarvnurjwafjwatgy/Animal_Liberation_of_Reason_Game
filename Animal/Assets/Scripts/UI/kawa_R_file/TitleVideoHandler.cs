using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class TitleVideoHandler : MonoBehaviour
{
	public VideoPlayer videoPlayer;
	public RawImage videoDisplay; // 動画を表示しているRawImage
	public GameObject titleUI;      // ロゴや「PUSH START」などのUI


	GameObject SoundManagerObj;
	SoundManager soundmanager;

	public AudioClip bgm;

	void Start()
	{
		// UIを最初は消しておく（動画に集中させる場合）
		if (titleUI != null) titleUI.SetActive(false);

		// 動画が終了した時のイベントを登録
		videoPlayer.loopPointReached += OnVideoEnd;

		SoundManagerObj = GameObject.Find("SoundManager");
		//soundmanager = SoundManagerObj.GetComponent<SoundManager>();
	}

	void OnVideoEnd(VideoPlayer vp)
	{
		// 動画が終わった時の処理
		Debug.Log("動画再生完了");

		// 1. 動画の表示を消す、またはアルファを下げる
		videoDisplay.enabled = false;

		// 2. タイトルのロゴや「PUSH START」を表示する
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayBGM(bgm);
		}
		else
		{
			Debug.LogError("AudioManagerが見つかりません！タイトルシーンに配置されていますか？");
		}

		titleUI.SetActive(true);
		TitleMenu menu = titleUI.GetComponent<TitleMenu>();
		if (menu != null) menu.EnableInput();
	}

}