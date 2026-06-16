using UnityEngine;

//Animal_Skill_TraitBaseを元に。
public class Character_Ostrich : Animal_Skill_TraitBase
{
	[Header("毎時体力回復能力(ダチョウ固有)")]
	[SerializeField] private int Heal_hp_rate = 1;  //体力回復の割合量
	private float ostrichTimer = 0f;                //ダチョウ回復専用タイマー
	private const float OSTRICH_CT = 8.0f;          //スキルCT

	//ダチョウ専用のスキル
	public override void Skill()
	{
		base.Skill();   //共通の死亡チェックを実行

		if (skillCooldownTimer > 0) return; //CT中なら発動不可
		AnimalDebugLog("yellow", "固有スキルが発動した");
		SetSkillCooldownTimer(OSTRICH_CT);
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
				int ostrich_heal = status.MaxHP * Heal_hp_rate / 100;  //回復式
				if (status.MaxHP != status.CurrentHP)
				{
					status.HealHP(ostrich_heal);    //CharacterStatusの回復関数にて反映
					AnimalDebugLog("green", "固有特性で回復中:" + status.CurrentHP);
				}
				ostrichTimer = 0f;
			}
		}
	}
}
