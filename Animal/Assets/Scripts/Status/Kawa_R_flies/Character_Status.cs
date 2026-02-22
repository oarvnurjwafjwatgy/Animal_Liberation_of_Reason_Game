using UnityEngine;
using UnityEngine.UI;

public class Character_Status : MonoBehaviour
{
	[Header("選択キャラクター")]
	[SerializeField] protected bool SelectLion = false;        // ライオン選択フラグ
	[SerializeField] protected bool SelectOstrich = false;     // ダチョウ選択フラグ
	[SerializeField] protected bool SelectRhinocelos = false;  // サイ選択フラグ
	[SerializeField] protected bool SelectRatel = false;       // ラーテル選択フラグ

	/******ステータス変数*************/
	[Header("基本ステータス")]
	[SerializeField] protected int MaxHP = 400;                // キャラクター最大HP
	[SerializeField] protected int MaxReason = 100;            // キャラクター理性最大HP
	[SerializeField] protected int ReasonPoint = 100;          // 理性ゲージ
	[SerializeField] protected int AttackPower = 10;           // キャラクター攻撃力
	[SerializeField] protected int DefensePower = 20;          // キャラクター防御力
	[SerializeField] protected float MoveSpeed = 5.0f;         // キャラクター移動速度


	// --- キャラクター別ベースステータス定数 ---
	[Header("ライオン ステータス")]
	private const int LION_HP = 450;
	private const int LION_ATK = 30;
	private const int LION_DEF = 15;
	private const float LION_SPD = 6.0f;

	[Header("ダチョウ ステータス")]
	private const int OSTRICH_HP = 350;
	private const int OSTRICH_ATK = 15;
	private const int OSTRICH_DEF = 10;
	private const float OSTRICH_SPD = 8.5f;

	[Header("サイ ステータス")]
	private const int RHINO_HP = 600;
	private const int RHINO_ATK = 18;
	private const int RHINO_DEF = 30;
	private const float RHINO_SPD = 4.0f;

	[Header("ラーテル ステータス")]
	private const int RATEL_HP = 400;
	private const int RATEL_ATK = 20;
	private const int RATEL_DEF = 20;
	private const float RATEL_SPD = 5.0f;

	// --- 理性解放時の倍率定数 ---
	private const float LION_REASON_ATK_MULT = 1.6f;
	private const float RHINO_REASON_DEF_MULT = 1.5f;
	private const float OSTRICH_REASON_SPD_MULT = 1.5f;
	private const float DEFAULT_MULT = 1.3f;				// 基本的な上昇幅

	[Header("理性ゲージ解放時の減少設定")]
	[SerializeField] protected int Decrease_in_reason_time = 1;     // 理性ゲージ減少ダメージ

	[Header("通常時に時間経過によって理性ゲージ回復する量の設定")]
	[SerializeField] protected int Heal_in_reason_point = 1;        // 理性ゲージ回復量

	[Header("キャラクターごとの固有スキル設定一覧")]
	[Header("毎時体力回復能力(ダチョウ)")]
	[SerializeField] protected int Heal_hp_rate = 2;               // 体力回復割合量(ダチョウ固有)

	[Header("理性解放状態ステータス")]
	[SerializeField] protected int ReasonHP = 200;                  // キャラクター理性解放時最大HP
	[SerializeField] protected int ReasonAttackPower = 50;          // キャラクター理性解放時攻撃力
	[SerializeField] protected int ReasonDefensePower = 60;         // キャラクター理性解放時防御力
	[SerializeField] protected float ReasonMoveSpeed = 1.0f;        // キャラクター移動速度

	[Header("プレイヤー識別番号(1~4)")]
	public int playerID;


	// 実際に計算に使用する倍率（1.0f = 等倍）
	private float currentAtkMult = 1.0f;
	private float currentDefMult = 1.0f;
	private float currentSpdMult = 1.0f;

	// 外部参照用のプロパティ（ベース値 × 倍率）
	public int CurrentAttackPower => (int)(AttackPower * currentAtkMult);
	public int CurrentDefensePower => (int)(DefensePower * currentDefMult);
	public float CurrentMoveSpeed => MoveSpeed * currentSpdMult;

	private Slider hp_gauge;               //HPゲージUIスライダー参照用変数
	private Slider reason_gauge;           //HPゲージUIスライダー参照用変数
	private Animator animator;             //アニメーター参照用変数

	private float timer = 0f;              //タイマー系の変数
	private float ostrichTimer = 0f;       //ダチョウ回復専用タイマー（爆速化防止用）
	private float rhinoDashTimer = 0f;     //サイの突進用タイマー


	public int CurrentHP { get; protected set; }    // キャラクター現在HP(外部読み取り可、内部変更可)
	public int CurrentReason { get; protected set; }    // キャラクター現在理性HP(外部読み取り可、内部変更可)

	InputPlayer input;

	private PlayerManager playerManager;        // プレイヤーマネージャーオブジェクト

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

	State CharaState;                                   // キャラクター状態変数
	Mode CharaMode;                                     // キャラクターモード変数
	public CharacterType CharaAnim;                     // キャラクタータイプ変数
	public bool IsDead => CharaState == State.DEAD;     // 死亡状態かどうかを外部から判定できるプロパティ
														// 初期化
	private void Start()
	{
		SelectAnimal();

		if (playerID > 0)
		{
			CharaAnim = Animal_Select.playerChoices[playerID];
		}

		CharaState = State.IDLE;        // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;        // 初期モードをエニモーに設定
		CurrentHP = MaxHP;              // 現在HPに最大HPを代入
		CurrentReason = MaxReason;      // 現在理性ポイントに最大理性ポイントを代入

		SetBaseStatusByAnimal();        // 選択した動物に応じて基本ステータスを設定する関数呼び出し
		GetResonPoint();                // 理性ゲージ取得
		GetAttackPower();                // 攻撃力取得
		GetDefensePower();              // 防御力取得
		GetMoveSpeed();                  // 移動速度取得

		animator = GetComponent<Animator>();
		input = GetComponent<InputPlayer>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	// Hpゲージと理性ゲージのUIコンポーネントを外部からセットする関数
	public void SetUIComponents(Slider hpSlider, Slider rsSlider)
	{
		this.hp_gauge = hpSlider;
		this.reason_gauge = rsSlider;

		// 初期値をセット
		if (hp_gauge != null)
		{
			hp_gauge.maxValue = MaxHP;
			hp_gauge.value = CurrentHP;
		}
		if (reason_gauge != null)
		{
			reason_gauge.maxValue = MaxReason;
			reason_gauge.value = CurrentReason;
		}
	}

	//更新
	void Update()
	{
		Debug.Log("現在の動物は" + CharaAnim);

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

		Characteristic();                //毎度キャラクターの固有特性を呼び出す
		JudgeModeChange();              //毎度切替を判定する
		CheckAnimatorStateTag();
	}

	// UI更新用の共通関数
	private void UpdateUI()
	{
		if (hp_gauge != null) hp_gauge.value = CurrentHP;
		if (reason_gauge != null) reason_gauge.value = CurrentReason;
	}

	//選択キャラクターによってキャラクタータイプを設定する
	private void SelectAnimal()
	{
		if (SelectLion)
		{
			CharaAnim = CharacterType.LION;
		}
		else if (SelectOstrich)
		{
			CharaAnim = CharacterType.OSTRICH;
		}
		else if (SelectRhinocelos)
		{
			CharaAnim = CharacterType.RHINOCELOS;
		}
		else if (SelectRatel)
		{
			CharaAnim = CharacterType.RATEL;
		}
		else
		{
			CharaAnim = CharacterType.NONE;
		}
	}

	// --- 動物ごとのベース値を決める関数 ---
	private void SetBaseStatusByAnimal()
	{
		//選択した動物の基本ステータスを設定する
		switch (CharaAnim)
		{
			//ライオンの基本ステータス
			case CharacterType.LION:
				MaxHP = LION_HP; MaxReason = 100; AttackPower = LION_ATK;
				DefensePower = LION_DEF; MoveSpeed = LION_SPD;
				break;
			case CharacterType.OSTRICH:
				MaxHP = OSTRICH_HP; MaxReason = 120; AttackPower = OSTRICH_ATK;
				DefensePower = OSTRICH_ATK; MoveSpeed = OSTRICH_SPD;
				break;
			case CharacterType.RHINOCELOS:
				MaxHP = RHINO_HP; MaxReason = 150; AttackPower = RHINO_ATK;
				DefensePower = RHINO_DEF; MoveSpeed = RHINO_SPD;
				break;
			case CharacterType.RATEL:
				MaxHP = RATEL_HP; MaxReason = 100; AttackPower = RATEL_ATK;
				DefensePower = RATEL_DEF; MoveSpeed = RATEL_SPD;
				break;
		}
	}

	//モード切替発動によってチェンジする判定
	private void JudgeModeChange()
	{
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
	}

	// --- 倍率設定用の関数 ---
	private void SetMultiplierByAnimal(bool isReasoning)
	{
		// 通常モードに戻る時は全員 1.0f
		if (!isReasoning)
		{
			currentAtkMult = 1.0f;
			currentSpdMult = 1.0f;
			currentDefMult = 1.0f;
			return;
		}

		// 理性解放時の倍率設定
		switch (CharaAnim)
		{
			//ライオンの理性解放時の倍率設定は攻撃力1.6倍、その他1.3倍（攻撃特化）
			case CharacterType.LION:
				currentAtkMult = LION_REASON_ATK_MULT;
				currentDefMult =DEFAULT_MULT;
				currentSpdMult = DEFAULT_MULT;
				break;

			//ダチョウの理性解放時の倍率設定は移動速度1.5倍、その他1.3倍（速度特化）
			case CharacterType.OSTRICH: // ダチョウ：速度特化
				currentAtkMult = DEFAULT_MULT; 
				currentSpdMult = OSTRICH_REASON_SPD_MULT;
				currentDefMult = DEFAULT_MULT;
				break;

			//サイの理性解放時の倍率設定は防御力1.8倍、その他1.3倍（防御特化）
			case CharacterType.RHINOCELOS: // サイ：防御特化
				currentAtkMult = DEFAULT_MULT;
				currentSpdMult = DEFAULT_MULT;
				currentDefMult = RHINO_REASON_DEF_MULT;
				break;

			//ラーテルの理性解放時の倍率設定は全ステータス1.3倍（バランス型）
			case CharacterType.RATEL:
				currentAtkMult = DEFAULT_MULT;
				currentSpdMult = DEFAULT_MULT;
				currentDefMult = DEFAULT_MULT;
				break;
			default:
				currentAtkMult = 1.0f; currentSpdMult = 1.0f; currentDefMult = 1.0f;
				break;
		}
	}

	//死亡処理関数&ダメージ処理関数
	public virtual void TakeDamage(int damage)
	{
		// もしキャラクターが既に死亡状態であれば、ダメージ処理を行わない
		if (CharaState == State.DEAD) return;


		// モードごとのダメージ処理分岐
		if (CharaMode == Mode.ANIMAL)
		{
			int actualDamage = Mathf.Max(damage - CurrentDefensePower, 1);
			CurrentHP -= actualDamage; // HP減少処理
			//CurrentHP -= damage;
		}
		else if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			// ダメージ計算（防御力を考慮）
			int actualDamage = Mathf.Max(damage - CurrentDefensePower, 1);
			CurrentReason -= actualDamage; // 理性ゲージ減少処理
			CurrentHP -= (int)((float)damage * 0.1f); // HP減少処理
		}

		if (hp_gauge != null) hp_gauge.value = CurrentHP; // HPゲージの現在値を更新
		if (reason_gauge != null) reason_gauge.value = CurrentReason; // 理性ゲージの現在値を更新

		UpdateUI(); // UIの更新関数呼び出し

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
				SetMultiplierByAnimal(true);        // 倍率設定関数呼び出し

				// --- デバッグログ：上昇前後の比較を表示 ---
				Debug.Log($"<color=red>【理性解放】 {CharaAnim}</color>\n" +
						  $"攻撃力: {AttackPower} ➔ {CurrentAttackPower} ({currentAtkMult}倍)\n" +
						  $"防御力: {DefensePower} ➔ {CurrentDefensePower} ({currentDefMult}倍)\n" +
						  $"移動速度: {MoveSpeed} ➔ {CurrentMoveSpeed} ({currentSpdMult}倍)");

				animator.SetBool("Reason", true);
				Debug.Log("理性解放！！");
				break;

			case Mode.SPSIAL_ANIMAL:
				CharaMode = Mode.ANIMAL;
				SetMultiplierByAnimal(false);       // 倍率リセット関数呼び出し

				Debug.Log($"<color=cyan>【通常モードに戻りました】</color>\n" +
					  $"ステータスがベース値（攻撃:{CurrentAttackPower}, 速度:{CurrentMoveSpeed}）に復旧");

				animator.SetBool("Reason", false);
				Debug.Log("通常");
				break;
		}
	}

	

	//死亡処理関数
	protected virtual void Die()
	{
		//もし理性解放中に死亡したなら現在HPを0に設定する
		if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			CurrentHP = 0;  // 現在HPを0に設定
		}
		else if (CharaMode == Mode.ANIMAL)
		{
			CurrentReason = 0;  // 現在理性ポイントを0に設定
		}

		if (hp_gauge != null) hp_gauge.value = 0;
		if (reason_gauge != null) reason_gauge.value = 0;
		Debug.Log("キャラクターが死亡しました。");

		if (CharaState != State.DEAD)
			playerManager.SetDiePlayerList(playerID);

		CharaState = State.DEAD; // 状態を死亡状態に変更
	}

	//エニモー状態時、理性ゲージを回復
	protected virtual void ReasonHeal()
	{
		if (MaxReason != CurrentReason)
		{
			CurrentReason += Heal_in_reason_point;
			Debug.Log("現在の理性ポイント:" + CurrentReason);
		}
	}

	//スペシャルエニモーモード理性ゲージ減少処理関数
	protected virtual void ReasonDecrease()
	{
		//もし理性が0より大きいなら理性ゲージを減少させる
		if (CurrentReason > 0)
		{
			CurrentReason -= Decrease_in_reason_time;                // 理性ゲージ減少処理
			Debug.Log("現在の理性ポイント減少中:");
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

	//キャラの特有の特性関数
	protected virtual void Characteristic()
	{
		switch (CharaAnim)
		{
			case CharacterType.LION:
				break;

			//ダチョウの固有特性(常時体力回復)
			case CharacterType.OSTRICH:
				UniqueSkill_Ostrich();
				break;
			case CharacterType.RHINOCELOS:
				UniqueSkill_Rhinocelos();
				break;
			case CharacterType.RATEL:
				break;
		}
	}

	//ライオンの固有スキル処理関数
	void Skill_Lion()
	{
		Debug.Log("ライオンの固有スキル発動中");
	}

	///ダチョウの固有特性(常時体力回復)
	void UniqueSkill_Ostrich()
	{
		//もし死亡状態なら処理を行わない
		if (CharaState == State.DEAD) return;

		//0でないなら体力回復処理
		if (CurrentHP != 0)
		{
			//理性開放してるなら体力回復処理
			if (CharaMode == Mode.SPSIAL_ANIMAL)
			{
				/*ダチョウの固有スキルは体力を
				時間経過によって回復する*/
				int ostrich_heal = MaxHP * Heal_hp_rate;

				//共通タイマー(timer)ではなく専用タイマーを使用し爆速化を防止
				ostrichTimer += Time.deltaTime;

				// タイマーが1秒以上経過したら体力回復処理を行う
				if (ostrichTimer >= 1f)
				{
					// HP回復処理
					if (MaxHP != CurrentHP)
					{
						CurrentHP += MaxHP * Heal_hp_rate / 100;
						if (CurrentHP > MaxHP) CurrentHP = MaxHP; //最大値を超えないように
						Debug.Log("ダチョウの固有スキルで回復中:" + CurrentHP);
					}
					ostrichTimer = 0f;
				}
			}
		}
		else
		{
			//0なら死亡処理関数呼び出し
			Die();
		}
	}

	//サイの固有スキル処理関数（特性としての枠組み）
	void UniqueSkill_Rhinocelos()
	{
		if (CharaState == State.DEAD) return;

		//長押しによる理性消費のロジック（input.IsSkillPressedはInputPlayerの実装に合わせてください）
		/*
		if (input.IsSkillPressed) 
		{
			rhinoDashTimer += Time.deltaTime;
			if(rhinoDashTimer >= 0.1f) 
			{
				CurrentReason -= 1; //マッハで減らす
				rhinoDashTimer = 0f;
			}
		}
		*/
	}

	//ラーテルの固有スキル処理関数
	void UniqueSkill_Ratel()
	{
		Debug.Log("ラーテルの固有スキル発動中");
	}
}