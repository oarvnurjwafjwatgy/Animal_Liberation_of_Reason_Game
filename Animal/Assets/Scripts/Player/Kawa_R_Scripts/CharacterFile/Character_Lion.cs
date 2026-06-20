using UnityEngine;
using P = LionSkillParam;
using UnityEngine.UI;

public class Character_Lion : Animal_Skill_TraitBase
{
	// 特性用（蓄積ダメージ・憤怒バースト）の変数をこちらに移行
	private Image lionRageFill;             // 外周ゲージUI
	private GameObject lionRageUIRoot;      // アイコンUIルート
	private int accumulatedDamage = 0;      // 蓄積ダメージ

	[SerializeField] private int burstThreshold = 80;       // 発動しきい値
	[SerializeField] private float lionBurstDuration = 8f;   // バースト持続時間
	private float lionBurstSpeedBoost = 1.0f;               // 特性による速度倍率
	private float lionBurstAtkBoost = 1.0f;                 // 特性による攻撃倍率
	private float lionBurstTimer = 0f;                      // 特性の残り時間タイマー

	// ライオン専用のスキルバフ変数
	private float lionSkillAtkBoost = P.LION_RESET_VALUE;
	private float lionSkillDurationTimer = AnimalParam.TIMER_RESET;

	//public override float CurrentAtkBoost => lionSkillAtkBoost;
	public override float CurrentAtkBoost => lionSkillAtkBoost * lionBurstAtkBoost;
	public override float CurrentSpeedBoost => lionBurstSpeedBoost;

	protected override void Start()
	{
		base.Start();

		if (status != null && status.MyUIManager != null)
		{
			// Character_Lion.cs 内
			status.MyUIManager.CreateUI(UIManager.UI_ID.LION_RAGE, status.UiPos, status.playerID);

			if (status.MyUIManager.ui_list != null && status.MyUIManager.ui_list.Count > 0)
			{
				lionRageUIRoot = status.MyUIManager.ui_list[status.MyUIManager.ui_list.Count - 1];
				Transform gaugeTrans = lionRageUIRoot.transform.Find("Gauge");
				if (gaugeTrans != null)
				{
					lionRageFill = gaugeTrans.GetComponent<Image>();
				}
			}
		}
	}

	//更新
	protected override void Update()
	{
		base.Update(); // 親クラスの共通CTカウントダウン（skillCooldownTimer）を実行

		// ライオン独自の咆哮バフのカウントダウン
		if (lionSkillDurationTimer > 0)
		{
			lionSkillDurationTimer -= Time.deltaTime;
			if (lionSkillDurationTimer <= 0)
			{
				SetAtkBoost(ref lionSkillAtkBoost, P.LION_RESET_VALUE);
				AnimalDebugLog("white", "咆哮の効果が終了した");
			}
		}


		// 特性（憤怒バースト）のカウントダウン
		if (lionBurstTimer > 0)
		{
			lionBurstTimer -= Time.deltaTime;
			if (lionBurstTimer <= 0)
			{
				lionBurstSpeedBoost = 1.0f;
				lionBurstAtkBoost = 1.0f;
				AnimalDebugLog("white", "ライオン：憤怒のバフが終了した");
			}
		}

		// 外周ゲージUIの表示更新
		if (lionRageFill != null)
		{
			if (lionBurstTimer > 0)
			{
				lionRageFill.fillAmount = lionBurstTimer / lionBurstDuration;
				lionRageFill.color = Color.red;
			}
			else
			{
				float ratio = (float)accumulatedDamage / burstThreshold;
				lionRageFill.fillAmount = Mathf.Clamp01(ratio);
				lionRageFill.color = (ratio >= 1f) ? new Color(1f, 0.5f, 0f) : Color.yellow;
			}
		}
	}

	// ライオンの専用スキル処理
	public override void Skill()
	{
		AnimalDebugLog("yellow", "スキル発動");

		if (skillCooldownTimer > 0) return;

		// UIの処理
		if (status != null && status.MyUIManager != null && status.buffContainer != null)
		{
			status.MyUIManager.CreateOrUpdateBuffUI(
				status.playerID,
				Character_Status.BuffType.AttackBuff, // Character_Status. を挟む
				P.LION_SKILL_DURATION,  //CTをセット
				status.buffContainer
			);
		}

		// モードチェック
		if (IsSpecialAnimal) SetAtkBoost(ref lionSkillAtkBoost, P.REASON_SKILL_UP_VALUE); // 解放中:1.7倍
		else SetAtkBoost(ref lionSkillAtkBoost, P.SKILL_UP_VALUE);  //通常:1.3倍

		lionSkillDurationTimer = P.LION_SKILL_DURATION; // 5秒間持続
		SetSkillCooldownTimer(P.LION_CT);
	}

	//// ライオンがスキルを使用時に攻撃値が変化する
	//protected override void SetAtkBoost(ref float animl_atk, float boost_amount)
	//{
	//	base.SetAtkBoost(ref animl_atk, boost_amount);
	//}

	// 被弾時に Character_Status から呼び出されてダメージを溜める
	public override void OnCharacterTakeDamage(int actualDamage)
	{
		if (status == null || status.GetMode() != Character_Status.Mode.ANIMAL) return;

		accumulatedDamage += actualDamage;
		AnimalDebugLog("yellow", $"ライオン：ダメージ蓄積中（現在：{accumulatedDamage} / しきい値：{burstThreshold}）");
	}

	// 理性解放した瞬間（本体のGetModeChange時）に呼び出される特性確定処理
	public override void Characteristic()
	{
		base.Characteristic();

		if (accumulatedDamage >= burstThreshold)
		{
			// 蓄積量に応じて強化幅を変える（最大1.4倍、攻撃1.15倍など）
			float extraPower = (float)(accumulatedDamage - burstThreshold) / 150f;
			lionBurstSpeedBoost = 1.25f + Mathf.Min(extraPower, 0.15f);
			lionBurstAtkBoost = 1.15f;

			lionBurstTimer = lionBurstDuration; // バーストタイマー開始

			if (status != null && status.MyUIManager != null && status.buffContainer != null)
			{
				status.MyUIManager.CreateOrUpdateBuffUI(status.playerID, Character_Status.BuffType.SpeedBuff, lionBurstDuration, status.buffContainer);
			}

			AnimalDebugLog("red", $"【特性発動】憤怒解放！ {lionBurstDuration}秒間爆速！");
		}
		else
		{
			lionBurstSpeedBoost = 1.0f;
			lionBurstAtkBoost = 1.0f;
			lionBurstTimer = 0f;
			AnimalDebugLog("white", $"蓄積不足({accumulatedDamage}/{burstThreshold})のため特性は不発");
		}

		// 成否に関わらず蓄積はリセット
		accumulatedDamage = 0;
	}
}