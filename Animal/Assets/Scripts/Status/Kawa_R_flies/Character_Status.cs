using UnityEngine;
using UnityEngine.UI;
using Mode = ChangeMode;
using animalP = AnimalParam;
using State = AnimalState.State;

public class Character_Status : MonoBehaviour
{
	// ステータス
	public int MaxHP { get; private set; }
	public int MaxReason { get; private set; }
	public int AttackPower { get; private set; }
	public int DefensePower { get; private set; }
	public float MoveSpeed { get; private set; }
	public int CurrentHP { get; protected set; }    // キャラクター現在HP(外部読み取り可、内部変更可)
	public int CurrentReason { get; protected set; }    // キャラクター現在理性HP(外部読み取り可、内部変更可)

	public UIManager MyUIManager { get; private set; } // UIManagerへの参照

	// 実際に計算に使用する倍率（1.0f = 等倍）
	private float currentAtkMult = CharacterData.INITIAL_MAGNIFICATION;
	private float currentDefMult = CharacterData.INITIAL_MAGNIFICATION;
	private float currentSpdMult = CharacterData.INITIAL_MAGNIFICATION;

	// 外部参照用のプロパティ（蓄積バフ倍率も掛け合わせる）
	// 攻撃の値
	public virtual int CurrentAttackPower
	{
		get
		{
			float animalBuff = CharacterData.INITIAL_MAGNIFICATION;

			// 自分についているスキル親クラスを取得（例:中身がライオンならライオンの倍率も含める）
			Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();
			if (skillComponent != null) animalBuff = skillComponent.CurrentAtkBoost;

			return (int)(AttackPower * currentAtkMult * animalBuff * (1f + GetComponent<NutsEffectManager>().CurrentAttackModifier));
		}
	}

	// 防御の値
	public virtual int CurrentDefensePower => (int)(DefensePower * currentDefMult);

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
	[Header("プレイヤー識別番号(1~4)")]
	public int playerID;

	private Slider hp_gauge;                    //HPゲージUIスライダー参照用変数
	private Slider reason_gauge;                //HPゲージUIスライダー参照用変数
	private Animator animator;                  //アニメーター参照用変数
	private Animal_Skill_TraitBase animl_skill; //スキルに参照

	private PlayerManager playerManager;    // プレイヤーマネージャーオブジェクト
	protected State CharaState;             // キャラクター状態
	protected Transform uiPos;				// UI出現位置
	protected Mode CharaMode;               // キャラクターモード
	public CharacterType CharaAnim;         // キャラクタータイプ
	public Transform buffContainer;			// バフ

	public Transform UiPos => uiPos;
	public bool IsDead => CharaState == State.DEAD;     // 死亡状態かどうかを外部から判定できるプロパティ

	//変数
	private float timer = animalP.TIMER_RESET;              //タイマー


	// 選ばられた動物に合わせて適正のスクリプトをセットする
	public void SetAnimalScripts(CharacterType animaltype)
	{
		switch (animaltype)
		{
			case CharacterType.LION: gameObject.AddComponent<Character_Lion>(); break;
			case CharacterType.OSTRICH: gameObject.AddComponent<Character_Ostrich>(); break;
			case CharacterType.RHINOCELOS: gameObject.AddComponent<Character_Rhinocelos>(); break;
			case CharacterType.RATEL: gameObject.AddComponent<Character_HoneyBadger>(); break;
		}
	}

	// --- 動物ごとのベース値を決める関数 ---
	private void SetBaseStatusByAnimal()
	{
		//選択した動物の基本ステータスを設定する
		switch (CharaAnim)
		{
			case CharacterType.LION: ApplyParam(CharacterData.Lion); break;
			case CharacterType.OSTRICH: ApplyParam(CharacterData.Ostrich); break;
			case CharacterType.RHINOCELOS: ApplyParam(CharacterData.Rhinocelos); break;
			case CharacterType.RATEL: ApplyParam(CharacterData.Ratel); break;
			default: Debug.LogError("動物が選択されていません"); break;
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

	// Hpゲージと理性ゲージのUIコンポーネントを外部からセットする関数
	public virtual void SetUIComponents(Slider hpSlider, Slider rsSlider, UIManager uIManager, Transform barPos)
	{
		this.hp_gauge = hpSlider;
		this.reason_gauge = rsSlider;
		this.uiPos = barPos;
		this.MyUIManager = uIManager;

		// 初期値をセット
		if (hp_gauge != null) { hp_gauge.maxValue = MaxHP; hp_gauge.value = CurrentHP; }
		if (reason_gauge != null) { reason_gauge.maxValue = MaxReason; reason_gauge.value = CurrentReason; }

		uIManager.CreateUI(UIManager.UI_ID.BUFF_CONTAINER, uiPos, playerID);
		buffContainer = uIManager.ui_list[uIManager.ui_list.Count - 1].transform;
	}

	// 初期化
	private void Start()
	{
		if (playerID > 0) CharaAnim = Animal_Select.playerChoices[playerID];

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

	//更新
	void Update()
	{
		// HPゲージの現在値を更新
		if (hp_gauge != null && reason_gauge != null)
		{ hp_gauge.value = CurrentHP; reason_gauge.value = CurrentReason; }

		JudgeModeChange();              //毎度切替を判定する
		CheckAnimatorStateTag();
	}

	// UI更新用の共通関数
	private void UpdateUI()
	{
		if (hp_gauge != null) hp_gauge.value = CurrentHP;
		if (reason_gauge != null) reason_gauge.value = CurrentReason;
	}

	//モード切替発動によってチェンジする判定
	private void JudgeModeChange()
	{
		switch (CharaMode)
		{
			case Mode.ANIMAL:
				if (CharaState == State.DEAD) return;   //死亡なら以下を通さない
				timer += Time.deltaTime;
				if (timer >= 1f) { ReasonHeal(); timer = 0f; } //通常時にのみ理性回復
				break;

			// 理性開放時の理性ゲージ減少処理関数呼び出し
			case Mode.SPSIAL_ANIMAL:
				timer += Time.deltaTime;
				if (timer >= 1f) { ReasonDecrease(); timer = 0f; }
				break;
		}
	}

	// --- 倍率設定用の関数 ---
	private void SetMultiplierByAnimal(bool isReasoning)
	{
		// 通常モードに戻る時は全員 1.0f
		if (!isReasoning)
		{
			currentAtkMult = CharacterData.INITIAL_MAGNIFICATION;
			currentSpdMult = CharacterData.INITIAL_MAGNIFICATION;
			currentDefMult = CharacterData.INITIAL_MAGNIFICATION;
			return;
		}

		// 現在選択されている動物のデータを取得して倍率を適用
		animalP currentParam = GetCurrentAnimalParam();
		currentAtkMult = currentParam.reasonAtkMult;
		currentDefMult = currentParam.reasonDefMult;
		currentSpdMult = currentParam.reasonSpdMult;
	}

	// 現在の動物のパラメーターを返す関数
	private animalP GetCurrentAnimalParam()
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
		if (CharaMode == Mode.ANIMAL) DamageCalculation(damage);
		else if (CharaMode == Mode.SPSIAL_ANIMAL) ReasonDamageCalculation(damage);

		UpdateUI(); // UIの更新関数呼び出し

		//死亡直前の特性チェック(ex.ラーテルのピンチで耐えて発動する等
		if (CurrentHP <= 0)
		{
			Animal_Skill_TraitBase trait = GetComponent<Animal_Skill_TraitBase>();
			if (trait != null && trait.OnFatalDamage()) { return; }
		}
		// 最終死亡判定
		if (CurrentHP <= 0 || CurrentReason <= 0) { Die(); }
	}

	//防御力取得関数
	public int GetDefensePower() { return DefensePower; }

	//モード取得
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

				Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();
				if (animl_skill != null) skillComponent.Characteristic();

				// --- デバッグログ：上昇前後の比較を表示 ---
				Debug.Log($"<color=red>【理性解放】 {CharaAnim}</color>\n" +
						  $"攻撃力: {AttackPower} ➔ {CurrentAttackPower} ({currentAtkMult}倍)\n" +
						  $"防御力: {DefensePower} ➔ {CurrentDefensePower} ({currentDefMult}倍)\n" +
						  $"移動速度: {MoveSpeed} ➔ {CurrentMoveSpeed} ({currentSpdMult}倍)");

				animator.SetBool("Reason", true); Debug.Log("理性解放！！");
				break;

			case Mode.SPSIAL_ANIMAL:
				CharaMode = Mode.ANIMAL;
				SetMultiplierByAnimal(false);       // 倍率リセット関数呼び出し

				Debug.Log($"<color=cyan>【通常モードに戻りました】</color>\n" +
					  $"ステータスがベース値（攻撃:{CurrentAttackPower}, 速度:{CurrentMoveSpeed}）に復旧");

				animator.SetBool("Reason", false); Debug.Log("通常");
				break;
		}
	}

	//死亡処理関数
	protected virtual void Die()
	{
		//もし理性解放中に死亡したなら現在HP/通常なら理性を0に設定する
		if (CharaMode == Mode.SPSIAL_ANIMAL) SetHP(0);
		else if (CharaMode == Mode.ANIMAL) SetReason(0);

		if (hp_gauge != null) hp_gauge.value = 0;
		if (reason_gauge != null) reason_gauge.value = 0;

		//体力ゲージ・理性ゲージのUIを非表示にする処理
		if (hp_gauge != null) hp_gauge.gameObject.SetActive(false);
		if (reason_gauge != null) reason_gauge.gameObject.SetActive(false);

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

	// スキル実行
	public virtual void Skill()
	{
		Animal_Skill_TraitBase skillComponent = GetComponent<Animal_Skill_TraitBase>();
		if (skillComponent != null) skillComponent.Skill();
	}

	/*******回復処理*********/
	//指定した値分の回復
	public void HealHP(int amount)
	{
		CurrentHP += Mathf.Clamp(amount, 0, MaxHP);
		if (CurrentHP > MaxHP) SetHP(MaxHP);
	}
	public void HealReason(int amount)
	{
		CurrentReason += amount;
		if (CurrentReason > MaxReason) SetReason(MaxReason);
	}

	//主に体力・理性ゲージの回復に使用する上限付の関数
	public void ItemHeal(int hp, int reason)
	{
		CurrentHP = Mathf.Clamp(hp, 0, MaxHP);
		CurrentReason = Mathf.Clamp(reason, 0, MaxReason);
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

	/***********ダメージ関連****************/
	private void DamageCalculation(int damage)
	{
		// 攻撃力の 20% は防御を無視して必ず通る
		int actualDamage = Mathf.Max(damage - CurrentDefensePower, (int)(damage * 0.2f));
		CurrentHP -= actualDamage; // HP減少処理

		// 今いくら防いだか
		Debug.Log($"<color=yellow>【被弾】 元ダメ:{damage} -> 防御後:" +
		$"{actualDamage} (現在の防御力:{CurrentDefensePower})</color>");

		Animal_Skill_TraitBase trait = GetComponent<Animal_Skill_TraitBase>();
		if (trait != null) { trait.OnCharacterTakeDamage(actualDamage); }
	}

	//理性ゲージでのダメージ計算（防御力を考慮）
	private void ReasonDamageCalculation(int reason_damage)
	{
		int actualDamage = Mathf.Max(reason_damage - CurrentDefensePower, 1);
		CurrentReason -= actualDamage;                   // 理性ゲージ減少処理
		CurrentHP -= (int)((float)reason_damage * 0.1f); // HP減少処理

		Debug.Log($"<color=yellow>【被弾】 元ダメ:{reason_damage} -> 防御後:" +
		$"{actualDamage} (現在の防御力:{CurrentDefensePower})</color>"); // 今いくら防いだか
	}

	//理性解放状態時:理性ゲージ減少処理関数
	protected virtual void ReasonDecrease()
	{
		if (playerManager.isGameEnd) return;

		//もし理性が0より大きいなら理性ゲージを減少させる
		if (CurrentReason > 0)
		{
			//最大理性ポイントに減少率をかけて減少量を計算し、最低でも1は減少するようにする
			int decreaseAmount = Mathf.Max((int)(MaxReason * CharacterData.REASON_DECREASE_RATE), 1);
			ReducedReasoning(decreaseAmount);    // 理性ゲージ減少処理
			Debug.Log($"{CharaAnim}の理性減少中: 残り{CurrentReason} (毎秒{decreaseAmount}減)");
		}
		//0以下なら理性ゲージを0・死亡処理を行う
		else { animator.SetBool("Reason_Dead", true); Die(); }
	}

	// 現在の理性ゲージに引数分引く
	public void ReducedReasoning(int amount)
	{
		if (playerManager.isGameEnd) return;
		CurrentReason -= amount;
	}

	/*割合ダメージ計算関数
	 max     :最大体力
	 raito   :割合
	 使用箇所:デッドゾーンに出たときの処理
	 */
	private void PercentageDamage(int max, float ratio)
	{ CurrentHP -= (int)((float)max * ratio); }

	//外部から呼び出すための関数
	public void ForceDie() { Die(); }

	// 奈落に落ちていった時に呼ばれる
	public void DieAbyss() { this.Die(); }

	// 範囲外に出た時に呼ばれる
	public void OutOfRangeDamage()
	{
		if (playerManager.isGameEnd) return;  //ゲーム終了が確定したら死なせない
		PercentageDamage(MaxHP, CharacterData.OFF_SITE_RAITO);
		if (CurrentHP <= 0) this.Die();     // 死亡判定
	}
}