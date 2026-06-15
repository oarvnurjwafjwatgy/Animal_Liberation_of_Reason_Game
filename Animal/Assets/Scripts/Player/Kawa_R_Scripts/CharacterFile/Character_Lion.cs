using UnityEngine;
using UnityEngine.UI;

public class Character_Lion : Animal_Skill_TraitBase
{
	// 定数
	private const float LION_SKILL_DURATION = 5.0f; // 咆哮バフの持続時間
	private const float LION_CT = 15.0f;            // スキルのCT(咆哮)

	// ライオン専用のスキルバフ変数
	private float lionSkillAtkBoost = 1.0f;
	private float lionSkillDurationTimer = 0f;

	//public float LionSkillAtkBoost => lionSkillAtkBoost;
	public override float CurrentAtkBoost => lionSkillAtkBoost;

	protected override void Update()
	{
		base.Update(); // 親クラスの共通CTカウントダウン（skillCooldownTimer）を実行

		// ライオン独自の咆哮バフのカウントダウン
		if (lionSkillDurationTimer > 0)
		{
			lionSkillDurationTimer -= Time.deltaTime;
			if (lionSkillDurationTimer <= 0)
			{
				lionSkillAtkBoost = 1.0f;
				Debug.Log("<color=white>ライオン：咆哮の効果が終了した</color>");
			}
		}
	}

	// ライオンの専用スキル処理
	public override void Skill()
	{
		Debug.Log("スキル発動");

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
			lionSkillAtkBoost = 1.7f; // 解放中は1.7倍
		else
			lionSkillAtkBoost = 1.3f; // 通常時は1.3倍

		lionSkillDurationTimer = LION_SKILL_DURATION; // 5秒間持続
		skillCooldownTimer = LION_CT; // 親から貰ったCTタイマーに直接セット！
	}
}