using UnityEngine;
using CharaType = CharacterType;
using A_CT = NormalAttack_CoolTime;

public class Character_CPU : Character_Status
{
	[Header("CPUê›íË")]
	public Transform targetEnemy;   //ë_Ç§PlayerÇÃTransform

	private Rigidbody rb;
	private Animator animator;
	private NutsEffectManager nuts;
	private float lastAttackTime;

	private void Awake()
	{
		TryGetComponent(out rb);
		TryGetComponent(out animator);
		TryGetComponent(out nuts);
	}
	
	public bool isInsideDamageZone = false; // ç°ÉfÉbÉhÉ]Å[ÉìÇ…Ç¢ÇÈÇ©
	private void OnTriggerEnter(Collider other){
		if (other.CompareTag("DamageZone")) isInsideDamageZone = true;}

	private void Update()
	{
		if (CurrentHP <= 0 || CurrentReason <= 0 || targetEnemy == null) return;
		CPUOrder currentOrder = CPUBrain.Think(this, nuts);
		ExecuteMove(currentOrder);
		HandleAttack(currentOrder);
	}

	//à⁄ìÆÇ∑ÇÈÇæÇØÇÃèàóù
	private void ExecuteMove(CPUOrder order)
	{
		Vector3 directionToTarget = (targetEnemy.position - transform.position).normalized;
		directionToTarget.y = 0;
		Vector3 moveVelocity = Vector3.zero;

		switch (order)
		{
			case CPUOrder.Attack: moveVelocity = directionToTarget * CurrentMoveSpeed; break;
			case CPUOrder.Retreat: moveVelocity = -directionToTarget * CurrentMoveSpeed; break;
			case CPUOrder.EscapeDeadZone:
				Vector3 centerDirection = (Vector3.zero - transform.position).normalized;
				centerDirection.y = 0;
				moveVelocity = centerDirection * CurrentMoveSpeed; break;
		}
		rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

		if (moveVelocity.magnitude > 0.1f)
		{
			animator.SetInteger("State", 1);//move
			transform.rotation = Quaternion.LookRotation(moveVelocity);
		}
		else animator.SetInteger("State", 0);//idle
	}

	private void HandleAttack(CPUOrder order)
	{
		if (order != CPUOrder.Attack) return;
		if (Vector3.Distance(transform.position, targetEnemy.position) > 2.0f) return;

		float cooldown = GetNormalAttackCoolTime(CharaAnim);

		if (Time.deltaTime - lastAttackTime < cooldown) return;
		lastAttackTime = Time.time;
		animator.SetTrigger("Attack");
		Debug.Log($"CPUÇ™çUåÇÇµÇ‹ÇµÇΩÅI");
	}

	private float GetNormalAttackCoolTime(CharaType type)
	{
		switch (type)
		{
			case CharaType.LION:		 return A_CT.LION_ATTACK_CT;
			case CharaType.OSTRICH:		 return A_CT.OSTRICH_ATTACK_CT;
			case CharaType.RHINOCELOS:	 return A_CT.RHINOCELOS_ATTACK_CT;
			case CharaType.RATEL:		 return A_CT.RATEL_ATTACK_CT;
			default:					 return 1.0f;
		}
	}
}
