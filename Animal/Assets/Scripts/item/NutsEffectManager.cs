using UnityEngine;
using System.Collections.Generic;
using BfType = BuffType;

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
		CheckNutsEfficacyTime();
	}

	// バフの種類に応じて、対応するタイマーと効果量を更新するメソッド
	public void TriggerBuff(BfType type)
	{
		switch (type)
		{
			case BfType.SpeedBuff:
				ApplyBuffParam(type, NutsData.SpeedBuff, "<color=#80ffff>スピードバフを付与しました</color>");
				break;

			case BfType.SpeedDebuff:
				ApplyBuffParam(type, NutsData.SpeedDebuff, "<color=#00ffff>スピードデバフを付与しました</color>");
				break;

			case BfType.AttackBuff:
				ApplyBuffParam(type, NutsData.AttackBuff, "<color=#ff8080>攻撃力バフを付与しました</color>");
				break;

			case BfType.AttackDebuff:
				ApplyBuffParam(type, NutsData.AttackDebuff, "<color=#ff0000>攻撃力デバフを付与しました</color>");
				break;
		}
	}

	// バフの種類に応じて、対応するタイマーと効果量を更新するメソッド
	private void ApplyBuffParam(BfType type, NutsParam param, string logMessage)
	{
		// バフの種類に応じて、対応するタイマーと効果量を更新
		switch (type)
		{
			case BfType.SpeedBuff:
				SpeedBuffTimer = param.duration;
				SpeedBuffPower = param.power;
				break;
			case BfType.SpeedDebuff:
				SpeedDebuffTimer = param.duration;
				SpeedDebuffPower = param.power;
				break;
			case BfType.AttackBuff:
				AttackBuffTimer = param.duration;
				AttackBuffPower = param.power;
				break;
			case BfType.AttackDebuff:
				AttackDebuffTimer = param.duration;
				AttackDebuffPower = param.power;
				break;
		}

		// バフUIの生成・更新
		if (status.MyUIManager != null)
			status.MyUIManager.CreateOrUpdateBuffUI(status.playerID, type, param.duration,
			status.buffContainer);

		Debug.Log(logMessage);
	}

	// 現在アクティブなバフの種類を管理するリスト
	private List<BfType> activeBuffTypes = new List<BfType>();
	private void CheckNutsEfficacyTime()
	{
		// 動いてるバフの種類をリストに追加
		activeBuffTypes.Clear();

		if (SpeedBuffTimer > 0f) activeBuffTypes.Add(BfType.SpeedBuff);
		if (SpeedDebuffTimer > 0f) activeBuffTypes.Add(BfType.SpeedDebuff);
		if (AttackBuffTimer > 0f) activeBuffTypes.Add(BfType.AttackBuff);
		if (AttackDebuffTimer > 0f) activeBuffTypes.Add(BfType.AttackDebuff);

		// もしアクティブなバフがなければ、処理を抜ける
		if (activeBuffTypes.Count == 0) return;

		//動いてるバフだけをswitch文で処理
		for (int i =0; i<activeBuffTypes.Count; i++)
		{
			BfType currentType = activeBuffTypes[i];


			switch (currentType)
			{
				case BfType.SpeedBuff:
					// 計算結果を受け取ってプロパティを更新
					SpeedBuffTimer = UpdateSingleTimer(currentType, SpeedBuffTimer, "スピードバフ");
					if (SpeedBuffTimer <= 0f) SpeedBuffPower = 0f;
					break;

				case BfType.SpeedDebuff:
					SpeedDebuffTimer = UpdateSingleTimer(currentType, SpeedDebuffTimer, "スピードデバフ");
					if (SpeedDebuffTimer <= 0f) SpeedDebuffPower = 0f;
					break;

				case BfType.AttackBuff:
					AttackBuffTimer = UpdateSingleTimer(currentType, AttackBuffTimer, "攻撃力バフ");
					if (AttackBuffTimer <= 0f) AttackBuffPower = 0f;
					break;

				case BfType.AttackDebuff:
					AttackDebuffTimer = UpdateSingleTimer(currentType, AttackDebuffTimer, "攻撃力デバフ");
					if (AttackDebuffTimer <= 0f) AttackDebuffPower = 0f;
					break;
			}
		}
	}


	// 単一のバフのタイマーを更新し、必要に応じてUIを削除するメソッド
	private float UpdateSingleTimer(BfType type, float timer, string logName)
	{
		timer -= Time.deltaTime;

		if (timer <= 0f)
		{
			timer = 0f;

			// バフUIの削除
			if (status.MyUIManager != null)
				status.MyUIManager.RemoveBuffUI(status.playerID, type);

			Debug.Log($"<color=#80ff80>{logName}削除しました</color>");
		}

		return timer;// 更新されたタイマー値を返す
	}
}
