using UnityEngine;

public class Animal_Skill_TraitBase : MonoBehaviour
{
	//開発者用の設定
	private bool debugMode = true; // デバックの全体スイッチ

	[Header("共通スキル設定")]
	[SerializeField] protected float skillCooldownTimer = 0f; // 現在のCT
	[SerializeField] protected float skillCTMax = 10f;        // スキルの最大CT

	protected Character_Status status; // 本体のステータスへの参照

	// 外部（本体のCharacter_Statusなど）から、現在のCTを安全に覗き見（読み取り）するためのプロパティ
	public float SkillCooldownTimer => skillCooldownTimer;

	public virtual float CurrentAtkBoost => 1.0f;

	public virtual float CurrentSpeedBoost => 1.0f;

	protected virtual void Start()
	{
		// 同じオブジェクト（Player本体）についているステータスを取得しておく
		status = GetComponent<Character_Status>();

		if (status == null) Debug.LogError("Player本体に Character_Status が見つかりません！");
	}

	// 共通更新
	protected virtual void Update()
	{
		if (skillCooldownTimer > 0) skillCooldownTimer -= Time.deltaTime;   //共通クールタイム
	}

	// 固有特性用（ virtual で子クラスに上書きさせる ）
	public virtual void Characteristic() { }

	// 固有スキル用（ virtual で子クラスに上書きさせる ）
	public virtual void Skill() { }

	// 特殊な特性発動に使用する関数
	public virtual bool OnFatalDamage() { return false; }

	// クールタイムをセットする関数
	public virtual void SetSkillCooldownTimer(float timer) { skillCooldownTimer = timer; }

	// CharacterStatusのReducedReasoning関数の処理を呼び出す。
	protected void ReducedReasoning(int amount) { status.ReducedReasoning(amount); }

	// CharacterStatusにある死亡処理を呼ぶ
	protected virtual void Die() { status.ForceDie(); }

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