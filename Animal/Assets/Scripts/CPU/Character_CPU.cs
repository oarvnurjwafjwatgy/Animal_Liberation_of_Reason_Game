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
	public Transform targetNut;     //狙う木の実のTransform

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

	[SerializeField] private float roamRadius = 12f;    // 放浪範囲
	[SerializeField] private float wanderInterval = 3f; // 目的地を変える間隔（秒）

	
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
		SearchForTarget(); // 定期的にターゲットを見直す（0.5秒ごとに周囲を索敵）
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
		//プレイヤーリストをもらった瞬間に、1P(Player)からCollisionプレハブの参照を自動でコピーする
		if (searcher != null) searcher.Initialize(playersList);
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

	private void Update()
	{
		InputPlayer inputCheck = GetComponent<InputPlayer>();
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

		// 木の実が誰かに取られて消滅した時の安全クリア処理
		if (targetNut != null && (!targetNut.gameObject.activeInHierarchy || targetNut.gameObject == null))
		{
			targetNut = null;
		}

		// targetNutがnullのときはフィールド全体で木の実を探す
		if (targetNut == null)
		{
			Transform foundNut = searcher.SearchNut();
			if (foundNut != null) targetNut = foundNut;
		}

		// 判断
		CPUOrder currentOrder = CPUBrain.Think(myStatus, this, nuts);

		// 攻撃射程チェック
		bool hasTrueEnemy = (targetEnemy != null);
		bool inRange = (hasTrueEnemy && Vector3.Distance(transform.position, targetEnemy.position) < 2.5f);

		// 移動か攻撃か、どちらか一つだけを行う
		if (inRange && currentOrder == CPUOrder.Attack) PerformAttack(currentOrder);
		else PerformMovement(currentOrder);
	}

	private void PerformAttack(CPUOrder order)
	{
		movement.StopImmediate(); // 移動を確実に止める
		string baseKeyName = (cpuNormalObject != null) ? cpuNormalObject.name : gameObject.name;
		attackHandler.HandleAttack(baseKeyName, targetEnemy, order, myStatus.CharaAnim, rb, animator, cpuNormalObject);
	}

	// 命令(order)に応じて移動ターゲットを選択する
	private void PerformMovement(CPUOrder order)
	{
		if (movement == null) movement = GetComponent<CPU_MovementHandler>();

		// 木の実関連の命令が来たら木の実を優先
		if ((order == CPUOrder.Retreat || order == CPUOrder.SearchNut) && targetNut != null)
			movement.CalculateMoveVelocity(targetNut, order, 5f);
		else
		{
			// Attack/WatchOut/EscapeDeadZone 等は targetEnemy を使う（null のときは movement 側で中央向け等を処理）
			movement.CalculateMoveVelocity(targetEnemy, order, 5f);
		}
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
		Transform lookTarget = targetNut != null ? targetNut : targetEnemy;
		if (lookTarget == null) return;
		if (movement.IsAvoidingWall) return;

		// CPUはターゲットへの方向を回転に。
		Vector3 directionToTarget = (lookTarget.position - transform.position).normalized;
		directionToTarget.y = 0; // 高さは無視
		if (directionToTarget != Vector3.zero)
		{
			// ターゲットを向くための回転値を作成
			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
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