using UnityEngine;

public class CPU_MovementHandler : MonoBehaviour
{
	[Header("移動設定")]
	[SerializeField] private float stopDistance = 2.2f;      // 相手を押し付けないように立ち止まる距離
	private LayerMask obstacleLayer;

	private float wanderTimer = 0f;
	private Vector3 wanderTarget = Vector3.zero;
	[SerializeField] private float roamRadius = 12f;    // 放浪範囲 (m)
	[SerializeField] private float wanderInterval = 3f; // 目的地を変える間隔 (秒)

	private Rigidbody rb;
	private Vector3 calculatedVelocity = Vector3.zero;
	private GameObject normalModel;
	private GameObject reasonModel;
	private float avoidTimer = 0f;
	CPU_TargetSearcher searcher;

	// 壁回避用
	private bool isAvoidingWall = false;
	private Vector3 avoidDirection;
	public bool IsAvoidingWall => isAvoidingWall;

	// 大元や他のコンポーネントから現在の計算速度を参照できるようにするプロパティ
	public Vector3 CalculatedVelocity => calculatedVelocity;

	// 追加：外から「今どの方向を目指しているか」を取り出せるようにする
	public Vector3 CurrentMoveDirection { get; private set; }

	//初期化:Character_CPUからRigidbodyを受け取る
	public void Initialize(Rigidbody rigidbody) { rb = rigidbody; }

	private void Awake()
	{
		obstacleLayer = LayerMask.GetMask("FieldObject");
		searcher = GetComponent<CPU_TargetSearcher>();
	}
	public void SetModels(GameObject normal, GameObject reason)
	{
		normalModel = normal;
		reasonModel = reason;
	}

	public void CalculateMoveVelocity(Transform target, CPUOrder order, float speed)
	{
		Vector3 baseVelocity = MoveByOrder(order, target, speed);// ターゲット方向等を予めセット

		//止まる命令なら終わり
		if (baseVelocity.sqrMagnitude < 0.1f/*0.001f*/)
		{
			calculatedVelocity = Vector3.zero;
			return;
		}
		if (HandleWallAvoid(baseVelocity.normalized, speed)) return;
		calculatedVelocity = baseVelocity;
	}

	// 物理移動と回転:アニメーションの実行
	public void ExecuteFixedUpdate(
	Animator animator,
	bool isAttacking)
	{
		if (rb == null) return;

		// 計算された移動速度をRigidbodyに適用（Y軸の落下速度は維持する）
		rb.velocity = new Vector3(calculatedVelocity.x, rb.velocity.y, calculatedVelocity.z);

		// 壁回避していないなら移動方向へ向ける（回避後の向き戻し）
		if (!isAvoidingWall)
		{
			Vector3 moveDir = calculatedVelocity;
			moveDir.y = 0;
			if (moveDir.sqrMagnitude > 0.01f)
			{
				RotateModelTowards(moveDir.normalized);
			}
		}

		if (animator == null) return;
		if (isAttacking) return;

		if (calculatedVelocity.magnitude > 0.1f) animator.SetInteger("State", 1); // move
		else animator.SetInteger("State", 0); // idle
	}

	private bool HandleWallAvoid(Vector3 moveDir, float speed)
	{
		Vector3 rayOrigin = transform.position + Vector3.up;
		if (avoidTimer > 0f)
		{
			isAvoidingWall = true;

			avoidTimer -= Time.deltaTime;
			calculatedVelocity = avoidDirection * speed;
			RotateModelTowards(avoidDirection);
			return true;
		}

		isAvoidingWall = false;   // 回避タイマーが切れていた場合はフラグを解除
		RaycastHit hit;

		//壁にぶつかると
		if (Physics.Raycast(rayOrigin, moveDir, out hit, 2f, obstacleLayer))
		{
			Vector3 right = Quaternion.Euler(0, 90, 0) * moveDir;
			Vector3 left = Quaternion.Euler(0, -90, 0) * moveDir;

			bool rightBlocked = Physics.Raycast(rayOrigin, right, 2f, obstacleLayer);
			bool leftBlocked = Physics.Raycast(rayOrigin, left, 2f, obstacleLayer);

			if (!rightBlocked) avoidDirection = right;
			else if (!leftBlocked) avoidDirection = left;
			else { calculatedVelocity = Vector3.zero; isAvoidingWall = false; return true; }

			avoidTimer = 0.8f;
			isAvoidingWall = true;
			calculatedVelocity = avoidDirection * speed;
			RotateModelTowards(avoidDirection);
			return true;
		}
		return false;
	}

	// 攻撃中などに即座に足を止めたい場合用のメソッド
	public void StopImmediate() { calculatedVelocity = Vector3.zero; }

	// 壁に衝突時モデルの向きを変える（スムーズ回転）
	// instant フラグが true の場合は即時回転（攻撃時に使うなら別オーバーロードで呼べるように）
	private void RotateModelTowards(Vector3 dir, bool instant = false)
	{
		if (normalModel == null) return;
		Quaternion targetRot  = Quaternion.LookRotation(dir);

		if (instant) //即時回転
		{
			normalModel.transform.rotation = targetRot;
			if (reasonModel != null) reasonModel.transform.rotation = targetRot;
		}
		else
		{
			// スムーズ回転（補間速度は要調整）
			float rotSpeed = 10f;
			normalModel.transform.rotation = Quaternion.Slerp(
			normalModel.transform.rotation, targetRot, rotSpeed * Time.deltaTime);

			if (reasonModel != null) reasonModel.transform.rotation = Quaternion.Slerp(
			reasonModel.transform.rotation, targetRot, rotSpeed * Time.deltaTime);
		}
	}

	private Vector3 MoveByOrder(CPUOrder order, Transform target, float speed)
	{
		// 基本方向
		if (target == null)
		{
			wanderTimer -= Time.deltaTime;
			if (wanderTimer <= 0f || (wanderTarget - transform.position).magnitude < 1f)
			{
				Vector3 randomOffset = Random.insideUnitSphere * roamRadius;
				randomOffset.y = 0f;
				wanderTarget = transform.position + randomOffset;
				wanderTimer = wanderInterval;
			}
			Vector3 toWander = (wanderTarget - transform.position).normalized;
			toWander.y = 0f;
			return toWander * speed;
		}

		Vector3 targetDir = (target.position - transform.position).normalized;
		targetDir.y = 0;

		switch (order)
		{
			case CPUOrder.Attack: return targetDir * speed;
			case CPUOrder.Retreat:
				{
					Transform nut = searcher.SearchNut();//木の実捜索

					if (nut != null) return (nut.position - transform.position).normalized * speed;
					return -targetDir * speed;
				}
			case CPUOrder.EscapeDeadZone://Deadゾーンからステージ中心へ逃げる
				{
					// 中央（0,0,0）ではなく、少しランダムな位置（中央から半径3m以内）を目指させる
					Vector3 randomOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
					return (randomOffset - transform.position).normalized * speed;
				}
			//様子見中はふらつく
			case CPUOrder.WatchOut: return transform.right * (Mathf.Sin(Time.time) * speed * 0.5f);
			default: return Vector3.zero;// どの case にも入らなかった保険
		}
	}
}