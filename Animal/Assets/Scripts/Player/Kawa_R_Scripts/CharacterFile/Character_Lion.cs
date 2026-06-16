using UnityEngine;
using UnityEngine.UI;

public class Character_Lion : Animal_Skill_TraitBase
{
	// 定数
	private const float LION_SKILL_DURATION = 5.0f; // 咆哮バフの持続時間
	private const float LION_CT = 15.0f;            // スキルのCT(咆哮)※爆発力が高いので長め

	// ライオン専用のスキルバフ変数
	private float lionSkillAtkBoost = 1.0f;
	private float lionSkillDurationTimer = 0f;

	public override float CurrentAtkBoost => lionSkillAtkBoost;

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
				SetLionAtkBoost(1.0f);
				AnimalDebugLog("white", "咆哮の効果が終了した");
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
				LION_SKILL_DURATION,
				status.buffContainer
			);
		}

		// モードチェック
		if (status != null && status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL)
			SetLionAtkBoost(1.7f); // 解放中は1.7倍
		else
			SetLionAtkBoost(1.3f); // 通常時は1.3倍

		lionSkillDurationTimer = LION_SKILL_DURATION; // 5秒間持続
		SetSkillCooldownTimer(LION_CT);
	}

	// ライオンがスキルを使用時に攻撃値が変化する
	private void SetLionAtkBoost(float boost_amount){ lionSkillAtkBoost =  boost_amount; }
}