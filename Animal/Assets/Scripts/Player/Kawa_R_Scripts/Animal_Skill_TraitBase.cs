using UnityEngine;

public class Animal_Skill_TraitBase : MonoBehaviour
{
	//開発者用の設定
	private bool debugMode = true; // デバックの全体スイッチ

	[Header("共通スキル設定")]
	[SerializeField] protected float skillCooldownTimer = 0f; // 現在のCT
	[SerializeField] protected float skillCTMax = 10f;        // スキルの最大CT

	protected Character_Status status; // 本体のステータスへの参照
	protected PlayerManager playerManager;

	// 外部（本体のCharacter_Statusなど）から、現在のCTを安全に覗き見（読み取り）するためのプロパティ
	public float SkillCooldownTimer => skillCooldownTimer;

	public virtual float CurrentAtkBoost => 1.0f;

	public virtual float CurrentSpeedBoost => 1.0f;

	// if文のチェックで使用する(理性開放かどうか)
	protected bool IsSpecialAnimal => status != null && status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL;

	protected virtual void Start()
	{
		// 同じオブジェクト（Player本体）についているステータスを取得しておく
		status = GetComponent<Character_Status>();
		playerManager = GetComponent<PlayerManager>();
		if (status == null) Debug.LogError("Player本体に Character_Status が見つかりません！");
	}

	// 共通更新
	protected virtual void Update()
	{
		if (skillCooldownTimer > 0) skillCooldownTimer -= Time.deltaTime;   //共通クールタイム
	}

	// 固有特性
	public virtual void Characteristic() { }

	// 固有スキル
	public virtual void Skill() { }

	// 特殊な特性発動に使用する関数
	public virtual bool OnFatalDamage() { return false; }

	// クールタイムをセットする関数
	public virtual void SetSkillCooldownTimer(float timer) { skillCooldownTimer = timer; }

	/* スキル発動時一時的に攻撃が上昇した後、数秒後に戻す関数
	 delay:指定した秒数（0.2秒など）だけ待つ*/
	protected virtual System.Collections.IEnumerator ResetAtkBoostAfterDelay(float delay)
	{ yield return new WaitForSeconds(delay); }

	// CharacterStatusのReducedReasoning関数の処理を呼び出す。
	protected void ReducedReasoning(int amount) { status.ReducedReasoning(amount); }

	// CharacterStatusにある死亡処理を呼ぶ
	protected virtual void Die() { status.ForceDie(); }


	/*スキル使用時に通常攻撃の上乗せで攻撃力を作る関数(refで直接変更可能)
	animl_atk   :各動物のAttackBoostを入れる。
	boost_amount:変化した火力に変化させる(ex.理性開放後のスキル火力を反映*/
	protected virtual void SetAtkBoost(ref float animl_atk, float boost_amount) { animl_atk = boost_amount; }

	
	// 通常被弾時にCharacter_Statusからダメージ通知を受け取るための仮想関数
	public virtual void OnCharacterTakeDamage(int actualDamage) { }


	//動物のデバック用関数
	protected virtual void AnimalDebugLog(
	string color,
	string message,
	bool localDebug = true)
	{
		if (!debugMode) return;
		if (!localDebug) return;
		Debug.Log($"<color={color}>[{GetType().Name}] {message}</color>");
	}
}