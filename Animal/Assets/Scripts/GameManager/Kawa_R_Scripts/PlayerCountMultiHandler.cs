using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerCountMultiHandler : MonoBehaviour
{
	public Button[] countButtons;            // 1P～4Pボタンを順番に
	public GameDataManager dataManager;      // 参加人数を管理するスクリプトの参照

	// 枠（カーソル）のRectTransform
	[SerializeField] private RectTransform cursorRect;
	// 枠のサイズを調整するオフセット
	[SerializeField] private Vector2 cursorPadding = new Vector2(20, 20);

	// ★カーソル音の重複防止用
	private GameObject lastSelected;

	void Start()
	{
		// 1Pは時間的に実装が難しいため、最初から選べないようにする
		if (countButtons.Length > 0)
		{
			countButtons[0].gameObject.SetActive(false);
			countButtons[0].interactable = false;
		}

		// ★1Pが非表示になったことで壊れた「上下左右の繋がり」をプログラムで繋ぎ直す
		FixButtonNavigation();

		// 最初は2Pボタンを選択状態にする
		if (countButtons.Length > 1)
		{
			EventSystem.current.SetSelectedGameObject(countButtons[1].gameObject);
			lastSelected = countButtons[1].gameObject; // 初期選択を保存
		}

		// ボタンにイベント登録
		// 1Pは実装が難しいため、最初から選べないようにするので、ループは2Pから
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
				// 決定音（0番）を鳴らす
				if (AudioManager.Instance != null)
				{
					AudioManager.Instance.PlaySEByIndex(0);
				}
			});
		}
	}

	// ★ボタンの移動経路を再定義する関数
	void FixButtonNavigation()
	{
		if (countButtons.Length < 4) return;

		// countButtons[1]=2P, [2]=3P, [3]=4P と想定

		// --- 2Pボタンの設定 ---
		Navigation nav2 = countButtons[1].navigation;
		nav2.mode = Navigation.Mode.Explicit; // 自動判定をオフにして手動指定
		nav2.selectOnDown = countButtons[3];  // 下に行ったら4Pへ
		nav2.selectOnLeft = countButtons[2];  // 左に行ったら3Pへ
		countButtons[1].navigation = nav2;

		// --- 3Pボタンの設定 ---
		Navigation nav3 = countButtons[2].navigation;
		nav3.mode = Navigation.Mode.Explicit;
		nav3.selectOnUp = countButtons[1];    // 上に行ったら2Pへ（1Pがいないため）
		nav3.selectOnRight = countButtons[3]; // 右に行ったら4Pへ
		nav3.selectOnDown = countButtons[1];  // 下に行っても行き止まりにならないよう2Pへ
		countButtons[2].navigation = nav3;

		// --- 4Pボタンの設定 ---
		Navigation nav4 = countButtons[3].navigation;
		nav4.mode = Navigation.Mode.Explicit;
		nav4.selectOnUp = countButtons[1];    // 上に行ったら2Pへ
		nav4.selectOnLeft = countButtons[2];  // 左に行ったら3Pへ
		countButtons[3].navigation = nav4;
	}

	void Update()
	{
		// 接続人数に応じてボタンの有効/無効を更新
		UpdateInteractable();

		//カーソル（枠）を選択中のボタンに追従させる処理
		UpdateCursorPosition();
	}

	// カーソル（枠）を選択中のボタンに追従させる処理
	void UpdateCursorPosition()
	{
		GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

		if (currentSelected != null && cursorRect != null)
		{
			// 枠を表示する
			cursorRect.gameObject.SetActive(true);

			// 選択中のボタンのRectTransformを取得
			RectTransform targetRect = currentSelected.GetComponent<RectTransform>();

			if (targetRect != null)
			{
				// 1. 位置を合わせる
				cursorRect.position = targetRect.position;

				// 2. サイズを合わせる（Paddingで少し大きくする）
				cursorRect.sizeDelta = targetRect.sizeDelta + cursorPadding;
			}
		}
		else if (cursorRect != null)
		{
			// 何も選択されていない時は枠を隠す
			cursorRect.gameObject.SetActive(false);
		}
	}


	// 接続されているコントローラー数を取得し、選択可能なボタン数を制限する
	void UpdateInteractable()
	{
		// --- リアルタイム制限：接続数より多いボタンは選べなくする ---
		int connectedCount = Gamepad.all.Count;

		// 参加してないPlayerの数は選べないようにする
		for (int j = 1; j < countButtons.Length; j++)
		{
			// 接続数以下なら選択可能、超えてたら選択不可にする
			countButtons[j].interactable = (j + 1 <= connectedCount);
		}

		GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

		// ★カーソル移動音の処理：選択が変わった瞬間に鳴らす
		if (currentSelected != null && currentSelected != lastSelected)
		{
			// 最初の一回（nullからの変化）以外で鳴らす
			if (lastSelected != null && AudioManager.Instance != null)
			{
				// 移動音（1番）を鳴らす
				AudioManager.Instance.PlaySEByIndex(1);
			}
			lastSelected = currentSelected;
		}

		// ボタンが選択されているか確認し、選択されている場合はそのボタンが有効かどうかを確認する
		if (currentSelected != null)
		{
			Button btn = currentSelected.GetComponent<Button>();

			if (btn != null && !btn.interactable)
			{
				// 無効なボタン（接続されてない人数）なら2Pボタンへ戻す
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