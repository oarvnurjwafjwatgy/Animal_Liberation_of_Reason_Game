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

	//「一部共通の定数」
	public const float TIMER_RESET = 0f;	//基本全てで使用するタイマー初期値
	public const int INITIAL_VALUE = 0;		//初期値
	
}

//通常攻撃の攻撃間隔
public struct NormalAttack_CoolTime
{
	public const float LION_ATTACK_CT = 2.0f;
	public const float OSTRICH_ATTACK_CT = 0.3f;
	public const float RHINOCELOS_ATTACK_CT = 1.5f;
	public const float RATEL_ATTACK_CT = 1.0f;
}


/****各動物のスキルパラメーター******/

//ライオンのスキル&特性の定数
public struct LionSkillParam
{
	/****時間のパラメーター***********/
	//スキルの定数
	public const float LION_SKILL_DURATION = 5.0f;  // 咆哮バフの持続時間
	public const float LION_CT = 15.0f;             // スキルのCT(咆哮)※爆発力が高いので長め
	
	// 特性用の定数
	public const float UI_VISIBLE_DURATION = 3.0f;  // 被弾後、UIを何秒間表示させるか
	public const float LION_BURST_DURATION = 8f;    // バースト持続時間


	/******倍率のパラメーター********/
	//スキル
	public const float LION_RESET_VALUE = 1.0f;     // 初期値:変化前に戻す値(ex.攻撃後に値を元に戻す
	public const float SKILL_UP_VALUE = 1.3f;       // 通常時にスキルでバフ強化する時に上がる倍率
	public const float REASON_SKILL_UP_VALUE = 1.7f;// 理性開放時にバフ強化する時に上がる倍率

	//特性
	public const int BURST_THRESHOLD = 80;                 // 発動しきい値
	public const float BURST_BASE_SPEED_BOOST = 1.25f;     // バースト時の基本速度上昇率
	public const float BURST_MAX_EXTRA_SPEED = 0.15f;      // 蓄積ダメージによる追加速度の上限値
	public const float BURST_ATK_BOOST = 1.15f;            // バースト時の攻撃上昇率
	public const float DAMAGE_TO_SPEED_SCALE = 150f;       // ダメージを速度倍率に変換する際の割る数

}

//ダチョウのスキル
public struct OstrichSkillParam
{
	//時間のパラメーター
	public const float OSTRICH_CT = 6.0f;           // スキルCT(他動物との差別点はあまりないが回転率たかめ)
	public const float RESET_DELAY = 0.2f;			//攻撃値を含めた値をリセットするのにかかる時間

	//倍率のパラメーター
	public const float OSTRICH_RESET_VALUE = 1.0f;   // 初期値:変化前に戻す値(ex.攻撃後に値を元に戻す
	public const float SKILL_UP_VALUE = 1.9f;        // 通常時:スキルで攻撃する時に通常攻撃に上乗せさせる値
	public const float REASON_SKILL_UP_VALUE = 2.5f; // 理性解放:スキルで攻撃強化する時に通常攻撃に上乗せさせる値

	//割合パラメーター
	public const int HEAL_HP_RATE = 1;               //体力回復の割合量
}

//サイのスキル
public struct RhinocelosSkillParam
{
	//時間のパラメーター
	public const float REASON_DECREASEINTERVAL = 1.0f; //何秒に1回理性を削るか(1秒)
	public const int REASON_DECREASEAMOUNT = 5;        // 1回あたりに減らす理性の量（5）

	//倍率のパラメーター
	public const float RHINOCELOS_RESET_VALUE = 1.0f;   // 初期値:変化前に戻す値(ex.攻撃後に値を元に戻す
	public const float RHINOCELOS_SPPEED_UP = 1.8f;     // スキル使用時:1.8倍速度が倍率かかる
	public const float SKILL_UP_VALUE = 1.4f;           // 通常時:スキルで攻撃する時に通常攻撃に上乗せさせる値
	public const float REASON_SKILL_UP_VALUE = 1.6f;    // 理性解放:スキルで攻撃強化する時に通常攻撃に上乗せさせる値
}

//ラーテルのスキル
public struct HoneyBadgerSkillParam
{
	//時間のパラメーター
	public const float HONEYBADGER_CT = 12.0f;    // ラーテルのスキルCT
	public const float RESET_DELAY = 2.0f;        // 攻撃値を含めた値をリセットするのにかかる時間
	public const float RATEL_MAX_HIDE_TIME = 10f; // ラーテルのスキルで潜っている最長時間

	//倍率のパラメーター
	public const float HONEYBADGER_RESET_VALUE = 1.0f;   // 初期値:変化前に戻す値(ex.攻撃後に値を元に戻す
	public const float SKILL_UP_VALUE = 1.7f;			 // 通常時:スキルで攻撃する時に通常攻撃に上乗せさせる値
	public const float REASON_SKILL_UP_VALUE = 2.0f;	 // 理性解放:スキルで攻撃強化する時に通常攻撃に上乗せさせる値

	//割合パラメーター
	public const float TRAIT_PERCENTAGE_RECOVERY = 0.25f;	//特性発動後の回復量
}


//動物ごとのパラメーター&理性解放の倍率
public static class CharacterData
{
	/*共通の定数*/
	public const float DEFAULT_MULT = 1.3f;				//理性解放時の基本的な上昇倍率
	public const float INITIAL_MAGNIFICATION = 1.0f;	//全倍率の初期値(ex.AtkMult→リセット
	public const float REASON_DECREASE_RATE = 0.02f;	// 最大理性ゲージから2%分で減少
	public const float REASON_HEAL_RATE = 0.01f;		// 最大理性ゲージから1%分で回復
	public const float OFF_SITE_RAITO = 0.05f;          // 場外に出たときの最大HP割合ダメージ量
	public const float ATTACK_OFFSET = 1.0f;            // 攻撃判定を出す位置（自分の中心からどれくらい前か）

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
		moveSpeed = 7.5f,

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

/**********キャラクタータイプ*******************/
public enum CharacterType
{
	NONE,           // 無し
	LION,           // ライオン
	OSTRICH,        // ダチョウ
	RHINOCELOS,     // サイ
	RATEL,          // ラーテル
}

/**********モード*******************/
public enum ChangeMode { ANIMAL, SPSIAL_ANIMAL }   // 通常&理性解放

// バフ・デバフ管理用の列挙型と変数
public enum BuffType { SpeedBuff, SpeedDebuff, AttackBuff, AttackDebuff, RhinoDash }


/**********状態*******************/
public struct AnimalState
{
	//共通状態
	public enum State
	{
		IDLE,       // 待機状態
		MOVE,       // 移動状態
		ATTAKING,   // 攻撃状態
		DEAD        // 死亡状態
	}

	//ラーテルのスキル状態
	public enum RatelSkillState
	{
		Idle = 0,
		Hide = 1,
		Attack = 2
	}
}

