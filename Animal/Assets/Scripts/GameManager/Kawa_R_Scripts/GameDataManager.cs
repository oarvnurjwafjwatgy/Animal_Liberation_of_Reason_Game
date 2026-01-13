using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
	// 静的変数：シーンを跨いで人数を保持する（デフォルトは2人）
	public static int SelectedPlayerCount = 2;

	// ボタンから呼ばれる関数
	public void SelectPlayerCount(int count)
	{
		// 人数を保存
		SelectedPlayerCount = count;
		Debug.Log($"<color=orange>参加人数を {count}名に設定しました。</color>");

		// キャラクター選択シーンへ遷移（シーン名は自分のプロジェクトに合わせてね）
		SceneManager.LoadScene("SelectScene");
	}
}