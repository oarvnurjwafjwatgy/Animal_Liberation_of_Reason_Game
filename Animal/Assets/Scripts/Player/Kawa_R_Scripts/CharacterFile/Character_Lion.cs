using UnityEngine;
using UnityEngine.UI;
using P = LionSkillParam;
using Istic = LionCharacterIsticParameter;


public class Character_Lion : Animal_Skill_TraitBase
{
	// 特性用（蓄積ダメージ・憤怒バースト）の変数をこちらに移行
	private Image lionRageFill;             // 外周ゲージUI
	private GameObject lionRageUIRoot;      // アイコンUIルート
	private int accumulatedDamage = 0;      // 蓄積ダメージ

	
	private float lionBurstSpeedBoost = P.LION_RESET_VALUE;		// 特性による速度倍率
	private float lionBurstAtkBoost = P.LION_RESET_VALUE;		// 特性による攻撃倍率
	private float lionBurstTimer = AnimalParam.TIMER_RESET;		// 特性の残り時間タイマー
	
	// 特性用の定数
	private const int   BURST_THRESHOLD = 80;               // 発動しきい値
	private const float LION_BURST_DURATION = 8f;			// バースト持続時間
	private const float BURST_BASE_SPEED_BOOST = 1.25f;     // バースト時の基本速度上昇率
	private const float BURST_MAX_EXTRA_SPEED = 0.15f;      // 蓄積ダメージによる追加速度の上限値
	private const float BURST_ATK_BOOST = 1.15f;            // バースト時の攻撃上昇率
	private const float DAMAGE_TO_SPEED_SCALE = 150f;       // ダメージを速度倍率に変換する際の割る数
	
	// ライオン専用のスキルバフ変数
	private float lionSkillAtkBoost = P.LION_RESET_VALUE;
	private float lionSkillDurationTimer = AnimalParam.TIMER_RESET;

	public override float CurrentAtkBoost => lionSkillAtkBoost * lionBurstAtkBoost;
	public override float CurrentSpeedBoost => lionBurstSpeedBoost;

	protected override void Start()
	{
		base.Start();

		if (status != null && status.MyUIManager != null)
		{
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
		UpdateSkillTimer();   // 咆哮スキルのタイマー更新
		UpdateBurstTimer();   // 憤怒バーストのタイマー更新
		UpdateRageUI();       // 外周ゲージUIの表示更新
	}

	// 咆哮スキルのタイマー管理
	private void UpdateSkillTimer()
	{
		if (lionSkillDurationTimer <= 0) return;
		lionSkillDurationTimer -= Time.deltaTime;

		if (lionSkillDurationTimer <= 0)
		{
			SetAtkBoost(ref lionSkillAtkBoost, P.LION_RESET_VALUE);
			AnimalDebugLog("white", "咆哮の効果が終了した");
		}
	}

	// 憤怒バーストのタイマー管理
	private void UpdateBurstTimer()
	{
		if (lionBurstTimer <= 0) return;

		lionBurstTimer -= Time.deltaTime;
		if (lionBurstTimer <= 0)
		{
			lionBurstSpeedBoost = P.LION_RESET_VALUE;
			lionBurstAtkBoost = P.LION_RESET_VALUE;
			AnimalDebugLog("white", "ライオン：憤怒のバフが終了した");
		}
	}

	// 外周ゲージUIの表示更新
	private void UpdateRageUI()
	{
		if (lionRageFill == null) return;

		if (lionBurstTimer > 0)
		{
			lionRageFill.fillAmount = lionBurstTimer / LION_BURST_DURATION;
			lionRageFill.color = Color.red;
		}
		else
		{
			float ratio = (float)accumulatedDamage / BURST_THRESHOLD;
			lionRageFill.fillAmount = Mathf.Clamp01(ratio);
			lionRageFill.color = (ratio >= 1f) ? new Color(1f, 0.5f, 0f) : Color.yellow;
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

	// 被弾時に Character_Status から呼び出されてダメージを溜める
	public override void OnCharacterTakeDamage(int actualDamage)
	{
		if (status == null || status.GetMode() != Character_Status.Mode.ANIMAL) return;

		accumulatedDamage += actualDamage;
		AnimalDebugLog("yellow", $"ライオン：ダメージ蓄積中（現在：{accumulatedDamage} / しきい値：{BURST_THRESHOLD}）");
	}

	// 理性解放した瞬間（本体のGetModeChange時）に呼び出される特性確定処理
	public override void Characteristic()
	{
		base.Characteristic();

		if (accumulatedDamage >= BURST_THRESHOLD)
		{
			// 蓄積量に応じて強化幅を変える（最大1.4倍、攻撃1.15倍など）
			float extraPower = (float)(accumulatedDamage - BURST_THRESHOLD) / DAMAGE_TO_SPEED_SCALE;
			lionBurstAtkBoost = BURST_BASE_SPEED_BOOST + Mathf.Min(extraPower, BURST_MAX_EXTRA_SPEED);
			lionBurstAtkBoost = BURST_ATK_BOOST;
			lionBurstTimer = LION_BURST_DURATION; // バーストタイマー開始

			if (status != null && status.MyUIManager != null && status.buffContainer != null)
			{
				status.MyUIManager.CreateOrUpdateBuffUI(status.playerID, Character_Status.BuffType.SpeedBuff, LION_BURST_DURATION, status.buffContainer);
			}
			AnimalDebugLog("red", $"【特性発動】憤怒解放！ {LION_BURST_DURATION}秒間爆速！");
		}
		else
		{
			lionBurstSpeedBoost = P.LION_RESET_VALUE;
			lionBurstAtkBoost = P.LION_RESET_VALUE;
			lionBurstTimer = AnimalParam.TIMER_RESET;
			AnimalDebugLog("white", $"蓄積不足({accumulatedDamage}/{BURST_THRESHOLD})のため特性は不発");
		}
		accumulatedDamage = 0;  // 成否に関わらず蓄積はリセット
	}
}