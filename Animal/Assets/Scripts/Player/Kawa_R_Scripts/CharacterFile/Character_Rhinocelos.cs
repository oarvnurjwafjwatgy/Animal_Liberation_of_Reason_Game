using UnityEngine;
using P = RhinocelosSkillParam;

public class Character_Rhinocelos : Animal_Skill_TraitBase
{
	private Coroutine rhinoDashCoroutine;
	private bool isRhinoDashing = false;	//突進フラグ
	
	// サイの突進状態管理用変数
	private float rhinoDashSpeedBoost = P.RHINOCELOS_RESET_VALUE;	//速度
	private float rhinoSkillAtkBoost = P.RHINOCELOS_RESET_VALUE;	//スキルの攻撃力(通常から上乗せ)

	public override float CurrentSpeedBoost { get { return rhinoDashSpeedBoost; } }
	public override float CurrentAtkBoost => rhinoSkillAtkBoost;

	//固有スキル(継承)
	public override void Skill()
	{
		if (rhinoDashCoroutine != null) { StopDash(); return; }

		// 実行中でなければ、コルーチンを開始して猛スピードで走る
		rhinoDashCoroutine = StartCoroutine(RhinoDashLoop());
	}

	// 突進中の「継続処理」コルーチン
	private System.Collections.IEnumerator RhinoDashLoop()
	{
		isRhinoDashing = true;

		//スキル発動中に速度上昇バフのアイコンを表示
		if (status.MyUIManager != null)
			status.MyUIManager.CreateOrUpdateBuffUI(status.playerID,
			Character_Status.BuffType.SpeedBuff, 999f, status.buffContainer);

		rhinoDashSpeedBoost = P.RHINOCELOS_SPPEED_UP; // 突進開始.速度を1.8倍にアップ

		// モードチェック
		if (IsSpecialAnimal) SetAtkBoost(ref rhinoSkillAtkBoost, P.REASON_SKILL_UP_VALUE);//理性解放:突進に当たると1.6倍
		else SetAtkBoost(ref rhinoSkillAtkBoost, P.SKILL_UP_VALUE);						  //突進に当たると1.4倍

		AnimalDebugLog("orange", "突進スキル発動！猛スピードで理性を消費します");
		float reaon_timer = AnimalParam.TIMER_RESET;	//理性ゲージを削る間隔のタイマー

		while (status.CurrentReason > 0 && isRhinoDashing)
		{
			yield return new WaitForSeconds(0.1f);
			reaon_timer += 0.1f;
			if (reaon_timer >= P.REASON_DECREASEINTERVAL)
			{
				ReducedReasoning(P.REASON_DECREASEAMOUNT); // 理性を削る
				reaon_timer = AnimalParam.TIMER_RESET;	//リセット
			}
			if (status.CurrentReason <= 0 || status.IsDead) { Die(); yield break; }// 理性が尽きたり死亡したらループ抜け
		}
		StopDash();// ループを抜けたら終了処理
	}

	// 突進を安全に止めるためのサイ専用の関数
	private void StopDash()
	{
		// サイの突進を強制停止
		if (rhinoDashCoroutine != null)
		{ StopCoroutine(rhinoDashCoroutine); rhinoDashCoroutine = null; }
		
		//UIの削除
		if (status.MyUIManager != null)
			status.MyUIManager.RemoveBuffUI(status.playerID, Character_Status.BuffType.SpeedBuff);

		rhinoDashSpeedBoost = P.RHINOCELOS_RESET_VALUE; // 速度を元に戻す
		SetAtkBoost(ref rhinoSkillAtkBoost, P.RHINOCELOS_RESET_VALUE);	//攻撃値を戻す
		isRhinoDashing = false;
		AnimalDebugLog("white", "突進終了。速度が戻りました");
	}

	//もし突進中に倒れたら、強制的に突進を止めるルールを上書き追加
	protected override void Die()
	{
		StopDash();   // 突進を安全に止める
		base.Die();   // 親クラスの共通死亡処理（UI非表示など）を動かす
	}
}
