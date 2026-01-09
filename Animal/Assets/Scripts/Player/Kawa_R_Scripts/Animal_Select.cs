using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Animal_Select : MonoBehaviour
{
    public static Character_Status.CharacterType[] playerChoices =
    new Character_Status.CharacterType[5];

    [Header("このボタンは何人用の説明用？1～4")]
    public int playerID;

    [Header("このボタンで選ばれる動物")]
    public Character_Status.CharacterType animalType;

	void Update()
	{
		// もしこのボタンが現在「選択（フォーカス）」されているなら
		if (EventSystem.current.currentSelectedGameObject == this.gameObject)
		{
			// 1Pの決定キー（例：Z）が押されたら
			if (Input.GetKeyDown(KeyCode.Z)) { SetChoice(1); }

			// 2Pの決定キー（例：Enter）が押されたら
			if (Input.GetKeyDown(KeyCode.Return)) { SetChoice(2); }

			// 3P, 4Pも同様にキーを割り当て可能
		}
	}


	// 実際に配列へ保存する処理
	void SetChoice(int playerId)
	{
		playerChoices[playerId] = animalType;
		Debug.Log($"{playerId}P が {animalType} を予約しました！");
	}


	// ボタンが押された時に呼ばれる関数（引数にプレイヤー番号を入れる）
	public void SelectCharacter(int playerID)
	{
		if (playerID < 1 || playerID > 4) return;

		// 指定されたプレイヤーの枠に、このボタンの動物を上書き
		playerChoices[playerID] = animalType;

		Debug.Log($"プレイヤー{playerID} が {animalType} を選択中（変更可能）");
	}
}
