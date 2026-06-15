using UnityEngine;

public class Character_HoneyBadger : Character_Status
{
	// ラーテル専用：くいしばり特性のフラグ
	private bool hasTriggeredGuts = false;

	//受けた時の処理(継承)&ラーテルの特性処理
	public override void TakeDamage(int damage)
	{
		base.TakeDamage(damage);    //普通にダメージを減らす

		//もしこのダメージでHPが0以下になり、まだ特性を使っていないなら発動
		if (CurrentHP <= 0 && !hasTriggeredGuts)
		{
			hasTriggeredGuts = true; // 特性使用済みにする

			CurrentHP = 1; // HPを1で耐える
			Debug.Log("<color=red>【ラーテル特性発動】致命傷をHP1で耐えた！</color>");
		}

		// そのまま強制的に「理性解放（SPECIAL_ANIMAL）状態」へ移行する
		CharaMode = Mode.SPSIAL_ANIMAL;
		Debug.Log("<color=purple>ラーテル：強制理性解放！</color>");
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
