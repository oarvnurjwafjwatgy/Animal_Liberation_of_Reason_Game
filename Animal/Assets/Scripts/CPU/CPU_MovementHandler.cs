using UnityEditor.Searcher;
using UnityEngine;

public class CPU_MovementHandler : MonoBehaviour
{
	[Header("移動設定")]
	[SerializeField] private float stopDistance = 2.2f;      // 相手を押し付けないように立ち止まる距離
	private LayerMask obstacleLayer;

	private Rigidbody rb;
	private Vector3 calculatedVelocity = Vector3.zero;
	private GameObject normalModel;
	private GameObject reasonModel;

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

	//旧式
	//public void CalculateMoveVelocity(Transform target, CPUOrder order, float speed)
	//{
	//	if (target == null) { calculatedVelocity = Vector3.zero; return; }

	//	float currentDistance = Vector3.Distance(transform.position, target.position);
	//	Vector3 targetDir = (target.position - transform.position).normalized;
	//	targetDir.y = 0;

	//	// 密着を防ぐ（2.5m以上なら突撃、2.2m以内なら立ち止まるか回る）
	//	if (currentDistance < 2.2f)
	//	{
	//		// 攻撃可能な距離なら、少しだけ横に回り込む
	//		calculatedVelocity = transform.right * speed * 0.5f;
	//		return;
	//	}

	//	// 基本の移動方向
	//	Vector3 finalDir = targetDir;
	//	RaycastHit hit;
	//	int layerMask = LayerMask.GetMask("FieldObject");

	//	// 2.0m 前方にレイを飛ばす
	//	if (Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), targetDir, out hit, 2.0f, layerMask))
	//	{
	//		// 壁があれば、ターゲット方向から90度右に逸らすベクトルを作る
	//		finalDir = Quaternion.Euler(0, 90, 0) * targetDir;
	//	}

	//	// 移動速度の計算（orderによる分岐はそのまま）
	//	Vector3 moveVelocity = finalDir * speed;

	//	switch (order)
	//	{
	//		case CPUOrder.Attack: moveVelocity = finalDir * speed; break;
	//		case CPUOrder.Retreat: moveVelocity = -finalDir * speed; break;//ターゲットから逆に逃げる
	//		case CPUOrder.EscapeDeadZone://Deadゾーンからステージ中心へ逃げる
	//			// 中央（0,0,0）ではなく、少しランダムな位置（中央から半径3m以内）を目指させる
	//			Vector3 randomOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
	//			moveVelocity = (randomOffset - transform.position).normalized * speed;
	//			break;
	//		//様子見中はふらつく
	//		case CPUOrder.WatchOut: moveVelocity = transform.right * (Mathf.Sin(Time.time) * speed * 0.5f); break;
	//	}
	//	calculatedVelocity = moveVelocity;
	//}

	public void CalculateMoveVelocity(Transform target, CPUOrder order, float speed)
	{
		// 基本のターゲット方向
		Vector3 targetDir = (target == null)
			? (Vector3.zero - transform.position).normalized
			: (target.position - transform.position).normalized;

		targetDir.y = 0;

		if (HandleWallAvoid(targetDir, speed)) return;

		Vector3 moveVelocity = Vector3.zero;

		if (target != null)
		{
			switch (order)
			{
				case CPUOrder.Attack: moveVelocity = targetDir * speed; break;
				case CPUOrder.Retreat:
					Transform nut = searcher.SearchNut();//木の実捜索

					if (nut != null)
					{
						Vector3 nutDir = (nut.position - transform.position).normalized;
						moveVelocity = nutDir * speed;
					}
					else moveVelocity = transform.right * (Mathf.Sin(Time.time) * speed * 0.5f); break;
				//moveVelocity = -targetDir * speed;
				//break;//ターゲットから逆に逃げる

				case CPUOrder.EscapeDeadZone://Deadゾーンからステージ中心へ逃げる
				 // 中央（0,0,0）ではなく、少しランダムな位置（中央から半径3m以内）を目指させる
					Vector3 randomOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
					moveVelocity = (randomOffset - transform.position).normalized * speed;
					break;
				//様子見中はふらつく
				case CPUOrder.WatchOut: moveVelocity = transform.right * (Mathf.Sin(Time.time) * speed * 0.5f); break;
			}
		}
		else moveVelocity = targetDir * speed; // 中央へ向かう

		calculatedVelocity = moveVelocity;
	}

	// 物理移動と回転:アニメーションの実行
	public void ExecuteFixedUpdate(
	Animator animator,
	bool isAttacking)
	{
		if (rb == null) return;

		// 計算された移動速度をRigidbodyに適用（Y軸の落下速度は維持する）
		rb.velocity = new Vector3(calculatedVelocity.x, rb.velocity.y, calculatedVelocity.z);

		if (animator == null) return;
		if (isAttacking) return;

		if (calculatedVelocity.magnitude > 0.1f) animator.SetInteger("State", 1); // move
		else animator.SetInteger("State", 0); // idle
	}

	private bool HandleWallAvoid(Vector3 targetDir,float speed)
	{
		Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
		Vector3 moveDir = calculatedVelocity.sqrMagnitude > 0.01f
			? calculatedVelocity.normalized
			: targetDir;

		RaycastHit hit;
		bool hitWall = Physics.Raycast(rayOrigin, moveDir, out hit, 2f, obstacleLayer);

		//壁の回避中
		if (isAvoidingWall)
		{
			// 壁を完全に抜けたか？
			bool stillNearWall =
				Physics.Raycast(rayOrigin, avoidDirection, 2f, obstacleLayer) ||
				Physics.Raycast(rayOrigin + transform.right * 0.5f, avoidDirection, 2f, obstacleLayer) ||
				Physics.Raycast(rayOrigin - transform.right * 0.5f, avoidDirection, 2f, obstacleLayer);

			if (!stillNearWall)
			{
				// 壁を抜けたので通常状態へ
				isAvoidingWall = false;
				return false;
			}

			// 壁回避継続
			calculatedVelocity = avoidDirection * speed;
			RotateModelTowards(avoidDirection);//モデルを横向きに
			return true;
		}

		// 通常状態：壁を検知したら回避開始
		if (hitWall)
		{
			avoidDirection = Quaternion.Euler(0, 90, 0) * moveDir;
			isAvoidingWall = true;
			calculatedVelocity = avoidDirection * speed;
			
			// 壁回避開始時にも向きを変える
			RotateModelTowards(avoidDirection);
			return true;
		}
		return false;
	}

	// 攻撃中などに即座に足を止めたい場合用のメソッド
	public void StopImmediate() { calculatedVelocity = Vector3.zero; }

	//壁に衝突時モデルの向きを変える
	private void RotateModelTowards(Vector3 dir)
	{
		if (normalModel == null) return;
		Quaternion rot = Quaternion.LookRotation(dir);
		normalModel.transform.rotation = rot;
		if (reasonModel != null) reasonModel.transform.rotation = rot;
	}

}