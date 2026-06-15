using UnityEngine;

public class Character_Rhinocelos : Character_Status
{
	// サイの突進状態管理用変数
	private bool isRhinoDashing = false;
	private Coroutine rhinoDashCoroutine;
	private float rhinoDashSpeedBoost = 1.0f;

	//親クラスの計算式を上書きして、サイが突進している時だけ速度を掛け算する
	public override float CurrentMoveSpeed => base.CurrentMoveSpeed * rhinoDashSpeedBoost;

	//固有スキル(継承)
	public override void Skill()
	{
		base.Skill();//共通チェック

		if (rhinoDashCoroutine != null) { StopDash(); return; }

		// 実行中でなければ、コルーチンを開始して猛スピードで走る
		rhinoDashCoroutine = StartCoroutine(RhinoDashLoop());
	}

	// 突進中の「継続処理」コルーチン
	private System.Collections.IEnumerator RhinoDashLoop()
	{
		isRhinoDashing = true;

		//スキル発動中に速度上昇バフのアイコンを表示
		if (MyUIManager != null)
			MyUIManager.CreateOrUpdateBuffUI(playerID, BuffType.SpeedBuff, 999f, buffContainer);

		rhinoDashSpeedBoost = 1.8f; // 突進開始.速度を1.8倍にアップ
		Debug.Log("<color=orange>サイ：突進スキル発動！猛スピードで理性を消費します</color>");

		while (CurrentReason > 0 && isRhinoDashing)
		{
			yield return new WaitForSeconds(0.1f);
			CurrentReason -= 1; // 理性を削る

			// もし理性が尽きたり死亡したらループを抜ける
			if (CurrentReason <= 0 || IsDead) break;
		}
		// ループを抜けたら終了処理
		StopDash();
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
		if (MyUIManager != null)
			MyUIManager.RemoveBuffUI(playerID, BuffType.SpeedBuff);

		rhinoDashSpeedBoost = 1.0f; // 速度を元に戻す
		isRhinoDashing = false;
		Debug.Log("<color=white>サイ：突進終了。速度が戻りました</color>");
	}

	//もし突進中に倒れたら、強制的に突進を止めるルールを上書き追加
	protected override void Die()
	{
		StopDash();   // 突進を安全に止める
		base.Die();   // 親クラスの共通死亡処理（UI非表示など）を動かす
	}
}
