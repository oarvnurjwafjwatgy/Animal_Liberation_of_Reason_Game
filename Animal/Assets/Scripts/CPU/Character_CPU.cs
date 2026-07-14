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

	private float stuckTimer = 0f;

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
		SearchForTarget();
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
		if (movement != null)
		{
			movement.Initialize(rb);
			movement.SetModels(normal, reason);
		}
	}

	public bool isInsideDamageZone = false; // 今デッドゾーンにいるか
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DamageZone")) isInsideDamageZone = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("DamageZone")) isInsideDamageZone = false;
	}

	//private void Update()
	//{
	//	if (!IsAlive()) return;
	//	CPUOrder currentOrder = CPUBrain.Think(myStatus, this, nuts);
	//	if (targetEnemy == null)
	//	{
	//		// ターゲットがいない時は、移動処理に「ターゲットなし」を伝えて中央へ向かわせる
	//		movement.CalculateMoveVelocity(null, currentOrder, 5f);
	//		return;
	//	}
	//	// 4. 行動決定
	//	float distance = Vector3.Distance(transform.position, targetEnemy.position);
	//	bool inRange = distance < 2.5f; // 攻撃射程

	//	if (inRange && currentOrder == CPUOrder.Attack)
	//	{
	//		// 攻撃圏内なら「攻撃のみ」
	//		movement.StopImmediate();
	//		ExecuteAttack(currentOrder);
	//	}
	//	else
	//	{
	//		// 圏外なら「移動のみ」
	//		movement.CalculateMoveVelocity(targetEnemy, currentOrder, 5f);
	//	}

	//	//bool canAttack = (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.position) < 2.5f);

	//	//// 移動か攻撃かの切り替え
	//	//if (canAttack && currentOrder == CPUOrder.Attack)
	//	//{
	//	//	// 【射程内：攻撃モード】
	//	//	movement.StopImmediate(); // 移動停止
	//	//	if (attackHandler != null)
	//	//	{
	//	//		string baseKeyName = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
	//	//		attackHandler.HandleAttack(baseKeyName, targetEnemy, currentOrder, myStatus.CharaAnim, rb, animator, cpuNormalObject);
	//	//	}
	//	//}
	//	//else
	//	//{
	//	//	// 【射程外：移動モード】
	//	//	if (movement != null)
	//	//	{
	//	//		movement.CalculateMoveVelocity(targetEnemy, currentOrder, 5f);
	//	//	}
	//	//}

	//	//CheckAnimator();

	//	//if (myStatus == null || myStatus.CurrentHP <= 0 || myStatus.CurrentReason <= 0)
	//	//{
	//	//	if (movement != null) movement.StopImmediate();//速度0に。
	//	//	return;
	//	//}

	//	//CPUOrder currentOrder = CPUBrain.Think(myStatus, this, nuts);
	//	//// 【最優先：ターゲットがいないときは探索（＝中央への移動命令）】
	//	//if (targetEnemy == null)
	//	//{
	//	//	movement.CalculateMoveVelocity(null, CPUOrder.EscapeDeadZone, 5f);
	//	//	return; // 何もしない
	//	//}

	//	//InputPlayer inputCheck = GetComponent<InputPlayer>();
	//	//if (inputCheck != null && !inputCheck.enabled)
	//	//{
	//	//	if (movement != null) movement.StopImmediate();
	//	//	return;
	//	//}

	//	////攻撃射程チェック
	//	//bool inRange = false;
	//	//if (targetEnemy != null)
	//	//{
	//	//	float dist = Vector3.Distance(transform.position, targetEnemy.position);
	//	//	inRange = (dist < 2.5f); // 射程2.5m以内なら攻撃圏内
	//	//}


	//	////CPUOrder currentOrder = CPUOrder.Attack;//デバッグ用に常に攻撃するように固定

	//	//// 移動中か攻撃中かで条件分岐
	//	////if (attackHandler != null && !attackHandler.IsAttacking)
	//	////{
	//	////	if (movement != null) movement.CalculateMoveVelocity(targetEnemy, currentOrder, 5f);
	//	////	Vector3 moveDir = movement.CalculatedVelocity;
	//	////	moveDir.y = 0; // 水平方向だけに絞る

	//	////	if (playerRef != null && moveDir.sqrMagnitude > 0.01f)
	//	////	{
	//	////		// InputPlayerのメソッドを使って、現在の向きを計算
	//	////		Quaternion nextRot = playerRef.CalculateRotation(moveDir, cpuNormalObject.transform.rotation);

	//	////		// 計算された向きをモデルに適用
	//	////		if (cpuNormalObject != null) cpuNormalObject.transform.rotation = nextRot;
	//	////		if (cpuReasonObject != null) cpuReasonObject.transform.rotation = nextRot;
	//	////	}
	//	////}
	//	////else { if (movement != null) movement.StopImmediate(); }

	//	//if (currentOrder == CPUOrder.Attack && rb.velocity.magnitude < 0.1f)
	//	//{
	//	//	// 進めていないなら：少しだけ右を向く
	//	//	transform.Rotate(0, 90 * Time.deltaTime, 0);
	//	//	rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);//一瞬だけ後ろに押し出す（壁から離す）
	//	//}

	//	//if (attackHandler != null)
	//	//{
	//	//	string baseKeyName = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
	//	//	attackHandler.HandleAttack(baseKeyName, targetEnemy, currentOrder, myStatus.CharaAnim,
	//	//	rb, animator, cpuNormalObject);
	//	//}

	//	//// 攻撃中かつ、速度がほぼゼロの時、スタックタイマーを増やす
	//	//if (rb.velocity.magnitude < 0.1f && targetEnemy != null)
	//	//{
	//	//	stuckTimer += Time.deltaTime;
	//	//}
	//	//else
	//	//{
	//	//	stuckTimer = 0f;
	//	//}
	//}

	//更新
	private void Update()
	{
		InputPlayer inputCheck = GetComponent<InputPlayer>();

		//スタート時は強制停止
		if (inputCheck != null && !inputCheck.enabled)
		{
			if (movement != null) movement.StopImmediate();
			return;
		}

		if (myStatus == null || myStatus.CurrentHP <= 0 || myStatus.CurrentReason <= 0)
		{
			movement?.StopImmediate();
			return;
		}

		// 判断
		CPUOrder currentOrder = CPUBrain.Think(myStatus, this, nuts);

		// 攻撃射程チェック
		bool inRange = (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.position) < 2.5f);

		// 移動か攻撃か、どちらか一つだけを行う
		if (inRange && currentOrder == CPUOrder.Attack)
		{
			PerformAttack(currentOrder);
		}
		else
		{
			PerformMovement(currentOrder);
		}

		// スタック判定（移動できない時にタイマーを進める）現在不安定。処理が上手くいかないため削除を考慮中
		UpdateStuckTimer();
	}

	private void PerformAttack(CPUOrder order)
	{
		movement.StopImmediate(); // 移動を確実に止める
		string baseKeyName = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
		attackHandler.HandleAttack(baseKeyName, targetEnemy, order, myStatus.CharaAnim, rb, animator, cpuNormalObject);
	}

	private void PerformMovement(CPUOrder order)
	{
		if (movement == null) movement = GetComponent<CPU_MovementHandler>(); // 自己修復を試みる

		if (targetEnemy == null) movement.CalculateMoveVelocity(null, order, 5f);
		else movement.CalculateMoveVelocity(targetEnemy, order, 5f);
	}

	private void UpdateStuckTimer()
	{
		if (rb.velocity.magnitude < 0.1f && targetEnemy != null)
		{
			stuckTimer += Time.deltaTime;
			// スタック時の回避挙動
			if (stuckTimer > 1.0f)
			{
				transform.Rotate(0, 90 * Time.deltaTime, 0);
				rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);
			}
		}
		else stuckTimer = 0f;
	}

	public bool IsStuck()
	{
		// 1.5秒動けなかったら「スタック」とみなす
		return stuckTimer > 1.5f;
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

		if (movement.IsAvoidingWall)
			return;

		// CPUはターゲットへの方向を回転に。
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

	public Transform SearchNut()
	{
		return searcher.SearchNut();
	}


	private bool IsAlive() => myStatus != null && myStatus.CurrentHP > 0 && myStatus.CurrentReason > 0;

	private void ExecuteAttack(CPUOrder order)
	{
		if (attackHandler == null) return;
		string name = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
		attackHandler.HandleAttack(name, targetEnemy, order, myStatus.CharaAnim, rb, animator, cpuNormalObject);
	}
}
