using System.Collections.Generic;
using UnityEngine;
using A_CT = NormalAttack_CoolTime;
using CharaType = CharacterType;

/****自動アタッチ**************/
[RequireComponent(typeof(CPU_TargetSearcher))]
[RequireComponent(typeof(CPU_MovementHandler))]
[RequireComponent(typeof(CPU_AttackHandler))]

public class Character_CPU : MonoBehaviour
{
	[Header("CPU設定")]
	public Transform targetEnemy;   //狙うPlayerのTransform
	private Character_Status myStatus;
	private CPU_TargetSearcher searcher;
	private CPU_MovementHandler movement;
	private CPU_AttackHandler attackHandler;

	private Rigidbody rb;
	private Animator animator;
	private NutsEffectManager nuts;

	private GameObject cpuNormalObject;
	private GameObject cpuReasonObject;
	private InputPlayer playerRef;
	private List<Character_Status> allPlayers = new List<Character_Status>();

	private void Awake()
	{
		TryGetComponent(out rb);
		TryGetComponent(out nuts);
		myStatus = GetComponent<Character_Status>();
		searcher = GetComponent<CPU_TargetSearcher>();
		movement = GetComponent<CPU_MovementHandler>();
		attackHandler = GetComponent<CPU_AttackHandler>();
		playerRef = GetComponent<InputPlayer>();
	}

	void Start()
	{
		// 定期的にターゲットを見直す（0.5秒ごとに周囲を索敵）
		InvokeRepeating(nameof(SearchForTarget), 0.5f, 0.5f);
	}

	private void CheckAnimator()
	{
		if (animator != null) return;
		animator = GetComponentInChildren<Animator>();
		if (animator != null)
		{
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			Debug.Log($"{gameObject.name} (CPU) のAnimatorを捕捉しました。Cullingを解除。");
		}
	}

	public void SetAllPlayersList(List<Character_Status> playersList)
	{
		allPlayers = playersList;
		if (searcher != null) searcher.Initialize(playersList);

		//プレイヤーリストをもらった瞬間に、1P(Player)からCollisionプレハブの参照を自動でコピーする
		if (attackHandler != null) attackHandler.SetupCollisionPrefab(playersList);
	}

	public void SetupCPUModels(GameObject normal, GameObject reason)
	{
		this.cpuNormalObject = normal;
		this.cpuReasonObject = reason;
		animator = cpuNormalObject.GetComponentInChildren<Animator>();
		if (movement != null) movement.Initialize(rb);
	}

	public bool isInsideDamageZone = false; // 今デッドゾーンにいるか
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DamageZone")) isInsideDamageZone = true;
	}

	private void Update()
	{
		CheckAnimator();

		if (myStatus == null || myStatus.CurrentHP <= 0 || myStatus.CurrentReason <= 0)
		{
			if (movement != null) movement.StopImmediate();//速度0に。
			return;
		}
		//CPUOrder currentOrder = CPUBrain.Think(myStatus, this, nuts);
		InputPlayer inputCheck = GetComponent<InputPlayer>();
		if (inputCheck != null && !inputCheck.enabled)
		{
			if (movement != null) movement.StopImmediate();
			return;
		}

		CPUOrder currentOrder = CPUOrder.Attack;//デバッグ用に常に攻撃するように固定

		// 移動中か攻撃中かで条件分岐
		if (attackHandler != null && !attackHandler.IsAttacking)
		{
			if (movement != null) movement.CalculateMoveVelocity(targetEnemy, currentOrder, 5f);
			Vector3 moveDir = movement.CalculatedVelocity;
			moveDir.y = 0; // 水平方向だけに絞る

			if (playerRef != null && moveDir.sqrMagnitude > 0.01f)
			{
				// InputPlayerのメソッドを使って、現在の向きを計算
				Quaternion nextRot = playerRef.CalculateRotation(moveDir, cpuNormalObject.transform.rotation);

				// 計算された向きをモデルに適用
				if (cpuNormalObject != null) cpuNormalObject.transform.rotation = nextRot;
				if (cpuReasonObject != null) cpuReasonObject.transform.rotation = nextRot;
			}
		}
		else { if (movement != null) movement.StopImmediate(); }

		if (attackHandler != null)
		{
			string baseKeyName = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
			attackHandler.HandleAttack(baseKeyName, targetEnemy, currentOrder, myStatus.CharaAnim,
			rb, animator, cpuNormalObject);
		}
	}

	private void FixedUpdate()
	{
		if (rb == null || myStatus == null || myStatus.CurrentHP <= 0 || myStatus.CurrentReason <= 0) return;

		//実際の物理移動・回転・アニメ同期をコンポーネント側で実行
		if (movement != null) movement.ExecuteFixedUpdate(animator, attackHandler.IsAttacking);
	}

	//索敵処理を実行するメソッド
	private void SearchForTarget()
	{
		if (searcher != null)
		{
			searcher.Search();//索敵処理を実行
			targetEnemy = searcher.TargetEnemy;//ターゲットの参照を同期
		}
	}

	private void LateUpdate()
	{
		// ターゲットがいないなら何もしない
		if (targetEnemy == null) return;

		// PlayerのLateUpdateと同じ考え方で「回転」を計算する
		// Playerは「スティック入力」を回転にしているが、
		// CPUは「ターゲットへの方向」を回転にすれば、動きは完全に一致する
		Vector3 directionToTarget = (targetEnemy.position - transform.position).normalized;
		directionToTarget.y = 0; // 高さは無視

		if (directionToTarget != Vector3.zero)
		{
			// ターゲットを向くための回転値を作成
			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
			Debug.Log("Late発動。回転値を作成しました。");

			// Playerがやっているようにモデルを回す
			if (cpuNormalObject != null) cpuNormalObject.transform.rotation = targetRotation;
			if (cpuReasonObject != null) cpuReasonObject.transform.rotation = targetRotation;
		}
	}
}
