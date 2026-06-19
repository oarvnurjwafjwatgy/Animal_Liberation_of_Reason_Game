using UnityEngine;

// 木の実_個別のパラメーター
public struct NutsParam
{
	public float duration; //効果時間
	public float power; //効果量

	public NutsParam(float duration, float power)
	{
		this.duration = duration;
		this.power = power;
	}
}

// 木の実ごとのパラメーター
public static class NutsData
{
	//スピード・攻撃力のバフ・デバフの効果時間と効果量
	public static readonly NutsParam SpeedBuff = new NutsParam(10.0f, 0.5f);
	public static readonly NutsParam SpeedDebuff = new NutsParam(10.0f, -0.25f);
	public static readonly NutsParam AttackBuff = new NutsParam(10.0f, 0.5f);
	public static readonly NutsParam AttackDebuff = new NutsParam(10.0f, -0.25f);

	//HP・理性の回復割合(最小%~最大%)
	public static readonly Vector2Int HpHealRate = new Vector2Int(2, 25);
	public static readonly Vector2Int ReasonHealRate = new Vector2Int(2, 25);
}