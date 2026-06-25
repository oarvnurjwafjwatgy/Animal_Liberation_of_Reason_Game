using UnityEngine;
using P = HoneyBadgerSkillParam;

public class Character_HoneyBadger : Animal_Skill_TraitBase
{
//変数
	// ラーテル専用：くいしばり特性のフラグ
	private bool hasTriggeredGuts = false;
	private float honeyBadgerSkillAtkBoost = P.HONEYBADGER_RESET_VALUE;  //初期値:スキルの攻撃力(通常から上乗せ)
	public override float CurrentAtkBoost => honeyBadgerSkillAtkBoost;
	public override float MaxSkillCooldown => P.HONEYBADGER_CT;

	//スキル発動処理(継承)
	public override void Skill()
	{
		if (IsSpecialAnimal) SetAtkBoost(ref honeyBadgerSkillAtkBoost, P.REASON_SKILL_UP_VALUE);
		else SetAtkBoost(ref honeyBadgerSkillAtkBoost, P.SKILL_UP_VALUE);

		base.Skill();   //共通の死亡チェックを実行
		AnimalDebugLog("yellow", "固有スキルが発動した");
		StartCoroutine(ResetAtkBoostAfterDelay(P.RESET_DELAY)); // 0.2秒後に自動で等倍に戻すコルーチン
		SetSkillCooldownTimer(P.HONEYBADGER_CT);
	}

	// ラーテルの特性
	public override bool OnFatalDamage()
	{
		if (hasTriggeredGuts) return false;
		hasTriggeredGuts = true;
		status.SetHP((int)(status.MaxHP * P.TRAIT_PERCENTAGE_RECOVERY));  //最大HP25％まで回復
		status.HealReason(status.MaxReason);        //理性ゲージは最大まで回復
		status.GetModeChange();                     // 強制的に理性解放へと変化
		AnimalDebugLog("red", " 特性発動:致命傷を耐え理性解放");

		//モデルを変更するためにInputPlayerに参照
		InputPlayer player = GetComponent<InputPlayer>();
		if (player != null) player.Enhancement();
		return true;
	}


	//少し処理を遅らせてから攻撃値を元に戻す処理
	protected override System.Collections.IEnumerator ResetAtkBoostAfterDelay(float delay)
	{
		yield return base.ResetAtkBoostAfterDelay(delay);
		SetAtkBoost(ref honeyBadgerSkillAtkBoost, P.HONEYBADGER_RESET_VALUE); // ここで1.0倍に戻す
		AnimalDebugLog("white", "ラーテルのスキル倍率が元に戻りました");
	}
}
