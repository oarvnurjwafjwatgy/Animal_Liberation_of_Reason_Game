using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;   // UIマネージャーの参照
    [SerializeField] private float endWaitTime;     // ゲーム終了後演出時間
    private bool endFlag;       // ゲーム終了フラグ
    private float endWaitTimer; // ゲーム終了後演出タイマー 

    // Start is called before the first frame update
    void Start()
    {
        endWaitTime = 5f;
        endFlag = false;
        endWaitTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateTimer();
    }

    // ゲーム終了フラグの設定
    // flag     ゲームが終了したなら、trueを与える
    public void SetEndFlag(bool flag)
    {
        endFlag = flag;
    }

    private void UpdateTimer()
    {
        if (!endFlag) return;

        endWaitTimer += Time.deltaTime;
        if (endWaitTimer > endWaitTime)
        {
            if (uiManager != null)
                uiManager.SetTitleButton();
            Debug.Log("<color=#c0ffff>タイトルへ戻るボタンを表示</color>");
        }
        Debug.Log("<color=#80ffff>Timer:" +  endWaitTime + "</color>");
    }
}
