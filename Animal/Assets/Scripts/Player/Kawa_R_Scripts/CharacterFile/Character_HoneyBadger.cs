using UnityEngine;

public class Character_HoneyBadger : Animal_Skill_TraitBase
{
	// ラーテル専用：くいしばり特性のフラグ
	private bool hasTriggeredGuts = false;

	public bool CanUseGats(){ return hasTriggeredGuts; }

	//ラーテルの特性関数
	public void TriggerGuts()
	{
		status.SetHP(1);    //体力を1で耐えさせる
		Debug.Log("<color=red>【ラーテル特性発動】致命傷をHP1で耐えた！</color>");
	}

	//スキル発動処理(継承)
	public override void Skill()
	{
		base.Skill(); // 共通の死亡チェックなどを実行

		//if (skillCooldownTimer > 0) return; // CT中なら発動不可
		Debug.Log("<color=green>ラーテル：固有スキルが発動した（現在はCT設定のみ）</color>");

		// ラーテル専用のCT（例: 10秒）を設定
		//skillCooldownTimer = 10.0f;
	}
}
