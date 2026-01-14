using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerCountMultiHandler : MonoBehaviour
{
	public Button[] countButtons;			// 1P～4Pボタンを順番に
	public GameDataManager dataManager;     //参加人数を管理するスクリプトの参照
	private int currentIndex = 0;           // 現在選択中のボタンインデックス


	//初期化
	void Start()
	{
		// 最初は1Pボタンを選択状態にする
		if (countButtons.Length > 0) countButtons[0].Select();
	}

	void Update()
	{
		// 全ての接続済みコントローラーをチェック
		for (int i = 0; i < Gamepad.all.Count; i++)
		{
			//参加してるコントローラーを取得(複数人対応)
			var pad = Gamepad.all[i];

			// コントローラーが無効なら以下の処理をスキップ
			if (pad == null) continue;

			// --- 誰かが「右」を押した ---
			if (pad.leftStick.right.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame)
			{
				MoveSelection(1);
				break; // 1つの入力で1回動けばいいのでループを抜ける
			}
			// --- 誰かが「左」を押した ---
			if (pad.leftStick.left.wasPressedThisFrame || pad.dpad.left.wasPressedThisFrame)
			{
				MoveSelection(-1);
				break;
			}
			// --- 誰かが「決定(A)」を押した ---
			if (pad.buttonSouth.wasPressedThisFrame)
			{
				if (countButtons[currentIndex].interactable)
				{
					// GameDataManagerの関数でシーン遷移
					dataManager.SelectPlayerCount(currentIndex + 1);
				}
				break;
			}
		}

		// --- リアルタイム制限：接続数より多いボタンは選べなくする ---
		int connectedCount = Gamepad.all.Count;

		//参加してないPlayerの数は選べないようにする
		for (int j = 0; j < countButtons.Length; j++)
		{
			// 接続数以下なら選択可能、超えてたら選択不可にする
			countButtons[j].interactable = (j + 1 <= connectedCount);
		}
	}

	// 選択中のボタンを移動する関数
	void MoveSelection(int dir)
	{
		currentIndex = (currentIndex + dir + countButtons.Length) % countButtons.Length;
		countButtons[currentIndex].Select();
	}
}