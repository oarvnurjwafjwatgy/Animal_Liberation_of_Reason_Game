using UnityEngine;
using UnityEngine.UI;

public class Character_Status : MonoBehaviour
{
	// ステータス
	public int MaxHP { get; private set; }
	public int MaxReason { get; private set; }
	public int AttackPower { get; private set; }
	public int DefensePower { get; private set; }
	public float MoveSpeed { get; private set; }

	[Header("キャラクターごとの固有特性設定一覧")]
	[Header("ライオン特性：蓄積ダメージ設定")]
	private Image lionRageFill;                                    // 外周ゲージを制御するための変数
	private GameObject lionRageUIRoot;                             // アイコン全体を制御するための変数
	private Transform uiPos;
	private int accumulatedDamage = 0;
	[SerializeField] private int burstThreshold = 80;              // これ以上食らわないと発動しない
	[SerializeField] private float lionBurstDuration = 8f;         // バフが続く秒数（調整可能）
	private float lionBurstSpeedBoost = 1.0f;
	private float lionBurstAtkBoost = 1.0f;
	private float lionBurstTimer = 0f;                             // バフの持続時間用


	[Header("プレイヤー識別番号(1~4)")]
	public int playerID;

	// バフ・デバフ管理用の列挙型と変数
	public enum BuffType { SpeedBuff, SpeedDebuff, AttackBuff, AttackDebuff, RhinoDash }

	public Transform buffContainer;

	// サイの突進状態管理用フラグ
	private bool isRhinoDashing = false; // 突進中かどうか
	private Coroutine rhinoDashCoroutine;
	private float rhinoDashSpeedBoost = 1.0f;

	// --- キャラクター別スキルクールタイム(CT)定数 ---
	
											 // サイは特殊（CTなし）

	// 実際に計算に使用する倍率（1.0f = 等倍）
	private float currentAtkMult = 1.0f;
	private float currentDefMult = 1.0f;
	private float currentSpdMult = 1.0f;

	// 外部参照用のプロパティ（蓄積バフ倍率も掛け合わせる）
	//public virtual int CurrentAttackPower => (int)(AttackPower * currentAtkMult * lionBurstAtkBoost
	//* lionSkillAtkBoost * (1f + GetComponent<NutsEffectManager>().CurrentAttackModifier));
	//public virtual int CurrentDefensePower => (int)(DefensePower * currentDefMult);
	//public virtual float CurrentMoveSpeed => MoveSpeed * currentSpdMult * lionBurstSpeedBoost
	//* rhinoDashSpeedBoost * (1f + GetComponent<NutsEffectManager>().CurrentSpeedModifier);

	// 攻撃の値
	public virtual int CurrentAttackPower
	{
		get
		{
			float animalBuff = 1.0f;

			// 自分についているスキル親クラスを取得（例:中身がライオンならライオンの倍率も含める）
			Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();
			if (skillComponent != null) animalBuff = skillComponent.CurrentAtkBoost;

			return (int)(AttackPower * currentAtkMult * animalBuff * (1f + GetComponent<NutsEffectManager>().CurrentAttackModifier));
		}
	}

	// 防御の値
	public virtual int CurrentDefensePower => (int)(DefensePower * currentDefMult);

	//public virtual float CurrentMoveSpeed =>
	//	MoveSpeed * currentSpdMult * rhinoDashSpeedBoost * (1f + GetComponent<NutsEffectManager>().CurrentSpeedModifier);

	// 速度の値
	public virtual float CurrentMoveSpeed
	{
		get
		{
			Animal_Skill_TraitBase skill = GetComponent<Animal_Skill_TraitBase>();
			float skillSpeed = 1.0f;

			if (skill != null) skillSpeed = skill.CurrentSpeedBoost;

			return MoveSpeed
				* currentSpdMult
				* skillSpeed
				* (1f + GetComponent<NutsEffectManager>().CurrentSpeedModifier);
		}
	}

	private Slider hp_gauge;                    //HPゲージUIスライダー参照用変数
	private Slider reason_gauge;                //HPゲージUIスライダー参照用変数
	private Animator animator;                  //アニメーター参照用変数
	private Animal_Skill_TraitBase animl_skill; //スキルに参照

	public UIManager MyUIManager { get; private set; } // UIManagerへの参照

	private float timer = 0f;              //タイマー系の変数

	public int CurrentHP { get; protected set; }    // キャラクター現在HP(外部読み取り可、内部変更可)
	public int CurrentReason { get; protected set; }    // キャラクター現在理性HP(外部読み取り可、内部変更可)

	private PlayerManager playerManager;        // プレイヤーマネージャーオブジェクト

	/**********状態*******************/
	public enum State
	{
		IDLE,       // 待機状態
		MOVE,       // 移動状態
		ATTAKING,   // 攻撃状態
		DEAD        // 死亡状態
	}

	/**********モード*******************/
	public enum Mode { ANIMAL, SPSIAL_ANIMAL }   // 通常&理性解放

	/**********キャラクタータイプ*******************/
	public enum CharacterType
	{
		NONE,           // 無し
		LION,           // ライオン
		OSTRICH,        // ダチョウ
		RHINOCELOS,     // サイ
		RATEL,          // ラーテル
	}

	protected State CharaState;                                   // キャラクター状態変数
	protected Mode CharaMode;                                     // キャラクターモード変数
	public CharacterType CharaAnim;                     // キャラクタータイプ変数
	public bool IsDead => CharaState == State.DEAD;     // 死亡状態かどうかを外部から判定できるプロパティ

	// 初期化
	private void Start()
	{
		if (playerID > 0)
			CharaAnim = Animal_Select.playerChoices[playerID];

		SetAnimalScripts(CharaAnim);    //選んだ動物に合わせて動物ごとの専用スクリプトを張り付ける
		SetBaseStatusByAnimal();        // 選択した動物に応じて基本ステータスを設定する関数呼び出し
		SetHP(MaxHP);                   // 現在HPに最大HPを代入
		SetReason(MaxReason);           // 現在理性ポイントに最大理性ポイントを代入

		CharaState = State.IDLE;        // 初期状態を待機状態に設定
		CharaMode = Mode.ANIMAL;        // 初期モードをエニモーに設定

		animator = GetComponent<Animator>();
		animl_skill = GetComponent<Animal_Skill_TraitBase>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	// Hpゲージと理性ゲージのUIコンポーネントを外部からセットする関数
	public virtual void SetUIComponents(Slider hpSlider, Slider rsSlider, UIManager uIManager, Transform barPos)
	{
		this.hp_gauge = hpSlider;
		this.reason_gauge = rsSlider;
		this.uiPos = barPos;
		this.MyUIManager = uIManager;

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
			uIManager.CreateUI(UIManager.UI_ID.LION_RAGE, uiPos, playerID);

			// 生成されたアイコンはリストの最後に追加されるはずなので、そこから参照する
			lionRageUIRoot = uIManager.ui_list[uIManager.ui_list.Count - 1];

			// リストの最後（今作ったアイコン）から Gauge 画像を探す
			GameObject iconObj = uIManager.ui_list[uIManager.ui_list.Count - 1];
			lionRageFill = iconObj.transform.Find("Gauge").GetComponent<Image>();
		}

		uIManager.CreateUI(UIManager.UI_ID.BUFF_CONTAINER, uiPos, playerID);
		buffContainer = uIManager.ui_list[uIManager.ui_list.Count - 1].transform;
	}

	//ステータスを再度初期化する（外部から呼び出す用）
	public void ReInitialize(int id)
	{
		this.playerID = id;
		if (playerID > 0) CharaAnim = Animal_Select.playerChoices[playerID];// 選択状況を強制更新

		SetBaseStatusByAnimal();// ステータスを再確定
		SetHP(MaxHP);           // 現在値を満タンにする
		SetReason(MaxReason);
		Debug.Log($"Player{id} を {CharaAnim} として再初期化しました。HP:{MaxHP}");
	}

	// 選ばられた動物に合わせて適正のスクリプトをセットする
	public void SetAnimalScripts(CharacterType animaltype)
	{
		switch (animaltype)
		{
			case CharacterType.LION: gameObject.AddComponent<Character_Lion>(); break;
			case CharacterType.OSTRICH: gameObject.AddComponent<Character_Ostrich>();break;
			case CharacterType.RHINOCELOS: gameObject.AddComponent<Character_Rhinocelos>(); break;
			case CharacterType.RATEL: gameObject.AddComponent<Character_HoneyBadger>();break;
		}
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
		//      if (CharaAnim == CharacterType.LION && lionBurstTimer > 0)
		//{
		//	lionBurstTimer -= Time.deltaTime;
		//	if (lionBurstTimer <= 0)
		//	{
		//		// 時間切れでバフをリセット
		//		lionBurstSpeedBoost = 1.0f;
		//		lionBurstAtkBoost = 1.0f;
		//		Debug.Log("<color=white>ライオン：憤怒のバフが終了した</color>");
		//	}
		//}
	}

	// UI更新用の共通関数
	private void UpdateUI()
	{
		if (hp_gauge != null) hp_gauge.value = CurrentHP;
		if (reason_gauge != null) reason_gauge.value = CurrentReason;
	}

	// --- 動物ごとのベース値を決める関数 ---
	private void SetBaseStatusByAnimal()
	{
		//選択した動物の基本ステータスを設定する
		switch (CharaAnim)
		{
			case CharacterType.LION:
				ApplyParam(CharacterData.Lion);
				break;
			case CharacterType.OSTRICH:
				ApplyParam(CharacterData.Ostrich);
				break;
			case CharacterType.RHINOCELOS:
				ApplyParam(CharacterData.Rhinocelos);
				break;
			case CharacterType.RATEL:
				ApplyParam(CharacterData.Ratel);
				break;
			default:
				Debug.LogError("動物が選択されていません");
				break;
		}
	}

	//別ファイルから読み込んだデータを、実際のステータス変数に代入する処理
	private void ApplyParam(AnimalParam param)
	{
		MaxHP = param.maxHP;
		MaxReason = param.maxReason;
		AttackPower = param.attackPower;
		DefensePower = param.defensePower;
		MoveSpeed = param.moveSpeed;
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

		// 現在選択されている動物のデータを取得して倍率を適用
		AnimalParam currentParam = GetCurrentAnimalParam();
		currentAtkMult = currentParam.reasonAtkMult;
		currentDefMult = currentParam.reasonDefMult;
		currentSpdMult = currentParam.reasonSpdMult;
	}

	// 現在の動物のパラメーターを返す関数
	private AnimalParam GetCurrentAnimalParam()
	{
		switch (CharaAnim)
		{
			case CharacterType.LION: return CharacterData.Lion;
			case CharacterType.OSTRICH: return CharacterData.Ostrich;
			case CharacterType.RHINOCELOS: return CharacterData.Rhinocelos;
			case CharacterType.RATEL: return CharacterData.Ratel;
			default: return default;
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

		UpdateUI(); // UIの更新関数呼び出し

		//死亡直前の特性チェック
		if (CurrentHP <= 0)
		{
			Animal_Skill_TraitBase trait = GetComponent<Animal_Skill_TraitBase>();
			if (trait != null && trait.OnFatalDamage()) { return; }
		}
		// 最終死亡判定
		if (CurrentHP <= 0 || CurrentReason <= 0) { Die(); }
	}

	//現在HP取得関数
	//public int GetCurrentHP()
	//{
	//	return CurrentHP;
	//}
	//移動速度取得関数
	//public float GetMoveSpeed()
	//{
	//    return MoveSpeed;
	//}

	//防御力取得関数
	public int GetDefensePower(){ return DefensePower; }

	//主に体力・理性ゲージの回復に使用する上限付の関数
	public void NotDied(int hp, int reason)
	{
		CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
		CurrentReason = Mathf.Clamp(reason, 0, MaxReason);
	}

	//モード
	public Mode GetMode() { return CharaMode; }

	// 現在のステート状態をreturnで返す
	public State GetState() { return CharaState; }

	//現在の体力を引数から代入させる関数
	public void SetHP(int hp) { CurrentHP = hp; }
	//上記同様に理性ゲージも同じく代入させる関数
	public void SetReason(int reason) { CurrentReason = reason; }

	//モードが切り替え時に呼び出す関数
	public void GetModeChange()
	{
		switch (CharaMode)
		{
			case Mode.ANIMAL:
				CharaMode = Mode.SPSIAL_ANIMAL;
				SetMultiplierByAnimal(true);        // 倍率設定関数呼び出し

				//ライオンなら
				//if (CharaAnim == CharacterType.LION)
				//{
				//	//ライオンの特性関数呼び出し（蓄積ダメージに応じてさらに強くなる）
				//	Characteristic();
				//}

				Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();
				if (animl_skill != null) skillComponent.Characteristic();

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
		//if (rhinoDashCoroutine != null)
		//{
		//	StopCoroutine(rhinoDashCoroutine);
		//	rhinoDashCoroutine = null;
		//}

		//もし理性解放中に死亡したなら現在HP/通常なら理性を0に設定する
		if (CharaMode == Mode.SPSIAL_ANIMAL) SetHP(0);
		else if (CharaMode == Mode.ANIMAL) SetReason(0);

		if (hp_gauge != null) hp_gauge.value = 0;
		if (reason_gauge != null) reason_gauge.value = 0;


		//体力ゲージ・理性ゲージのUIを非表示にする処理
		if (hp_gauge != null) hp_gauge.gameObject.SetActive(false);
		if (reason_gauge != null) reason_gauge.gameObject.SetActive(false);

		// ライオンの専用UIも非表示にする
		if (lionRageUIRoot != null) lionRageUIRoot.gameObject.SetActive(false);

		Debug.Log($"{gameObject.name} が死亡したため、UIを非表示にしました。");

		if (CharaState != State.DEAD) playerManager.SetDiePlayerList(playerID);

		CharaState = State.DEAD; // 状態を死亡状態に変更
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
				}
			}
		}
	}
	// キャラ特有のスキル実行
	//public virtual void Skill()
	//{
	//	//// 死亡時、またはCT中は発動不可（サイ以外）
	//	//if (CharaState == State.DEAD || (skillCooldownTimer > 0 && CharaAnim != CharacterType.RHINOCELOS))
	//	//	return;

	//	//switch (CharaAnim)
	//	//{
	//	//	case CharacterType.LION:
	//	//		//Skill_Lion();
	//	//		//skillCooldownTimer = LION_CT; // ライオン用のCTをセット
	//	//		break;

	//	//	case CharacterType.OSTRICH:
	//			 Skill_Ostrich(); // ダチョウのスキル
	//	//		skillCooldownTimer = OSTRICH_CT;
	//	//		break;

	//	//	case CharacterType.RHINOCELOS:
	//	//		Skill_Rhinocelos(); // サイは CT セットなし（理性が続く限り）
	//	//		break;

	//	//	case CharacterType.RATEL:
	//	//		UniqueSkill_Ratel();
	//	//		skillCooldownTimer = RATEL_CT;
	//	//		break;
	//	//}

	//	Debug.Log($"本体のSkillが呼ばれました。現在の状態: {CharaState}");

	//	if (CharaState == State.DEAD) return;
	//	Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();

	//	if (skillComponent != null)
	//		skillComponent.Skill();
	//}

	// スキル実行
	public virtual void Skill()
	{
		Debug.Log("本体Skill");

		Animal_Skill_TraitBase skillComponent =
			GetComponent<Animal_Skill_TraitBase>();

		if (skillComponent != null)
		{
			Debug.Log("skillComponent発見");
			Debug.Log(skillComponent.GetType().Name);
			skillComponent.Skill();
		}
	}

	/*******回復処理*********/
	//指定した値分の回復
	public void HealHP(int amount)
	{
		CurrentHP += amount;
		if (CurrentHP > MaxHP) SetHP(MaxHP);
	}
	public void HealReason(int amount)
	{
		CurrentReason += amount;
		if (CurrentReason > MaxReason) SetReason(MaxReason);
	}

	//通常時は理性ゲージを回復(割合時間経過回復)
	protected virtual void ReasonHeal()
	{
		if (MaxReason != CurrentReason)
		{
			// 最大理性の1%を計算。最低でも1は回復させる
			int healAmount = Mathf.Max((int)(MaxReason * CharacterData.REASON_HEAL_RATE), 1);
			HealReason(healAmount);
		}
	}

	//理性解放状態時:理性ゲージ減少処理関数
	protected virtual void ReasonDecrease()
	{
		//もし理性が0より大きいなら理性ゲージを減少させる
		if (CurrentReason > 0)
		{
			//最大理性ポイントに減少率をかけて減少量を計算し、最低でも1は減少するようにする
			int decreaseAmount = Mathf.Max((int)(MaxReason * CharacterData.REASON_DECREASE_RATE), 1);
			ReducedReasoning(decreaseAmount);    // 理性ゲージ減少処理
			Debug.Log($"{CharaAnim}の理性減少中: 残り{CurrentReason} (毎秒{decreaseAmount}減)");
		}
		//0以下なら理性ゲージを0・死亡処理を行う
		else
		{
			animator.SetBool("Reason_Dead", true);
			Die();
		}
	}
	
	// 現在の理性ゲージに引数分引く
	public void ReducedReasoning(int amount) { CurrentReason -= amount; }

	//外部から呼び出すための関数
	public void ForceDie() { Die(); }

	//サイの固有スキル処理関数
	//void Skill_Rhinocelos()
	//{
	//	// もし既に実行中なら、止める
	//	if (rhinoDashCoroutine != null)
	//	{
	//		// 止める前に掃除をする
	//		if (MyUIManager != null)
	//			MyUIManager.RemoveBuffUI(playerID, BuffType.SpeedBuff);

	//		StopCoroutine(rhinoDashCoroutine);
	//		rhinoDashSpeedBoost = 1.0f; // 速度を元に戻す
	//		isRhinoDashing = false;     // フラグを下ろす
	//		rhinoDashCoroutine = null;  // 参照を消す
	//		Debug.Log("<color=white>サイ：突進を中止しました</color>");
	//		return;
	//	}

	//	// 実行中でなければ、コルーチンを開始してループ処理を開始
	//	rhinoDashCoroutine = StartCoroutine(RhinoDashLoop());
	//}

	//// 突進中の「継続処理」をここに完結させる
	//private System.Collections.IEnumerator RhinoDashLoop()
	//{
	//	isRhinoDashing = true;

	//	if (MyUIManager != null)
	//		MyUIManager.CreateOrUpdateBuffUI(playerID, BuffType.SpeedBuff, 999f, buffContainer);

	//	rhinoDashSpeedBoost = 1.8f; // 突進開始！速度を1.8倍にアップ
	//	Debug.Log("<color=orange>サイ：突進スキル発動！猛スピードで理性を消費します</color>");

	//	while (CurrentReason > 0 && isRhinoDashing)
	//	{
	//		yield return new WaitForSeconds(0.1f);
	//		CurrentReason -= 1;

	//		UpdateUI();
	//		TakeDamage(0);

	//		if (CurrentReason <= 0 || CurrentHP <= 0)
	//		{
	//			break;// 理性が尽きた場合などはループを抜ける
	//		}
	//	}
	//	// ループを抜けたら、バフを消して速度を元に戻す
	//	if (MyUIManager != null)
	//		MyUIManager.RemoveBuffUI(playerID, BuffType.SpeedBuff);

	//	// 終了処理（ここを通れば必ず速度が元に戻る）
	//	rhinoDashSpeedBoost = 1.0f;
	//	rhinoDashCoroutine = null;
	//	isRhinoDashing = false;
	//	Debug.Log("<color=white>サイ：突進終了。速度が戻りました</color>");
	//}

	// 奈落に落ちていった時に呼ばれる
	public void DieAbyss() { this.Die(); }

	// 範囲外に出た時に呼ばれる
	public void OutOfRangeDamage()
	{
		CurrentHP -= (int)((float)MaxHP * 0.05);

		// 死亡判定
		if (CurrentHP <= 0) this.Die();
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

			if (MyUIManager != null)
				MyUIManager.CreateOrUpdateBuffUI(playerID, BuffType.SpeedBuff, lionBurstDuration, buffContainer);

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
}