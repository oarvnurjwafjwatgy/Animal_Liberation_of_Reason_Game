/*あくまでもここはデータのみの情報だけなので
 * MonoBehaviourというオブジェクトに張り付けるだけのものを削除*/

public struct InputData
{
	//定数
	public const float GHOST_MOVE_SPEED = 15f;
	
	//向き
	public enum Direction
	{
		Front,
		Right,
		Left,
		Back,
	}
}