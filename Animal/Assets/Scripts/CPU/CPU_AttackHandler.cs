using System.Collections.Generic;
using UnityEngine;
using A_CT = NormalAttack_CoolTime;
using CharaType = CharacterType;

public class CPU_AttackHandler : MonoBehaviour
{
	[Header("攻撃設定")]
	[SerializeField] private float attackRange = 2.8f;       // 攻撃を仕掛ける間合い

	private GameObject collisionPrefab;
	private EffectManager effectManager;
	private float lastAttackTime;
	private bool isAttacking = false;
	private float attackEndTime;

	private Vector3 effectOffset = new Vector3(0f, 0.2f, 0f);

	public bool IsAttacking => isAttacking;
	public float AttackRange => attackRange;

	private void Start()
	{
		// シーン内のエフェクトマネージャーを自動補正
		GameObject effObj = GameObject.Find("EffectManager");
		if (effObj != null) effectManager = effObj.GetComponent<EffectManager>();
	}

	public void SetupCollisionPrefab(List<Character_Status> playersList)
	{
		FetchCollisionPrefabFromPlayers(playersList);
	}

	public void HandleAttack(
	string baseKeyName,
	Transform target,
	CPUOrder order,
	CharaType charaType,
	Rigidbody rb,
	Animator animator,
	GameObject model)
	{
		// 攻撃中なら終了時間を待つ
		if (isAttacking)
		{
			if (Time.time >= attackEndTime) { Debug.Log("CPU攻撃終了"); isAttacking = false; }
			else return;
		}
		if (target == null || order != CPUOrder.Attack || isAttacking) return;

		// もしターゲットが「中心のダミー」だった場合は、攻撃を空振りしないように弾く
		if (target.name.StartsWith("_CenterFallback_")) return;

		if (Vector3.Distance(transform.position, target.position) > attackRange) return;

		float cooldown = GetNormalAttackCoolTime(charaType);

		if (Time.time - lastAttackTime < cooldown) return;
		lastAttackTime = Time.time;

		// 攻撃前に敵の方向を向く
		Vector3 dir = target.position - model.transform.position;
		dir.y = 0f;
		if (dir.sqrMagnitude > 0.01f) model.transform.rotation = Quaternion.LookRotation(dir);
		if (animator != null)
		{
			isAttacking = true;
			attackEndTime = Time.time + 0.8f;
			animator.SetTrigger("Attack");

			// プレイヤーと同じ登録名のキーを使ってエフェクトを発生させる
			PlayAnimalAttackFX(baseKeyName, charaType, animator);

			//動物ごとのSE・エフェクト再生
			//PlayAnimalAttackFX(charaType, animator);

			// 攻撃コライダー生成
			SpawnAttackCollider(animator);

			Vector3 pushDir = (target.position - transform.position).normalized;
			pushDir.y = 0;
			if (rb != null) rb.AddForce(pushDir * 4f, ForceMode.VelocityChange);
		}
		Debug.Log($"CPUが通常攻撃を繰り出しました！");
	}
	public void OnMoveResume() { Debug.Log("CPU_AttackHandler.MoveFlagFalse"); isAttacking = false; }
	private void PlayAnimalAttackFX(string baseKeyName, CharaType charaType, Animator animator)
	{
		//if (effectManager == null || animator == null) return;
		if (effectManager == null)
		{
			Debug.LogError("EffectManager NULL");
			return;
		}

		if (animator == null)
		{
			Debug.LogError("Animator NULL");
			return;
		}
		GameObject activeModel = animator.gameObject;

		Vector3 basePos = activeModel.transform.position +
		activeModel.transform.forward * effectOffset.z +
		activeModel.transform.up * effectOffset.y +
		activeModel.transform.right * effectOffset.x;

		string animalName = activeModel.name;
		if (animalName.Contains("Ostrich")) basePos += new Vector3(0f, 0.2f, 0f);

		Vector3 pos = basePos;
		Quaternion rot = activeModel.transform.rotation;
		Vector3 scale = Vector3.one;

		switch (charaType)
		{
			case CharaType.LION:
				AudioManager.Instance.PlaySEByIndex(2, 1.5f);
				pos = basePos + activeModel.transform.up * 0.43f;
				scale = new Vector3(0.4f, 0.4f, 0.4f);
				effectManager.PlayEffect(baseKeyName, 0, pos, rot, scale);
				break;

			case CharaType.OSTRICH:
				AudioManager.Instance.PlaySEByIndex(3, 1.5f);
				pos = basePos - activeModel.transform.right * 0.5f;
				rot = activeModel.transform.rotation * Quaternion.Euler(20f, 0, 0);
				scale = new Vector3(0.3f, 0.3f, 0.3f);
				effectManager.PlayEffect(baseKeyName, 0, pos, rot, scale, this.transform);
				break;

			case CharaType.RHINOCELOS:
				AudioManager.Instance.PlaySEByIndex(4, 1.5f);
				scale = new Vector3(0.3f, 0.3f, 0.3f);
				effectManager.PlayEffect(baseKeyName, 0, pos, rot, scale, this.transform);
				break;

			case CharaType.RATEL:
				AudioManager.Instance.PlaySEByIndex(5, 1.5f);
				pos = basePos + activeModel.transform.right * 0.6f + activeModel.transform.up * 0.5f;
				rot = activeModel.transform.rotation * Quaternion.Euler(0, 0, 30);
				scale = new Vector3(0.8f, 0.8f, 0.8f);
				effectManager.PlayEffect(baseKeyName, 0, pos, rot, scale, this.transform);
				break;
		}
	}

	private float GetNormalAttackCoolTime(CharaType type)
	{
		switch (type)
		{
			case CharaType.LION: return A_CT.LION_ATTACK_CT;
			case CharaType.OSTRICH: return A_CT.OSTRICH_ATTACK_CT;
			case CharaType.RHINOCELOS: return A_CT.RHINOCELOS_ATTACK_CT;
			case CharaType.RATEL: return A_CT.RATEL_ATTACK_CT;
			default: return 1.0f;
		}
	}
	private void FetchCollisionPrefabFromPlayers(List<Character_Status> allPlayers)
	{
		if (collisionPrefab != null) return;

		foreach (var p in allPlayers)
		{
			if (p == null) continue;

			if (p.TryGetComponent<InputPlayer>(out var inputPlayer))
			{
				System.Reflection.FieldInfo field = typeof(InputPlayer).GetField("Collision", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (field != null)
				{
					collisionPrefab = field.GetValue(inputPlayer) as GameObject;
					if (collisionPrefab != null) return;
				}
			}
		}
		if (collisionPrefab == null) collisionPrefab = Resources.Load<GameObject>("Collision");
	}

	private void SpawnAttackCollider(Animator animator)
	{
		if (collisionPrefab == null) return;
		GameObject activeModel = (animator != null) ? animator.gameObject : this.gameObject;
		Vector3 spawnPosition = activeModel.transform.position +
		new Vector3(0f, 0.5f, 0f) + activeModel.transform.forward * 1.0f;

		Instantiate(collisionPrefab, spawnPosition, activeModel.transform.rotation, this.gameObject.transform);
	}
}
