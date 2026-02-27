using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [Header("設定")]
    public string nextSceneName = "PlayerCountSelect";
    public CanvasGroup pressAnyButtonCG;
    public float flashSpeed = 2.0f;
    public float idleTimeout = 30.0f; // 放置とみなす時間

    private bool isTransitioning = false;
    private bool canInput = false;
    private float inputTimer = 0f;
    private float idleTimer = 0f; // 放置時間を計るタイマー

    public void EnableInput()
    {
        canInput = true;
        inputTimer = 0.5f;
        idleTimer = 0f; // 入力可能になった瞬間からカウント開始
    }

    void Update()
    {
        if (!canInput || isTransitioning) return;

        // 1. 文字の明滅処理
        if (pressAnyButtonCG != null)
        {
            pressAnyButtonCG.alpha = 0.65f + Mathf.Sin(Time.time * flashSpeed) * 0.35f;
        }

        // 2. 入力待機タイマーの更新
        if (inputTimer > 0)
        {
            inputTimer -= Time.deltaTime;
            return;
        }

        // --- 放置タイマーのカウント ---
        idleTimer += Time.deltaTime;

        // 30秒経過したらシーンを再読み込みして動画に戻る
        if (idleTimer >= idleTimeout)
        {
            Debug.Log("30秒間入力がなかったため、動画に戻ります");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // 3. 入力検知
        bool gamepadButtonPressed = false;
        if (Gamepad.current != null)
        {
            gamepadButtonPressed = Gamepad.current.allControls.Any(c =>
                c is UnityEngine.InputSystem.Controls.ButtonControl b &&
                b.wasPressedThisFrame &&
                !c.synthetic);
        }

        if (gamepadButtonPressed)
        {
            idleTimer = 0f; // ボタンが押されたのでタイマーをリセット
            Debug.Log("ゲームパッドのボタン入力を検知！遷移します。");
            StartTransition();
        }
    }

    void StartTransition()
    {
        isTransitioning = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySEByIndex(0);
        }
        SceneManager.LoadScene(nextSceneName);
    }
}