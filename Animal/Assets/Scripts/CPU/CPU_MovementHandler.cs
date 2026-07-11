using UnityEngine;

public class CPU_MovementHandler : MonoBehaviour
{
	[Header("移動設定")]
	[SerializeField] private float stopDistance = 2.2f;      // 相手を押し付けないように立ち止まる距離

	private Rigidbody rb;
	private Vector3 calculatedVelocity = Vector3.zero;
	private GameObject normalModel;
	private GameObject reasonModel;

	// 大元や他のコンポーネントから現在の計算速度を参照できるようにするプロパティ
	public Vector3 CalculatedVelocity => calculatedVelocity;

	// 追加：外から「今どの方向を目指しているか」を取り出せるようにする
	public Vector3 CurrentMoveDirection { get; private set; }

	//初期化:Character_CPUからRigidbodyを受け取る
	public void Initialize(Rigidbody rigidbody) { rb = rigidbody; }

	public void SetModels(GameObject normal, GameObject reason)
	{
		normalModel = normal;
		reasonModel = reason;
	}

	public void CalculateMoveVelocity(Transform target, CPUOrder order, float speed)
	{
		if (target == null) { calculatedVelocity = Vector3.zero; return; }

		// 立ち止まり距離の内側にいるなら速度を0にする
		float currentDistance = Vector3.Distance(transform.position, target.position);
		if (currentDistance <= stopDistance) { calculatedVelocity = Vector3.zero; return; }

		Vector3 directionToTarget = (target.position - transform.position).normalized;
		directionToTarget.y = 0;
		Vector3 moveVelocity = Vector3.zero;
		// 追加：計算した方向を保存
		CurrentMoveDirection = directionToTarget;

		switch (order)
		{
			case CPUOrder.Attack: moveVelocity = directionToTarget * speed; break;
			case CPUOrder.Retreat: moveVelocity = -directionToTarget * speed; break;
			case CPUOrder.EscapeDeadZone:
				Vector3 centerDirection = (Vector3.zero - transform.position).normalized;
				centerDirection.y = 0;
				moveVelocity = centerDirection * speed; break;
		}
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

	// 攻撃中などに即座に足を止めたい場合用のメソッド
	public void StopImmediate() { calculatedVelocity = Vector3.zero; }
}