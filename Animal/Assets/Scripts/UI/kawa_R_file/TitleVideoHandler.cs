using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

public class TitleVideoHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public GameObject titleUI;
    public AudioClip bgm;

    private bool isVideoPlaying = false;

    void Start()
    {
        // イベント登録をリセット
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        isVideoPlaying = true;
        if (videoDisplay != null) videoDisplay.enabled = true;
        if (titleUI != null) titleUI.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        videoPlayer.Play();
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    void Update()
    {
        if (isVideoPlaying && Gamepad.current != null)
        {
            bool gamepadButtonPressed = Gamepad.current.allControls.Any(c =>
                c is UnityEngine.InputSystem.Controls.ButtonControl b &&
                b.wasPressedThisFrame &&
                !c.synthetic);

            if (gamepadButtonPressed)
            {
                OnVideoEnd(videoPlayer);
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!isVideoPlaying) return;
        isVideoPlaying = false;

        Debug.Log("終了処理：動画をストップします");
        videoPlayer.Stop();
        videoDisplay.enabled = false;

        titleUI.SetActive(true);

        if (AudioManager.Instance != null)
        {
            // --- ここが重要：一度止めてから再生し直す ---
            Debug.Log("BGM再生命令を強制的に送ります");
            AudioManager.Instance.StopBGM(); // 2回目対策：一度完全に止める
            AudioManager.Instance.PlayBGM(bgm);
        }

        TitleMenu menu = titleUI.GetComponent<TitleMenu>();
        if (menu != null) menu.EnableInput();
    }
}