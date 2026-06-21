using UnityEngine;
using UnityEngine.UI;
using P = LionSkillParam;

public class Character_Lion : Animal_Skill_TraitBase
{
	// 特性用（蓄積ダメージ・憤怒バースト
	private Image lionRageFill;             // 外周ゲージUI
	private GameObject lionRageUIRoot;      // アイコンUIルート

	// スキルバフ変数
	private float lionSkillAtkBoost = P.LION_RESET_VALUE;
	private float lionSkillDurationTimer = AnimalParam.TIMER_RESET;

	// 特性の変数
	private float lionBurstSpeedBoost = P.LION_RESET_VALUE;     // 特性による速度倍率
	private float lionBurstAtkBoost = P.LION_RESET_VALUE;       // 特性による攻撃倍率
	private float lionBurstTimer = AnimalParam.TIMER_RESET;     // 特性の残り時間タイマー
	private float uiVisibleTimer = AnimalParam.TIMER_RESET;		// UIを表示し続けるタイマー
	private int accumulatedDamage = AnimalParam.INITIAL_VALUE;  // 蓄積ダメージ

	public override float CurrentAtkBoost => lionSkillAtkBoost * lionBurstAtkBoost;
	public override float CurrentSpeedBoost => lionBurstSpeedBoost;

	public override float MaxSkillCooldown => P.LION_CT;	// CTを入れる。

	//初期
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
				if (gaugeTrans != null) { lionRageFill = gaugeTrans.GetComponent<Image>(); }
			}
		}
	}

	//更新
	protected override void Update()
	{
		if (status != null && status.IsDead)
		{ lionRageUIRoot.SetActive(false); return; }
		base.Update();        // 親クラスの共通CTカウントダウンを実行
		UpdateSkillTimer();   // 咆哮スキルのタイマー更新
		UpdateBurstTimer();   // 憤怒バーストのタイマー更新

		if (uiVisibleTimer > 0) { uiVisibleTimer -= Time.deltaTime; }
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

		bool isBursting = lionBurstTimer > 0;                          // ①特性発動中（バースト中）
		bool isGaugeMax = accumulatedDamage >= P.BURST_THRESHOLD;      // ②ゲージが満タン
		bool isRecentlyDamaged = uiVisibleTimer > 0;                   // ③最近ダメージを喰らった（3秒以内）

		// いずれかの条件を満たしていればUIを表示、そうでなければ非表示にする
		if (isBursting || isGaugeMax || isRecentlyDamaged)
		{
			lionRageUIRoot.SetActive(true);

			if (lionBurstTimer > 0)
			{
				lionRageFill.fillAmount = lionBurstTimer / P.LION_BURST_DURATION;
				lionRageFill.color = Color.red;
				SetUIRootAlpha(1.0f);// バースト中は点滅させず、完全に不透明（Alpha = 1）にする
			}
			else
			{
				float ratio = (float)accumulatedDamage / P.BURST_THRESHOLD;
				lionRageFill.fillAmount = Mathf.Clamp01(ratio);
				lionRageFill.color = (ratio >= 1f) ? new Color(1f, 0.5f, 0f) : Color.yellow;
			}
		}
		else lionRageUIRoot.SetActive(false);   //条件をどれも満たさないのなら画面から削除。
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
				Character_Status.BuffType.AttackBuff, // Character_Statusを挟む
				P.LION_SKILL_DURATION,  //CTセット
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
		uiVisibleTimer = P.UI_VISIBLE_DURATION;// 被弾。UI表示タイマーを3秒セット&表示
		AnimalDebugLog("yellow", $"ライオン：ダメージ蓄積中（現在：{accumulatedDamage} / しきい値：{P.BURST_THRESHOLD}）");
	}

	// 理性解放した瞬間（本体のGetModeChange時）に呼び出される特性確定処理
	public override void Characteristic()
	{
		base.Characteristic();

		if (accumulatedDamage >= P.BURST_THRESHOLD)
		{
			// 蓄積量に応じて強化幅を変える（最大1.4倍、攻撃1.15倍など）
			float extraPower = (float)(accumulatedDamage - P.BURST_THRESHOLD) / P.DAMAGE_TO_SPEED_SCALE;
			lionBurstAtkBoost = P.BURST_BASE_SPEED_BOOST + Mathf.Min(extraPower, P.BURST_MAX_EXTRA_SPEED);
			lionBurstAtkBoost = P.BURST_ATK_BOOST;
			lionBurstTimer = P.LION_BURST_DURATION; // バーストタイマー開始

			if (status != null && status.MyUIManager != null && status.buffContainer != null)
			{
				status.MyUIManager.CreateOrUpdateBuffUI(status.playerID, Character_Status.BuffType.SpeedBuff, P.LION_BURST_DURATION, status.buffContainer);
			}
			AnimalDebugLog("red", $"【特性発動】憤怒解放！ {P.LION_BURST_DURATION}秒間爆速！");
		}
		else
		{
			lionBurstSpeedBoost = P.LION_RESET_VALUE;
			lionBurstAtkBoost = P.LION_RESET_VALUE;
			lionBurstTimer = AnimalParam.TIMER_RESET;
			AnimalDebugLog("white", $"蓄積不足({accumulatedDamage}/{P.BURST_THRESHOLD})のため特性は不発");
		}
		accumulatedDamage = AnimalParam.INITIAL_VALUE;  // 成否に関わらず蓄積はリセット
	}


	// UI全体の透明度を一発で変更するヘルパー関数
	private void SetUIRootAlpha(float alpha)
	{
		if (lionRageUIRoot == null) return;

		// UIの親オブジェクトに CanvasGroup がついていればそれを使う
		CanvasGroup cg = lionRageUIRoot.GetComponent<CanvasGroup>();
		if (cg != null) cg.alpha = alpha;
		else
		{
			// CanvasGroupがない場合は、とりあえずImageのColorからAlphaを直接いじる
			Image rootImage = lionRageUIRoot.GetComponent<Image>();
			if (rootImage != null)
			{
				Color c = rootImage.color;
				c.a = alpha;
				rootImage.color = c;
			}
		}

	}
}