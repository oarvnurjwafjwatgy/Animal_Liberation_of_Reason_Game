using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

public class CPU_TargetSearcher : MonoBehaviour
{
	[Header("索敵設定")]
	[SerializeField] private float searchLimitRange = 10.0f; // 索敵範囲

	private Transform targetEnemy;
	private List<Character_Status> allPlayers = new List<Character_Status>();
	private GameObject centerFallbackObject;// 敵が見つからなかった場合に中心を向かせるためのダミーターゲットオブジェクト
	public Transform TargetEnemy => targetEnemy;

	public void Initialize(List<Character_Status> playersList)
	{
		allPlayers = playersList;

		// 敵がいない時に中心(0,0,0)へ向かせるためのダミーを作成
		if (centerFallbackObject == null)
		{
			centerFallbackObject = new GameObject($"_CenterFallback_{gameObject.name}");
			centerFallbackObject.transform.position = Vector3.zero;
		}
		Search();
	}

	// 外部から定期的に、または必要なタイミングで呼ばれる索敵コア処理
	public void Search()
	{
		if (allPlayers == null || allPlayers.Count == 0) return;
		Transform bestTarget = null;
		float closestDistance = float.MaxValue;
		//float lowestHp = float.MaxValue; // 体力が少ないやつを狙う用

		foreach (var p in allPlayers)
		{
			if (p == null || p.IsDead) continue;
			if (p.gameObject == this.gameObject) continue; // 自殺防止

			float distance = Vector3.Distance(transform.position, p.transform.position);// ターゲットとの距離を計算

			if (distance > searchLimitRange) continue; // 索敵範囲外は無視

			// 思考ロジック:一番近い敵を狙う
			if (distance < closestDistance)
			{
				closestDistance = distance;
				bestTarget = p.transform;
			}

			/* ハイエナしたい場合（仮）
            if (p.CurrentHp < lowestHp)
            {
                lowestHp = p.CurrentHp;
                bestTarget = p.transform;
            }
            */
		}
		if (bestTarget != null) targetEnemy = bestTarget;//範囲内に誰もいなければnullになる
		else targetEnemy = centerFallbackObject != null ? centerFallbackObject.transform : null;
	}

	public Transform SearchNut()
	{
		GameObject[] nuts = GameObject.FindGameObjectsWithTag("Nuts");
		Transform nearest = null;
		float minDist = Mathf.Infinity;

		foreach (var nut in nuts)
		{
			float dist = Vector3.Distance(transform.position, nut.transform.position);
			if (dist > minDist)
			{
				minDist = dist;
				nearest = nut.transform;
			}
		}
		return nearest;
	}
}
