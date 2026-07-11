using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
	// 静的変数：何人でプレイするかを保持する（初期値は2人）
	public static int TotalRoomSize = 2;

	// ボタンから呼ばれる関数
	public void SelectPlayerCount(int count)
	{
		TotalRoomSize = count;// 押されたボタンの数（2〜4）を総枠数として保存
		Debug.Log($"<color=orange>参加人数を {count}名に設定しました。</color>");
		SetCpuAnimal();// CPUの動物を設定する
		SceneManager.LoadScene("SelectScene");// キャラクター選択シーンへ遷移
	}

	private void SetCpuAnimal()
	{
		int animalTypeCount = System.Enum.GetValues(typeof(CharacterType)).Length - 1;// NONEを除く
		int humanCount = Mathf.Max(1, Gamepad.all.Count);//現在接続されているコントローラーの数を取得（最低1人は人間）

		// 1P（自分）は常にプレイヤー確定なのでここで明示的にセット
		if (select_saver.Instance != null && select_saver.Instance.SlotTypes.Length > 0)
			select_saver.Instance.SlotTypes[0] = SlotType.PLAYER;

		for (int i = 2; i <= TotalRoomSize; i++)
		{
			// 配列のインデックス用に 1 つ引く (2Pならインデックス1)
			int saveIndex = i - 1;

			// 安全チェック：セーブデータの配列サイズを超えないようにする
			if (select_saver.Instance == null || saveIndex >= select_saver.Instance.SlotTypes.Length) continue;

			if (i <= humanCount)
			{
				select_saver.Instance.SlotTypes[i] = SlotType.PLAYER;
				Animal_Select.playerChoices[i] = CharacterType.NONE;// セレクト画面で自分で選ぶ
			}
			else
			{
				select_saver.Instance.SlotTypes[i] = SlotType.CPU;
				// 1 〜 動物の種類の数 の間でランダムな数字を決める（例: 1=LION, 2=OSTRICH...）
				int randomAnimalIndex = UnityEngine.Random.Range(1, animalTypeCount + 1);
				Animal_Select.playerChoices[i] = (CharacterType)randomAnimalIndex;// CPUの動物をランダムに設定
				Debug.Log($"{i}P(CPU)の動物をランダム設定: {Animal_Select.playerChoices[i]}");
			}
		}
	}
}