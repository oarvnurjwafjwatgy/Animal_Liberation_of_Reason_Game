using UnityEngine;

public class Character_HoneyBadger : Animal_Skill_TraitBase
{
	// ラーテル専用：くいしばり特性のフラグ
	private bool hasTriggeredGuts = false;
	private const float RATEL_CT = 12.0f;    // ラーテルのスキルCoolTime

	//スキル発動処理(継承)
	public override void Skill()
	{
		base.Skill(); // 共通の死亡チェックなどを実行

		if (skillCooldownTimer > 0) return; // CT中なら発動不可
		AnimalDebugLog("yellow", "スキル発動");
		SetSkillCooldownTimer(RATEL_CT);
	}

	// ラーテルの特性
	public override bool OnFatalDamage()
	{
		if (hasTriggeredGuts) return false;
		hasTriggeredGuts = true;
		status.SetHP((int)(status.MaxHP * 0.25f));  //最大HP25％まで回復
		status.HealReason(status.MaxReason);        //理性ゲージは最大まで回復
		status.GetModeChange();                     // 強制的に理性解放へと変化
		AnimalDebugLog("red", " 特性発動:致命傷を耐え理性解放");

		//モデルを変更するためにInputPlayerに参照
		InputPlayer player = GetComponent<InputPlayer>();
		if (player != null) player.Enhancement();
		return true;
	}
}
