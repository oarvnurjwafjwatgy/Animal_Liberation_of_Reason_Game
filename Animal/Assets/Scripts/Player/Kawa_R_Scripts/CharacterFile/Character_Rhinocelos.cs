using System;
using UnityEngine;

public class Character_Rhinocelos : Animal_Skill_TraitBase
{
	// サイの突進状態管理用変数
	private Coroutine rhinoDashCoroutine;
	private bool isRhinoDashing = false;
	private float rhinoDashSpeedBoost = 1.0f;
	public override float CurrentSpeedBoost { get { return rhinoDashSpeedBoost; } }

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

		rhinoDashSpeedBoost = 1.8f; // 突進開始.速度を1.8倍にアップ
		AnimalDebugLog("orange", "突進スキル発動！猛スピードで理性を消費します");

		while (status.CurrentReason > 0 && isRhinoDashing)
		{
			yield return new WaitForSeconds(0.1f);
			ReducedReasoning(1); // 理性を削る
			if (status.CurrentReason <= 0 || status.IsDead) { Die(); yield break; }// 理性が尽きたり死亡したらループ抜け
		}
		StopDash();// ループを抜けたら終了処理
	}

	// 突進を安全に止めるためのサイ専用の関数
	private void StopDash()
	{
		if (rhinoDashCoroutine != null)
		{
			StopCoroutine(rhinoDashCoroutine);
			rhinoDashCoroutine = null;
		}

		//UIの削除
		if (status.MyUIManager != null)
			status.MyUIManager.RemoveBuffUI(status.playerID, Character_Status.BuffType.SpeedBuff);

		rhinoDashSpeedBoost = 1.0f; // 速度を元に戻す
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
