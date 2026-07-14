using UnityEngine;
using Mode = ChangeMode;

public enum CPUOrder
{
	EscapeDeadZone,	// デッドゾーンから逃げる
	Attack,			// 攻撃
	Retreat,        //撤退
	SearchNut,      //木の実探し(アイテム)
	WatchOut,		//様子見
}

public class CPUBrain
{
	public static CPUOrder Think(Character_Status status, Character_CPU cpu, NutsEffectManager nuts)
	{
		//【絶対条件】 デッドゾーンに入ったら逃げる
		if (cpu.isInsideDamageZone) return CPUOrder.EscapeDeadZone;

		// ターゲットがいないなら、木の実を探す
		if (cpu.targetEnemy == null)
		{
			// 木の実があるなら木の実へ
			if (cpu.SearchNut() != null)
				return CPUOrder.Retreat;

			// 木の実がないなら様子見
			return CPUOrder.WatchOut;
		}

		//【条件】 アイテムでデバフを引いたら撤退
		if (nuts != null && nuts.CurrentSpeedModifier < 0) return CPUOrder.Retreat;

		// 【条件】ターゲットが遠すぎる場合は、追うのを諦めて様子見（Idle）にする
		if (cpu.targetEnemy != null)
		{
			float distance = Vector3.Distance(cpu.transform.position, cpu.targetEnemy.position);
			if (distance > 15.0f) // 15メートル以上離れたら諦める
			{
				cpu.targetEnemy = null; // ターゲットをリセット
				return CPUOrder.WatchOut;
			}
		}

		//【条件】 体力が減ったら
		if (status.CurrentHP <= status.MaxHP / 2)
		{
			if (status.CurrentReason >= status.MaxReason && status.GetMode() == Mode.ANIMAL)
			{
				status.GetModeChange();
				return CPUOrder.Attack;
			}
			else return CPUOrder.Retreat;//無理なら撤退
		}

		// 【条件】 壁に張り付いて進めていない時間をチェック
		if (cpu.IsStuck()) return CPUOrder.WatchOut; // 様子見（＝壁から離れる行動）に遷移

		// ターゲット不在時は「木の実探し」
		if (cpu.targetEnemy == null) return CPUOrder.Retreat;

		return CPUOrder.Attack;//基本は攻撃
	}
}
