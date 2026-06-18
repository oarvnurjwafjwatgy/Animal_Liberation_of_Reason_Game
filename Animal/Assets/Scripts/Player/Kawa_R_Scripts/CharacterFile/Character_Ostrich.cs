using UnityEngine;
using P = OstrichSkillParam;

//Animal_Skill_TraitBaseを元に。
public class Character_Ostrich : Animal_Skill_TraitBase
{
	//変数
	private float ostrichTimer = 0f; //ダチョウ回復専用タイマー
	private float ostrichSkillAtkBoost = P.OSTRICH_RESET_VALUE; //初期値:スキルの攻撃力(通常から上乗せ)

	public override float CurrentAtkBoost => ostrichSkillAtkBoost;

	//ダチョウ専用のスキル
	public override void Skill()
	{
		if (skillCooldownTimer > 0) return; //CT中なら発動不可
		if (IsSpecialAnimal) SetAtkBoost(ref ostrichSkillAtkBoost, P.REASON_SKILL_UP_VALUE);
		else SetAtkBoost(ref ostrichSkillAtkBoost, P.SKILL_UP_VALUE);

		base.Skill();   //共通の死亡チェックを実行
		AnimalDebugLog("yellow", "固有スキルが発動した");
		StartCoroutine(ResetAtkBoostAfterDelay(0.2f)); // 0.2秒後に自動で等倍に戻すコルーチン
		SetSkillCooldownTimer(P.OSTRICH_CT);
	}

	protected override void Update()
	{
		base.Update();
		Heal_Ostrich();
	}

	//回復処理(ダチョウの特性)
	private void Heal_Ostrich()
	{
		//もし死亡状態なら処理を行わない
		if (status.GetState() == Character_Status.State.DEAD) return;

		// 理性解放状態時のみ体力回復
		if (status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL)
		{
			ostrichTimer += Time.deltaTime;

			// 1秒ごとに回復
			if (ostrichTimer >= 1f)
			{
				int ostrich_heal = status.MaxHP * P.HEAL_HP_RATE / 100;  //回復式
				if (status.MaxHP != status.CurrentHP)
				{
					status.HealHP(ostrich_heal);    //CharacterStatusの回復関数にて反映
					AnimalDebugLog("green", "固有特性で回復中:" + status.CurrentHP);
				}
				ostrichTimer = 0f;
			}
		}
	}

	//少し処理を遅らせてから攻撃値を元に戻す処理
	private System.Collections.IEnumerator ResetAtkBoostAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay); // 指定した秒数（0.2秒など）だけ待つ
		SetAtkBoost(ref ostrichSkillAtkBoost, P.OSTRICH_RESET_VALUE); // ここで1.0倍に戻す
		AnimalDebugLog("white", "ダチョウのスキル倍率が元に戻りました");
	}
}
