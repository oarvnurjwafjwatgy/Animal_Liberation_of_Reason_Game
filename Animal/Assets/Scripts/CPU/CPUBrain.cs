using Mode = ChangeMode;

public enum CPUOrder
{
	EscapeDeadZone,	// デッドゾーンから逃げる
	Attack,			// 攻撃
	Retreat,		//撤退・アイテム探し
	WatchOut,		//様子見
}

public class CPUBrain
{
	public static CPUOrder Think(Character_CPU cpu, NutsEffectManager nuts)
	{
		//【絶対条件】 デッドゾーンに入ったら逃げる
		if (cpu.isInsideDamageZone) return CPUOrder.EscapeDeadZone;

		//【条件】 アイテムでデバフを引いたら撤退
		if (nuts != null && nuts.CurrentSpeedModifier < 0) return CPUOrder.Retreat;

		//【条件】 体力が減ったら
		if (cpu.CurrentHP <= cpu.MaxHP / 2)
		{
			if (cpu.CurrentReason >= cpu.MaxReason && cpu.GetMode() == Mode.ANIMAL)
			{
				cpu.GetModeChange();
				return CPUOrder.Attack;
			}
			else return CPUOrder.Retreat;//無理なら撤退
		}
		return CPUOrder.Attack;//基本は攻撃
	}
}
