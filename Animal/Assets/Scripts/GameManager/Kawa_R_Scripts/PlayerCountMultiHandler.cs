using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerCountMultiHandler : MonoBehaviour
{
    public Button[] countButtons;			// 1P～4Pボタンを順番に
    public GameDataManager dataManager;     //参加人数を管理するスクリプトの参照


    GameObject SoundManagerObj;
    SoundManager soundmanager;

    void Start()
    {
        SoundManagerObj = GameObject.Find("SoundManager");
        soundmanager = SoundManagerObj.GetComponent<SoundManager>();

		//1Pは時間的に実装が難しいため、最初から選べないようにする
		if (countButtons.Length > 0)
        {
            countButtons[0].gameObject.SetActive(false);
            countButtons[0].interactable = false;
        }

        //最初は2pボタンを選択状態にする
        if (countButtons.Length > 1)
        {
            EventSystem.current.SetSelectedGameObject(countButtons[1].gameObject);
        }


		// ボタンにイベント登録
		//1Pは実装が難しいため、最初から選べないようにするので、ループは2Pから
		for (int i = 1; i < countButtons.Length; i++)
        {
            // onClickは引数なし関数しか受け取れないがindexを渡したいため、
            // 引数なしのラムダ式を呼び、その中で引数付き関数を呼ぶ

            // ループ変数はそのまま使うとバグるのでローカルコピーを作る
            // (ラムダ式は、その場の値を保存するのではなく「変数そのもの」を参照)
            int index = i;
            countButtons[i].onClick.AddListener(() =>
            {
                OnButtonClicked(index);
                soundmanager.PlaySE(0);
            });
        }
    }

    void Update()
    {
        // 接続人数に応じてボタンの有効/無効を更新
        UpdateInteractable();
    }

    // 接続されているコントローラー数を取得し、選択可能なボタン数を制限する
    void UpdateInteractable()
    {
        // --- リアルタイム制限：接続数より多いボタンは選べなくする ---
        int connectedCount = Gamepad.all.Count;

        //参加してないPlayerの数は選べないようにする
        for (int j = 1; j < countButtons.Length; j++)
        {
            // 接続数以下なら選択可能、超えてたら選択不可にする
            countButtons[j].interactable = (j + 1 <= connectedCount);
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        //ボタンが選択されているか確認し、選択されている場合はそのボタンが有効かどうかを確認する
        if (currentSelected != null)
        {
            Button btn = currentSelected.GetComponent<Button>();

            if (btn != null&&!btn.interactable) 
            { 
            EventSystem.current.SetSelectedGameObject(countButtons[1].gameObject);
			}
        }
    }

    // ボタンが押された時に呼ばれる
    void OnButtonClicked(int index)
    {
        // 押されたボタンが有効か確認
        if (countButtons[index].interactable)
        {
            dataManager.SelectPlayerCount(index + 1);
        }
    }
}   