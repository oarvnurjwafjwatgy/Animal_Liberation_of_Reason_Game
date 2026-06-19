using UnityEngine;
using P = LionSkillParam;
using UnityEngine.UI;

public class Character_Lion : Animal_Skill_TraitBase
{
	// ライオン専用のスキルバフ変数
	private float lionSkillAtkBoost = P.LION_RESET_VALUE;
	private float lionSkillDurationTimer = AnimalParam.TIMER_RESET;

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
				SetAtkBoost(ref lionSkillAtkBoost, P.LION_RESET_VALUE);
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
}