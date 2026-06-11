using UnityEngine;

/*あくまでもここはデータのみの情報だけなので
 * MonoBehaviourというオブジェクトに張り付けるだけのものを削除*/

// 動物の情報
[System.Serializable]
public struct AnimalParam
{
	public int maxHP;
	public int maxReason;
	public int attackPower;
	public int defensePower;
	public float moveSpeed;
}

//動物ごとのパラメーター
public static class CharacterData
{
	//ライオンのパラメータ
	public static readonly AnimalParam Lion = new AnimalParam
	{
		maxHP = 450,
		maxReason = 100,
		attackPower = 40,
		defensePower = 15,
		moveSpeed = 5.5f
	};

	//ダチョウのパラメータ
	public static readonly AnimalParam Ostrich = new AnimalParam
	{
		maxHP = 350,
		maxReason = 120,
		attackPower = 20,
		defensePower = 13,
		moveSpeed = 7.5f
	};

	//サイのパラメータ
	public static readonly AnimalParam Rhinocelos = new AnimalParam
	{
		maxHP = 400,
		maxReason = 150,
		attackPower = 35,
		defensePower = 30,
		moveSpeed = 4.5f
	};

	//ラーテルのパラメータ
	public static readonly AnimalParam Ratel = new AnimalParam
	{
		maxHP = 400,
		maxReason = 100,
		attackPower = 25,
		defensePower = 20,
		moveSpeed = 5.0f
	};
}

