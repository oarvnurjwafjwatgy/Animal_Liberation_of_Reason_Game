using UnityEngine;
using UnityEngine.UI;

public class Character_Status : MonoBehaviour
{
	/******ステータス変数*************/
	[Header("基本ステータス")]
	[SerializeField] protected int MaxHP = 400;                         // キャラクター最大HP
	[SerializeField] protected int MaxReason = 100;                     // キャラクター理性最大HP
	[SerializeField] protected int ReasonPoint = 100;                   // 理性ゲージ
	[SerializeField] protected int AttackPower = 10;                    // キャラクター攻撃力
	[SerializeField] protected int DefensePower = 20;                   // キャラクター防御力
	[SerializeField] protected float MoveSpeed = 5.0f;                  // キャラクター移動速度

	[Header("理性ゲージ解放時の減少設定")]
	[SerializeField] protected int Decrease_in_reason_time = 1;         // 理性ゲージ減少ダメージ


	[Header("理性解放状態ステータス")]
	[SerializeField] protected int ReasonHP = 200;                      // キャラクター理性解放時最大HP
	[SerializeField] protected int ReasonAttackPower = 50;              // キャラクター理性解放時攻撃力
	[SerializeField] protected int ReasonDefensePower = 60;             // キャラクター理性解放時攻撃力
	[SerializeField] protected float ReasonMoveSpeed = 1.0f;            // キャラクター移動速度

	private Slider hp_gauge;			   //HPゲージUIスライダー参照用変数
	private Slider reason_gauge;        //HPゲージUIスライダー参照用変数
	private Animator animator;		   //アニメーター参照用変数

	private float timer = 0f;       //理性ゲージ減少用タイマー

	public GameObject hp_object;
	public GameObject reason_object;

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
	public enum Mode
	{
		ANIMAL,         // エニモー
		SPSIAL_ANIMAL   // スペシャルエニモー
	}


	State CharaState; // キャラクター状態変数
	Mode CharaMode;   // キャラクターモード変数

	//初期化
	private void Start()
	{
		//HPゲージのオブジェクトを探して自動的に取得させる。
		//hp_object = GameObject.Find("HP_ber");

		//理性ゲージのオブジェクトを探して自動的に取得させる。
		//reason_object = GameObject.Find("Reason_ber");

		CharaState = State.IDLE;    // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;    // 初期モードをエニモーに設定
		CurrentHP = MaxHP;          // 現在HPに最大HPを代入
		CurrentReason = MaxReason;  // 現在理性ポイントに最大理性ポイントを代入
		GetResonPoint();            // 理性ゲージ取得
		GetAttackPower();           // 攻撃力取得
		GetDefensePower();          // 防御力取得
		GetMoveSpeed();             // 移動速度取得

		animator = GetComponent<Animator>();
	}

	//更新
	void Update()
	{
		GetCurrentHP();
		GetResonPoint();

		// HPゲージの現在値を更新
		if (hp_gauge != null && reason_gauge != null)
		{
			hp_gauge.value = CurrentHP;
			reason_gauge.value = CurrentReason;
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			TakeDamage(40);
		}

		if (Input.GetKeyDown(KeyCode.O))
		{
			GetModeChange();
		}

		if(Input.GetKeyDown(KeyCode.K))
		{
			switch (CharaMode)
			{
				case Mode.ANIMAL:
					
					break;
				case Mode.SPSIAL_ANIMAL:

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
					Mode_SpsialAnimal();
					timer = 0f;
				}
				break;
		}
	}

	public void InitGauges()
	{
		// HPゲージスライダーコンポーネント取得
		if (hp_object != null)
		{
			hp_gauge = hp_object.GetComponent<Slider>();

			// HPゲージのオブジェクトに最大値と現在値を設定
			hp_gauge.maxValue = MaxHP;
			hp_gauge.value = CurrentHP;
		}

		// 理性ゲージスライダーコンポーネント取得
		if (reason_object != null)
		{
			reason_gauge = reason_object.GetComponent<Slider>();

			// 理性ゲージのオブジェクトに最大値と現在値を設定
			reason_gauge.maxValue = MaxReason;
			reason_gauge.value = CurrentReason;
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
			int actualDamage = Mathf.Max(damage - DefensePower, 0);
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
	public int GetCurrentHP()
	{
		return CurrentHP;
	}

	//理性ゲージ取得関数
	public int GetResonPoint()
	{
		return ReasonPoint;
	}

	//攻撃力取得関数
	public int GetAttackPower()
	{
		return AttackPower;
	}

	//防御力取得関数
	public int GetDefensePower()
	{
		return DefensePower;
	}

	//移動速度取得関数
	public float GetMoveSpeed()
	{
		return MoveSpeed;
	}

	public Mode GetMode()
	{
		return CharaMode;
	}

	//モードが切り替え時に呼び出す関数
	public void GetModeChange()
	{
		switch (CharaMode)
		{
			case Mode.ANIMAL:
				CharaMode = Mode.SPSIAL_ANIMAL;
				animator.SetBool("Reason", true);
				Debug.Log("モードがスペシャルエニモーに変化した。");
				break;

			case Mode.SPSIAL_ANIMAL:
				DefaultGetStatus();
				animator.SetBool("Reason", false);
				break;
		}
	}

	//切り替え時Animalステータス取得関数
	public void DefaultGetStatus()
	{
		CharaMode = Mode.ANIMAL;
		Debug.Log("モードがエニモーに変化した。");

	}

	//死亡処理関数
	protected virtual void Die()
	{
		hp_gauge.value = 0;
		CharaState = State.DEAD; // 状態を死亡状態に変更
		Debug.Log($"{gameObject.name} は死亡した。");

		gameObject.SetActive(false); // キャラクターオブジェクトを非アクティブ化(仮)
	}

	protected virtual void Mode_Animal(int damage)
	{
		// エニモーモードの受けたダメージ処理

		// ダメージ計算（防御力を考慮）
		int actualDamage = Mathf.Max(damage - DefensePower, 0);
		Debug.Log($"{gameObject.name} は" + actualDamage + "のダメージを受けた。");
		CurrentHP -= actualDamage;

		Debug.Log("現在のHP:" + CurrentHP);
	}

	//スペシャルエニモーモード理性ゲージ減少処理関数
	protected virtual void Mode_SpsialAnimal()
	{
		int num = 0;

		//もし理性が0より大きいなら理性ゲージを減少させる
		if (CurrentReason > 0)
		{
			num = Decrease_in_reason_time;		 // 理性ゲージ減少量計算
			CurrentReason -= num;                // 理性ゲージ減少処理
			Debug.Log("現在の理性ポイント:" + CurrentReason);
		}
		//もし理性が0以下なら理性ゲージを0にして死亡処理を行う
		else
		{
			CurrentReason = 0;
			Die();
		}
	}
}
