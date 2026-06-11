using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutsEffectManager : MonoBehaviour
{
	// スピード・攻撃力_バフ,デバフ_効果時間
	public float SpeedBuffTimer { get; private set; } = 0f;
	public float SpeedDebuffTimer { get; private set; } = 0f;
	public float AttackBuffTimer { get; private set; } = 0f;
	public float AttackDebuffTimer { get; private set; } = 0f;


	// スピード・攻撃力_バフ,デバフ_効果量
	public float SpeedBuffPower { get; private set; } = 0f;
	public float SpeedDebuffPower { get; private set; } = 0f;
	public float AttackBuffPower { get; private set; } = 0f;
	public float AttackDebuffPower { get; private set; } = 0f;


	public float CurrentAttackModifier => AttackBuffPower + AttackDebuffPower;
	public float CurrentSpeedModifier => SpeedBuffPower + SpeedDebuffPower;

	//参照
	private Character_Status status;

	//最初に呼び出す
	private void Awake()
	{
		status = GetComponent<Character_Status>();
	}

	//更新
	private void Update()
	{
		
	}

	private void ApplyBuffParam(Character_Status.BuffType type,NutsParam param,
	ref float timer, ref float power,string logMessage)
	{
		timer = param.duration;
		power = param.power;
	}
}
