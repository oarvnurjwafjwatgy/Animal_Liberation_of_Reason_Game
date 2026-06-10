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

	//※ここの箇所は川上流輝が担当しました。

	// ゲーム終了後のタイマー更新
	private void UpdateTimer()
	{
		// ゲーム終了フラグが立っていないなら、処理を行わない
		if (!endFlag) return;

		endWaitTimer += Time.deltaTime; // タイマーを更新

		// タイマーが既定時間をすぎた時
		if (endWaitTimer > endWaitTime)
		{
			// タイトルへ戻るボタンを表示
			if (uiManager != null)
				uiManager.ShowTitleButtonWithFade();    // タイトルへ戻るボタンをフェードインで表示

			Debug.Log("<color=#c0ffff>タイトルへ戻るボタンを表示</color>");

			// 処理を二度と通らないようにフラグを折る
			endFlag = false;
		}
	}
}
