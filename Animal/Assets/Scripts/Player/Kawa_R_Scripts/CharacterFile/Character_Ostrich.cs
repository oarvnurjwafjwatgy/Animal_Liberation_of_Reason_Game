using UnityEngine;

//Character_Statusへ継承
public class Character_Ostrich : Character_Status
{
    [Header("毎時体力回復能力(ダチョウ固有)")]
    //[SerializeField] private int Heal_hp_rate = 1;  //体力回復の割合量
    private float ostrichTimer = 0f;                //ダチョウ回復専用タイマー
    private const float OSTRICH_CT = 8.0f;          //スキルCT

	//ダチョウ専用のスキル
	public override void Skill()
	{
		base.Skill();   //共通の死亡チェックを実行

		//if (skillCooldownTimer > 0) return; //CT中なら発動不可
		Debug.Log("<color=green>ダチョウ：固有スキルが発動した（現在はCT設定のみ）</color>");
		//skillCooldownTimer = OSTRICH_CT; // ダチョウ用のCTをセット
	}

	// 固有特性(継承)
	protected override void Characteristic()
	{
		base.Characteristic();  //死亡チェック

		if (CurrentHP > 0)
		{
			// 理性解放状態時のみ体力が回復
			if (GetMode() == Mode.SPSIAL_ANIMAL)
			{
				int ostrich_heal = MaxHP * Heal_hp_rate / 100;  //回復式
				ostrichTimer += Time.deltaTime;

				// 1秒経過ごとに回復
				if (ostrichTimer >= 1f)
				{
					if (MaxHP != CurrentHP)
					{
						CurrentHP += ostrich_heal;
						if (CurrentHP > MaxHP) CurrentHP = MaxHP;
						Debug.Log("ダチョウの固有特性で回復中: " + CurrentHP);
					}
					ostrichTimer = 0f;
				}
			}
		}
	}
}
