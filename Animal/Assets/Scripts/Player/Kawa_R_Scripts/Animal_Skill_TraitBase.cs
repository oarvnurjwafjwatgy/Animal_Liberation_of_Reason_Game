using UnityEngine;

public class Animal_Skill_TraitBase : MonoBehaviour
{
	protected Character_Status status; // 本体のステータスへの参照

	[Header("共通スキル設定")]
	[SerializeField] protected float skillCooldownTimer = 0f; // 現在のCT
	[SerializeField] protected float skillCTMax = 10f;        // スキルの最大CT

	// 外部（本体のCharacter_Statusなど）から、現在のCTを安全に覗き見（読み取り）するためのプロパティ
	public float SkillCooldownTimer => skillCooldownTimer;

	public virtual float CurrentAtkBoost => 1.0f;

	protected virtual void Start()
	{
		// 同じオブジェクト（Player本体）についているステータスを取得しておく
		status = GetComponent<Character_Status>();

		if (status == null)
			Debug.LogError("Player本体に Character_Status が見つかりません！");
	}

	// 共通更新
	protected virtual void Update()
	{
		if (skillCooldownTimer > 0) skillCooldownTimer -= Time.deltaTime;	//CoolTime
	}

	// 固有特性用（ virtual で子クラスに上書きさせる ）
	public virtual void Characteristic() { }

	// 固有スキル用（ virtual で子クラスに上書きさせる ）
	public virtual void Skill() { }
}