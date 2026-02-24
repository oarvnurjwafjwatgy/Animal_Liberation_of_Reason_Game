using UnityEngine;
using UnityEngine.UI;
//using static UnityEditor.PlayerSettings;

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
	private const int LION_ATK = 20;
	private const int LION_DEF = 15;
	private const float LION_SPD = 6.0f;

	[Header("ダチョウ ステータス")]
	private const int OSTRICH_HP = 350;
	private const int OSTRICH_ATK = 20;
	private const int OSTRICH_DEF = 13;
	private const float OSTRICH_SPD = 7.5f;

	[Header("サイ ステータス")]
	private const int RHINO_HP = 600;
	private const int RHINO_ATK = 40;
	private const int RHINO_DEF = 25;
	private const float RHINO_SPD = 4.5f;

	[Header("ラーテル ステータス")]
	private const int RATEL_HP = 400;
	private const int RATEL_ATK = 25;
	private const int RATEL_DEF = 20;
	private const float RATEL_SPD = 5.0f;

	// --- 理性解放時の倍率定数 ---
	private const float LION_REASON_ATK_MULT = 1.6f;
	private const float RHINO_REASON_DEF_MULT = 1.5f;
	private const float OSTRICH_REASON_SPD_MULT = 1.5f;
	private const float DEFAULT_MULT = 1.3f;                // 基本的な上昇幅

	// --- 理性ゲージの減少・回復率定数 ---
	private const float REASON_DECREASE_RATE = 0.02f;       // 最大理性ゲージから2%分
	private const float REASON_HEAL_RATE = 0.01f;           // 通常時、最大理性の1%分回復

	[Header("理性ゲージ解放時の減少設定")]
	[SerializeField] protected int Decrease_in_reason_time = 1;     // 理性ゲージ減少ダメージ


	[Header("キャラクターごとの固有特性設定一覧")]
	[Header("ライオン特性：蓄積ダメージ設定")]
	private Image lionRageFill;                                    // 外周ゲージを制御するための変数
	private Transform uiPos;
	private int accumulatedDamage = 0;
	[SerializeField] private int burstThreshold = 80;              // これ以上食らわないと発動しない
	[SerializeField] private float lionBurstDuration = 8f;         // バフが続く秒数（調整可能）
	private float lionBurstSpeedBoost = 1.0f;
	private float lionBurstAtkBoost = 1.0f;
	private float lionBurstTimer = 0f;                             // バフの持続時間用

	[Header("毎時体力回復能力(ダチョウ)")]
	[SerializeField] protected int Heal_hp_rate = 2;               // 体力回復割合量(ダチョウ固有)



	[Header("理性解放状態ステータス")]
	[SerializeField] protected int ReasonHP = 200;                  // キャラクター理性解放時最大HP
	[SerializeField] protected int ReasonAttackPower = 50;          // キャラクター理性解放時攻撃力
	[SerializeField] protected int ReasonDefensePower = 60;         // キャラクター理性解放時防御力
	[SerializeField] protected float ReasonMoveSpeed = 1.0f;        // キャラクター移動速度

	[Header("プレイヤー識別番号(1~4)")]
	public int playerID;

	//スキル関連
	[Header("共通スキル設定")]
	[SerializeField] protected float skillCooldownTimer = 0f; // 現在のCT
	[SerializeField] protected float skillCTMax = 10f;        // スキルの最大CT

	// ライオン専用のバフ変数
	private float lionSkillAtkBoost = 1.0f;
	private float lionSkillDurationTimer = 0f;
	private const float LION_SKILL_DURATION = 5.0f; // バフ持続時間

	// サイの突進状態管理用フラグ
	private bool isRhinoDashing = false; // 突進中かどうか
	private Coroutine rhinoDashCoroutine;
	private float rhinoDashSpeedBoost = 1.0f;

	// --- キャラクター別スキルクールタイム(CT)定数 ---
	private const float LION_CT = 15.0f;     // ライオンは爆発力が高いので長め
	private const float OSTRICH_CT = 8.0f;   // ダチョウは機動力活かしで短め
	private const float RATEL_CT = 12.0f;    // ラーテルはバランス
											 // サイは特殊（CTなし）


	// 実際に計算に使用する倍率（1.0f = 等倍）
	private float currentAtkMult = 1.0f;
	private float currentDefMult = 1.0f;
	private float currentSpdMult = 1.0f;

	// 外部参照用のプロパティ（蓄積バフ倍率も掛け合わせる）
	public int CurrentAttackPower => (int)(AttackPower * currentAtkMult * lionBurstAtkBoost * lionSkillAtkBoost);
	public int CurrentDefensePower => (int)(DefensePower * currentDefMult);
	public float CurrentMoveSpeed => MoveSpeed * currentSpdMult * lionBurstSpeedBoost * rhinoDashSpeedBoost;

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
		//どの動物かを確定させる
		SelectAnimal();
		if (playerID > 0)
		{
			CharaAnim = Animal_Select.playerChoices[playerID];
		}

		SetBaseStatusByAnimal();        // 選択した動物に応じて基本ステータスを設定する関数呼び出し
		CurrentHP = MaxHP;              // 現在HPに最大HPを代入
		CurrentReason = MaxReason;      // 現在理性ポイントに最大理性ポイントを代入

		CharaState = State.IDLE;        // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;        // 初期モードをエニモーに設定
		GetResonPoint();                // 理性ゲージ取得
		GetAttackPower();                // 攻撃力取得
		GetDefensePower();              // 防御力取得
		GetMoveSpeed();                  // 移動速度取得

		animator = GetComponent<Animator>();
		input = GetComponent<InputPlayer>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	// Hpゲージと理性ゲージのUIコンポーネントを外部からセットする関数
	public void SetUIComponents(Slider hpSlider, Slider rsSlider, UIManager uIManager, Transform barPos)
	{
		this.hp_gauge = hpSlider;
		this.reason_gauge = rsSlider;
		this.uiPos = barPos;

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

		// ライオンなら専用アイコンも作る
		if (CharaAnim == CharacterType.LION)
		{
			// UIManagerに同じように頼む（Sliderは返ってこないが生成はされる）
			// uiPosは 1PBarPosition などの Transform
			UIManager.UI_ID id = UIManager.UI_ID.LION_RAGE;

			// 生成された実体は ui_list の最後に入っているのを利用する
			uIManager.CreateUI(id, uiPos, playerID);

			// リストの最後（今作ったアイコン）から Gauge 画像を探す
			GameObject iconObj = uIManager.ui_list[uIManager.ui_list.Count - 1];
			lionRageFill = iconObj.transform.Find("Gauge").GetComponent<Image>();
		}
	}

	//ステータスを再度初期化する（外部から呼び出す用）
	public void ReInitialize(int id)
	{
		this.playerID = id;

		// 1. 選択状況を強制更新
		if (playerID > 0)
		{
			CharaAnim = Animal_Select.playerChoices[playerID];
		}

		// 2. ステータスを再確定
		SetBaseStatusByAnimal();

		// 3. 現在値を満タンに
		CurrentHP = MaxHP;
		CurrentReason = MaxReason;

		Debug.Log($"Player{id} を {CharaAnim} として再初期化しました。HP:{MaxHP}");
	}

	//更新
	void Update()
	{
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

		// --- 共通クールタイムのカウントダウン ---
		if (skillCooldownTimer > 0)
		{
			skillCooldownTimer -= Time.deltaTime;
		}

		// --- ライオンの咆哮バフ時間のカウントダウン ---
		if (CharaAnim == CharacterType.LION && lionSkillDurationTimer > 0)
		{
			lionSkillDurationTimer -= Time.deltaTime;
			if (lionSkillDurationTimer <= 0)
			{
				lionSkillAtkBoost = 1.0f; // 時間切れで攻撃力倍率を等倍に戻す
				Debug.Log("<color=white>ライオン：咆哮の効果が終了した</color>");
			}
		}

		// ライオンの専用UIの更新
		if (CharaAnim == CharacterType.LION && lionRageFill != null)
		{
			if (lionBurstTimer > 0)
			{
				// バフ発動中：残り時間をカウントダウン（赤色など）
				lionRageFill.fillAmount = lionBurstTimer / lionBurstDuration;
				lionRageFill.color = Color.red;
			}
			else
			{
				// 蓄積中：ダメージの溜まり具合を表示（黄色など）
				float ratio = (float)accumulatedDamage / burstThreshold;
				lionRageFill.fillAmount = Mathf.Clamp01(ratio);

				// 溜まったら色を変えて教える（オレンジなど）
				lionRageFill.color = (ratio >= 1f) ? new Color(1f, 0.5f, 0f) : Color.yellow;
			}
		}

		JudgeModeChange();              //毎度切替を判定する
		CheckAnimatorStateTag();


		// ライオンのバーストバフタイマー管理
		if (CharaAnim == CharacterType.LION && lionBurstTimer > 0)
		{
			lionBurstTimer -= Time.deltaTime;
			if (lionBurstTimer <= 0)
			{
				// 時間切れでバフをリセット
				lionBurstSpeedBoost = 1.0f;
				lionBurstAtkBoost = 1.0f;
				Debug.Log("<color=white>ライオン：憤怒のバフが終了した</color>");
			}
		}
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
				DefensePower = OSTRICH_DEF; MoveSpeed = OSTRICH_SPD;
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

				// ダチョウの固有特性（体力回復）も同時に呼び出す
				if (CharaAnim == CharacterType.OSTRICH)
				{
					UniqueSkill_Ostrich();
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
				currentDefMult = DEFAULT_MULT;
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
			//int actualDamage = Mathf.Max(damage - CurrentDefensePower, 1);

			// 攻撃力の 20% は防御を無視して必ず通る
			int actualDamage = Mathf.Max(damage - CurrentDefensePower, (int)(damage * 0.2f));
			CurrentHP -= actualDamage; // HP減少処理

			// 今いくら防いだか
			Debug.Log($"<color=yellow>【被弾】 元ダメ:{damage} -> 防御後:" +
			$"{actualDamage} (現在の防御力:{CurrentDefensePower})</color>");

			// 通常モードかつライオンなら、受けた実ダメージを蓄積
			if (CharaAnim == CharacterType.LION && CharaMode == Mode.ANIMAL)
			{
				accumulatedDamage += actualDamage;
				Debug.Log($"ライオン：ダメージ蓄積中" +
				$"（現在：{accumulatedDamage} / しきい値：{burstThreshold}）");
			}
		}
		else if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			// ダメージ計算（防御力を考慮）
			int actualDamage = Mathf.Max(damage - CurrentDefensePower, 1);
			CurrentReason -= actualDamage; // 理性ゲージ減少処理
			CurrentHP -= (int)((float)damage * 0.1f); // HP減少処理

			// 今いくら防いだか
			Debug.Log($"<color=yellow>【被弾】 元ダメ:{damage} -> 防御後:" +
			$"{actualDamage} (現在の防御力:{CurrentDefensePower})</color>");
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

				//ライオンなら
				if (CharaAnim == CharacterType.LION)
				{
					//ライオンの特性関数呼び出し（蓄積ダメージに応じてさらに強くなる）
					Characteristic();
				}

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
		// サイの突進を強制停止
		if (rhinoDashCoroutine != null)
		{
			StopCoroutine(rhinoDashCoroutine);
			rhinoDashCoroutine = null;
		}

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
			// 最大理性の1%を計算。最低でも1は回復させる
			int healAmount = Mathf.Max((int)(MaxReason * REASON_HEAL_RATE), 1);

			CurrentReason += healAmount;

			// 最大値を超えないように制限
			if (CurrentReason > MaxReason) CurrentReason = MaxReason;

			Debug.Log("現在の理性ポイント:" + CurrentReason);
		}
	}

	//スペシャルエニモーモード理性ゲージ減少処理関数
	protected virtual void ReasonDecrease()
	{
		//もし理性が0より大きいなら理性ゲージを減少させる
		if (CurrentReason > 0)
		{
			//最大理性ポイントに減少率をかけて減少量を計算し、最低でも1は減少するようにする
			int decreaseAmount = Mathf.Max((int)(MaxReason * REASON_DECREASE_RATE), 1);
			CurrentReason -= decreaseAmount;                // 理性ゲージ減少処理
			Debug.Log($"{CharaAnim}の理性減少中: 残り{CurrentReason} (毎秒{decreaseAmount}減)");
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
				UniqueSkill_Lion();
				break;
			//ダチョウの固有特性(常時体力回復)
			case CharacterType.OSTRICH:
				UniqueSkill_Ostrich();
				break;
			case CharacterType.RHINOCELOS:
				break;
			case CharacterType.RATEL:
				break;
		}
	}

	// キャラ特有のスキル実行
	public virtual void Skill()
	{
		// 死亡時、またはCT中は発動不可（サイ以外）
		if (CharaState == State.DEAD || (skillCooldownTimer > 0 && CharaAnim != CharacterType.RHINOCELOS))
			return;

		switch (CharaAnim)
		{
			case CharacterType.LION:
				Skill_Lion();
				skillCooldownTimer = LION_CT; // ライオン用のCTをセット
				break;

			case CharacterType.OSTRICH:
				// Skill_Ostrich(); // ダチョウのスキル
				skillCooldownTimer = OSTRICH_CT;
				break;

			case CharacterType.RHINOCELOS:
				Skill_Rhinocelos(); // サイは CT セットなし（理性が続く限り）
				break;

			case CharacterType.RATEL:
				UniqueSkill_Ratel();
				skillCooldownTimer = RATEL_CT;
				break;
		}
	}

	//ライオンの固有スキル処理関数
	void Skill_Lion()
	{
		// 理性解放中かどうかで倍率を変化（覚醒ならより強く！）
		if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			lionSkillAtkBoost = 1.7f; // 解放中は 1.7倍！
			Debug.Log("<color=red>【王者の咆哮：覚醒】一気に決める！</color>");
		}
		else
		{
			lionSkillAtkBoost = 1.3f; // 通常時は 1.3倍
			Debug.Log("<color=orange>【王者の咆哮】牙を剥く！</color>");
		}

		lionSkillDurationTimer = LION_SKILL_DURATION; // 5秒間持続

		// アニメーション再生などの処理
		//if (animator != null) animator.SetTrigger("Skill_Roar");
	}


	//サイの固有スキル処理関数
	void Skill_Rhinocelos()
	{
		// もし既に実行中なら、止める
		if (rhinoDashCoroutine != null)
		{
			StopCoroutine(rhinoDashCoroutine);
			rhinoDashSpeedBoost = 1.0f; // 速度を元に戻す
			isRhinoDashing = false;     // フラグを下ろす
			rhinoDashCoroutine = null;  // 参照を消す
			Debug.Log("<color=white>サイ：突進を中止しました</color>");
			return;
		}

		// 実行中でなければ、コルーチンを開始してループ処理を開始
		rhinoDashCoroutine = StartCoroutine(RhinoDashLoop());


		//if (isRhinoDashing)
		//	rhinoDashTimer += Time.deltaTime;

		////理性が0より大きいなら突進処理を行う
		//if (CurrentReason > 0)
		//{
		//	if (rhinoDashTimer >= 0.1f)
		//	{
		//		CurrentReason -= 1;         //マッハで減らす
		//		rhinoDashTimer = 0f;
		//		TakeDamage(0);              //毎度ダメージ関数を呼び出し判定してもらう
		//	}
		//}
	}

	// 突進中の「継続処理」をここに完結させる
	private System.Collections.IEnumerator RhinoDashLoop()
	{
		isRhinoDashing = true;
		rhinoDashSpeedBoost = 1.8f; // 突進開始！速度を1.8倍にアップ
		Debug.Log("<color=orange>サイ：突進スキル発動！猛スピードで理性を消費します</color>");

		while (CurrentReason > 0 && isRhinoDashing)
		{
			yield return new WaitForSeconds(0.1f);
			CurrentReason -= 1;

			UpdateUI();
			TakeDamage(0);

			if (CurrentReason <= 0 || CurrentHP <= 0)
			{
				// 理性が尽きた場合などはループを抜ける
				break;
			}
		}

		// 終了処理（ここを通れば必ず速度が元に戻る）
		rhinoDashSpeedBoost = 1.0f;
		rhinoDashCoroutine = null;
		isRhinoDashing = false;
		Debug.Log("<color=white>サイ：突進終了。速度が戻りました</color>");
	}



	//ライオンの固有特性処理関数
	void UniqueSkill_Lion()
	{
		// しきい値を超えている場合のみバフを計算
		if (accumulatedDamage >= burstThreshold)
		{
			// 蓄積量に応じて強化幅を変える（最大1.4倍、攻撃1.1倍など）
			float extraPower = (float)(accumulatedDamage - burstThreshold) / 150f;
			lionBurstSpeedBoost = 1.25f + Mathf.Min(extraPower, 0.15f);
			lionBurstAtkBoost = 1.15f;

			// ここで持続時間をセット
			lionBurstTimer = lionBurstDuration;
			Debug.Log($"<color=red>【特性発動】憤怒解放！ {lionBurstDuration}秒間、爆速モード！</color>");
		}
		else
		{
			// 足りなければ通常通りの解放（バフなし）
			lionBurstSpeedBoost = 1.0f;
			lionBurstAtkBoost = 1.0f;
			lionBurstTimer = 0f;
			Debug.Log($"<color=white>蓄積不足({accumulatedDamage}/{burstThreshold})のため特性は不発</color>");
		}

		// 特性成否に関わらず、一度解放したら蓄積はリセット（「溜め」の戦略性を出すため）
		accumulatedDamage = 0;
	}

	///ダチョウの固有特性(常時体力回復)
	void UniqueSkill_Ostrich()
	{
		//もし死亡状態なら処理を行わない
		if (CharaState == State.DEAD) return;

		//0でないなら体力回復処理
		if (CurrentHP > 0)
		{
			//理性開放してるなら体力回復処理
			if (CharaMode == Mode.SPSIAL_ANIMAL)
			{
				/*ダチョウの固有スキルは体力を
				時間経過によって回復する*/
				int ostrich_heal = MaxHP * Heal_hp_rate;

				//共通タイマー(timer)ではなく専用タイマーを使用し爆速化を防止
				ostrichTimer += Time.deltaTime;
				Debug.Log("ダチョウ特性チェック中");
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



	//ラーテルの固有スキル処理関数
	void UniqueSkill_Ratel()
	{
		Debug.Log("ラーテルの固有スキル発動中");
	}
}