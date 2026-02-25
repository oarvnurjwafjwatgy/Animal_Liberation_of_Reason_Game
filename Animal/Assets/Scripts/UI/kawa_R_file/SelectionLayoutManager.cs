using UnityEngine;

public class SelectionLayoutManager : MonoBehaviour
{
	[Header("人数に応じて表示する背景枠 (1P~4Pの順)")]
	[SerializeField] private GameObject[] selectAreaFrames;

	void Update()
	{
		// 1. GameDataManagerから現在の選択人数を取得
		int requiredPlayers = GameDataManager.SelectedPlayerCount;

		// 2. 枠の表示・非表示を切り替え
		for (int i = 0; i < selectAreaFrames.Length; i++)
		{
			if (selectAreaFrames[i] != null)
			{
				// インデックス(0~3)が参加人数(1~4)より小さければ表示
				selectAreaFrames[i].SetActive(i < requiredPlayers);
			}
		}
	}
}