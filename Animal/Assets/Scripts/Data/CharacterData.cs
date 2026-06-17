using UnityEngine;

/*あくまでもここはデータのみの情報だけなので
 * MonoBehaviourというオブジェクトに張り付けるだけのものを削除*/

// 動物の情報
[System.Serializable]
public struct AnimalParam
{
	/*動物の基本パラメータ*/
	public int maxHP;
	public int maxReason;
	public int attackPower;
	public int defensePower;
	public float moveSpeed;

	/*理性解放状態時のパラメータ*/
	public float reasonAtkMult;
	public float reasonDefMult;
	public float reasonSpdMult;
}

//動物ごとのパラメーター&理性解放の倍率
public static class CharacterData
{
	/*共通の定数*/
	public const float DEFAULT_MULT = 1.3f;			 //理性解放時の基本的な上昇倍率
	public const float REASON_DECREASE_RATE = 0.02f; // 最大理性ゲージから2%分で減少
	public const float REASON_HEAL_RATE = 0.01f;     // 最大理性ゲージから1%分で回復

	//ライオンのパラメータ
	public static readonly AnimalParam Lion = new AnimalParam
	{
		//通常
		maxHP = 450,
		maxReason = 100,
		attackPower = 40,
		defensePower = 15,
		moveSpeed = 5.5f,

		//理性解放時の倍率
		reasonAtkMult = 1.6f,	//攻撃特化
		reasonDefMult = DEFAULT_MULT,
		reasonSpdMult = DEFAULT_MULT,
	};

	//ダチョウのパラメータ
	public static readonly AnimalParam Ostrich = new AnimalParam
	{
		//通常
		maxHP = 350,
		maxReason = 120,
		attackPower = 20,
		defensePower = 13,
		moveSpeed = 7.5f,//7.5f

		//理性解放時の倍率
		reasonAtkMult = DEFAULT_MULT,
		reasonDefMult = DEFAULT_MULT,
		reasonSpdMult = 1.5f,   //スピード特化
	};

	//サイのパラメータ
	public static readonly AnimalParam Rhinocelos = new AnimalParam
	{
		//通常
		maxHP = 400,
		maxReason = 150,
		attackPower = 35,
		defensePower = 30,
		moveSpeed = 4.5f,

		//理性解放時の倍率
		reasonAtkMult = DEFAULT_MULT,
		reasonDefMult = 1.5f,   //防御特化
		reasonSpdMult = DEFAULT_MULT,
	};

	//ラーテルのパラメータ
	public static readonly AnimalParam Ratel = new AnimalParam
	{
		//通常
		maxHP = 400,
		maxReason = 100,
		attackPower = 25,
		defensePower = 20,
		moveSpeed = 5.0f,

		//理性解放時の倍率(ラーテルは上昇は均等)
		reasonAtkMult = DEFAULT_MULT,
		reasonDefMult = DEFAULT_MULT,
		reasonSpdMult = DEFAULT_MULT,
	};
}

