using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;


public class BackTitleAuto : MonoBehaviour
{
    private float nonInputTimer = 0f;       // 未入力タイマー
    private const float nonInputTime = 60f; // 未入力で切り替わる時間

    // Update is called once per frame
    void Update()
    {
        this.UpdateTimer();
    }

    // 未入力タイマーの更新
    private void UpdateTimer()
    {
        // 何か入力されたらタイマーリセット
        if (CheckInputAny())
        {
            nonInputTimer = 0f;
        }
        else
        {
            nonInputTimer += Time.deltaTime;
            // タイマーが既定時間をすぎた時
            if (nonInputTimer > nonInputTime)
            {
                // タイトルシーンへ遷移
                SceneManager.LoadScene("TitleScene");
                Debug.Log("未入力のため、タイトルへ戻ります");
            }
        }

    }

    // コントローラー入力検知
    // return   何かしら入力されたらtrueを返す
    private bool CheckInputAny()
    {
        bool tmp = false;

        // 接続されている全Gamepadをチェック
        foreach (var gamepad in Gamepad.all)
        {
            // 何かボタンが押された瞬間
            if (gamepad.allControls.Any(control => control is ButtonControl button && button.wasPressedThisFrame))
            {
                tmp = true;
                Debug.Log("どれかのコントローラーで入力されました");
                break;
            }
        }

        return tmp;
    }
}
