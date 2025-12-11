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

	[Header("通常時に時間経過によって理性ゲージ回復する量の設定")]
	[SerializeField] protected int Heal_in_reason_point = 1;             // 理性ゲージ回復量

	[Header("キャラクターごとの固有スキル設定一覧")]
	[Header("毎時体力回復能力(ダチョウ)")]
	[SerializeField] protected int Heal_in_hp_point = 1;				 // 体力回復量(ダチョウ固有)

	[Header("理性解放状態ステータス")]
	[SerializeField] protected int ReasonHP = 200;                      // キャラクター理性解放時最大HP
	[SerializeField] protected int ReasonAttackPower = 50;              // キャラクター理性解放時攻撃力
	[SerializeField] protected int ReasonDefensePower = 60;             // キャラクター理性解放時攻撃力
	[SerializeField] protected float ReasonMoveSpeed = 1.0f;            // キャラクター移動速度

	private Slider hp_gauge;			   //HPゲージUIスライダー参照用変数
	private Slider reason_gauge;		   //HPゲージUIスライダー参照用変数
	private Animator animator;			   //アニメーター参照用変数

	private float timer = 0f;			   //タイマー系の変数

	public GameObject hp_object;
	public GameObject reason_object;

	public int CurrentHP { get; protected set; }    // キャラクター現在HP(外部読み取り可、内部変更可)
	public int CurrentReason { get; protected set; }    // キャラクター現在理性HP(外部読み取り可、内部変更可)

    InputPlayer input;

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

	/**********キャラクタータイプ*******************/
	public enum CharacterType
	{
		NONE,           // 無し
		LION,           // ライオン
		OSTRICH,        // ダチョウ
		RHINOCELOS,     // サイ
		RATEL,          // ラーテル
	}

	State CharaState;			// キャラクター状態変数
	Mode CharaMode;				// キャラクターモード変数
	CharacterType CharaAnim;    // キャラクタータイプ変数

	//初期化
	private void Start()
	{
		CharaState = State.IDLE;        // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;        // 初期モードをエニモーに設定
		CharaAnim = CharacterType.NONE;  // 初期キャラクタータイプを一旦無しに設定
		CurrentHP = MaxHP;              // 現在HPに最大HPを代入
		CurrentReason = MaxReason;      // 現在理性ポイントに最大理性ポイントを代入
		GetResonPoint();                // 理性ゲージ取得
		GetAttackPower();               // 攻撃力取得
		GetDefensePower();              // 防御力取得
		GetMoveSpeed();                 // 移動速度取得

		animator = GetComponent<Animator>();

		input = GetComponent<InputPlayer>();
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

		//一旦固有スキル関数をUpdate内で呼び出し
		UniqueSkill();

		switch (CharaMode)
		{
			case Mode.ANIMAL:
				//もし死亡状態でなければ理性ゲージ回復処理を行う
				if (CharaState != State.DEAD)
				{
					// エニモーモードの処理
					timer += Time.deltaTime;
					if (timer >= 1f)
					{
						ReasonHeal();
						timer = 0f;
					}
				}
				break;

			// スペシャルエニモーモードの理性ゲージ減少処理関数呼び出し
			case Mode.SPSIAL_ANIMAL:

				timer += Time.deltaTime;
				if (timer >= 1f)
				{
					ReasonDecrease();
					timer = 0f;
				}
				break;
		}
		CheckAnimatorStateTag();
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
			//ReasonHeal();  // エニモーモードのダメージ処理関数呼び出し
			CurrentHP -= damage; // HP減少処理
		}
		else if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			// ダメージ計算（防御力を考慮）
			int actualDamage = Mathf.Max(damage - DefensePower, 0);
			CurrentReason -= actualDamage; // 理性ゲージ減少処理
		}

		// 死亡判定
		if (CurrentHP <= 0 || CurrentReason <= 0)
		{
			CurrentHP = 0;
			CurrentReason = 0;
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
		//もし理性解放中に死亡したなら現在HPを0に設定する
		if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			CurrentHP = 0;  // 現在HPを0に設定
		}

		hp_gauge.value = 0;
		reason_gauge.value = 0;
		Debug.Log("キャラクターが死亡しました。");

		CharaState = State.DEAD; // 状態を死亡状態に変更
	}

	//エニモー状態時、理性ゲージを回復
	protected virtual void ReasonHeal()
	{
		int heal_num = 0;

		if (MaxReason != CurrentReason)
		{
			heal_num = MaxReason;
			CurrentReason += Heal_in_reason_point;
			Debug.Log("現在の理性ポイント:" + CurrentReason);
		}
	}

	//スペシャルエニモーモード理性ゲージ減少処理関数
	protected virtual void ReasonDecrease()
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
			animator.SetBool("Reason_Dead", true);
			Die();
		}
	}



	// Animatorの現在のステートのTagをチェックし、
	// 死亡状態であればオブジェクトを非アクティブ化する関数
	private void CheckAnimatorStateTag()
	{
		if (animator == null) return;

		// 現在のAnimatorStateInfoを取得 (通常はベースレイヤー: 0)
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		// キャラクターが死亡状態の場合のみTagをチェック
		if (CharaState == State.DEAD)
		{
			// アニメーションステートのTagが "Dead" であるかをチェック
			if (stateInfo.IsTag("Dead"))
			{
				// 既に死亡ログが出ていなければログを出し、ゲームオブジェクトを非アクティブ化
				if (gameObject.activeSelf) // 処理が複数回実行されるのを防ぐため
				{
					Debug.Log($"{gameObject.name} はアニメーション" +
					$"Tag 'Dead' に到達したため、オブジェクトを非アクティブ化します。");
					input.SetDeath();
				}
			}
		}
	}

	//常時呼び出し固有スキル関数
	protected virtual void UniqueSkill()
	{
		CharaAnim = CharacterType.OSTRICH;//お試しでダチョウに設定
		
		// キャラクター固有のスキル処理をここに実装
		switch (CharaAnim)
		{
			//ライオンを選択した場合固有スキル発動
			case CharacterType.LION:
				UniqueSkill_Lion();
				break;
			//ダチョウを選択した場合固有スキル発動
			case CharacterType.OSTRICH:
				UniqueSkill_Ostrich();
				break;
		}
	}

	//ライオンの固有スキル処理関数
	void UniqueSkill_Lion()
	{
	}

	//ダチョウの固有スキル処理関数
	void UniqueSkill_Ostrich()
	{
		//0でないなら体力回復処理
		if (CurrentHP != 0)
		{
			/*ダチョウの固有スキルは体力を
			時間経過によって回復する*/
			int ostrich_heal = Heal_in_hp_point;

			timer += Time.deltaTime;

			if (timer >= 1f)
			{
				// HP回復処理
				if (MaxHP != CurrentHP)
				{
					CurrentHP += ostrich_heal;
				}
				timer = 0f;
			}
		}
		else
		{
			//0なら死亡処理関数呼び出し
			Die();
		}
	}
}
