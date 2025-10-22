using UnityEngine;

public class Character_Status : MonoBehaviour
{
	/******ステータス変数*************/
	[Header("基本ステータス")]
	[SerializeField] protected int MaxHP = 100;                         // キャラクター最大HP
	[SerializeField] protected int MaxReason = 100;                     // キャラクター最大HP
	[SerializeField] protected int ResonPoint = 100;                    // 理性ゲージ
	[SerializeField] protected int decrease_in_reason_time = 1;         // 理性ゲージ減少ダメージ
	[SerializeField] protected int Attackpower = 10;                    // キャラクター攻撃力
	[SerializeField] protected int Defensepower = 20;                   // キャラクター防御力
	[SerializeField] protected float MoveSpeed = 5.0f;                  // キャラクター移動速度

	private float timer = 0f;

	public int CurrentHP { get; protected set; }    // キャラクター現在HP(外部読み取り可、内部変更可)
	public int CurrentReason { get; protected set; }    // キャラクター現在理性HP(外部読み取り可、内部変更可)

	/**********状態*******************/
	enum State
	{
		IDLE,       // 待機状態
		MOVE,       // 移動状態
		ATTAKING,   // 攻撃状態
		DEAD        // 死亡状態
	}

	/**********モード*******************/
	enum Mode
	{
		ANIMAL,         // エニモー
		SPSIAL_ANIMAL   // スペシャルエニモー
	}


	State CharaState; // キャラクター状態変数
	Mode CharaMode;   // キャラクターモード変数

	//初期化
	private void Start()
	{
		CharaState = State.IDLE;    // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;    // 初期モードをエニモーに設定
		CurrentHP = MaxHP;          // 現在HPに最大HPを代入
		CurrentReason = MaxReason;  // 現在理性ポイントに最大理性ポイントを代入
		GetResonPoint();            // 理性ゲージ取得
		GetAttackPower();           // 攻撃力取得
		GetDefensePower();          // 防御力取得
		GetMoveSpeed();             // 移動速度取得
	}

	//更新
	void Update()
	{
		GetCurrentHP();
		GetResonPoint();

		if (Input.GetKeyDown(KeyCode.P))
		{
			TakeDamage(40);
		}

		if (Input.GetKeyDown(KeyCode.O))
		{
			switch (CharaMode)
			{
				case Mode.ANIMAL:
					CharaMode = Mode.SPSIAL_ANIMAL;
					Debug.Log("モードがスペシャルエニモーに変化した。");
					break;

				case Mode.SPSIAL_ANIMAL:
					CharaMode = Mode.ANIMAL;
					Debug.Log("モードがエニモーに変化した。");
					break;
			}
		}


		switch (CharaMode)
		{
			case Mode.ANIMAL:
				// エニモーモードの処理
				break;
			// スペシャルエニモーモードの理性ゲージ減少処理関数呼び出し
			case Mode.SPSIAL_ANIMAL:

				timer += Time.deltaTime;
				if (timer >= 1f)
				{
					Mode_SpsialAnimal(decrease_in_reason_time);
				}
				break;
		}
	}

	//死亡処理関数&ダメージ処理関数
	protected virtual void TakeDamage(int damage)
	{
		// モードごとのダメージ処理分岐
		if (CharaMode == Mode.ANIMAL)
		{
			Mode_Animal(damage);  // エニモーモードのダメージ処理関数呼び出し
		}
		else if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			// ダメージ計算（防御力を考慮）
			int actualDamage = Mathf.Max(damage - Defensepower, 0);
			CurrentReason -= actualDamage; // 理性ゲージ減少処理
		}

		// HPが0以下になった場合の処理
		if (CurrentHP <= 0)
		{
			CurrentHP = 0;
			Die();  // 死亡処理関数呼び出し
		}
	}

	//現在HP取得関数
	private int GetCurrentHP()
	{
		return CurrentHP;
	}

	//理性ゲージ取得関数
	private int GetResonPoint()
	{
		return ResonPoint;
	}

	//攻撃力取得関数
	private int GetAttackPower()
	{
		return Attackpower;
	}

	//防御力取得関数
	private int GetDefensePower()
	{
		return Defensepower;
	}

	//移動速度取得関数
	private float GetMoveSpeed()
	{
		return MoveSpeed;
	}


	//死亡処理関数
	protected virtual void Die()
	{
		CharaState = State.DEAD; // 状態を死亡状態に変更
		Debug.Log($"{gameObject.name} は死亡した。");

		gameObject.SetActive(false); // キャラクターオブジェクトを非アクティブ化(仮)
	}

	protected virtual void Mode_Animal(int damage)
	{
		// エニモーモードの受けたダメージ処理

		// ダメージ計算（防御力を考慮）
		int actualDamage = Mathf.Max(damage - Defensepower, 0);
		Debug.Log($"{gameObject.name} は" + actualDamage + "のダメージを受けた。");
		CurrentHP -= actualDamage;

		Debug.Log("現在のHP:" + CurrentHP);
	}

	protected virtual void Mode_SpsialAnimal(int damage)
	{
		int num = 0;

		if (CurrentReason >= 0)
		{
			num = decrease_in_reason_time / 10;	 // 理性ゲージ減少量計算
			CurrentReason -= num;                // 理性ゲージ減少処理
			Debug.Log("現在の理性ポイント:" + CurrentReason);
		}
		else
		{
			CurrentReason = 0;
		}
	}
}
