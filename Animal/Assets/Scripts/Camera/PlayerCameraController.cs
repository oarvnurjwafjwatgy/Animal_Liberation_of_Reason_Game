using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
	private GameObject cameraObject;
	private GameObject ghostObject;

	/// <summary>
	/// 初期化：InputPlayerからカメラオブジェクトの参照を受け取ります
	/// </summary>
	public void Initialize(GameObject cameraObj, GameObject ghostObj)
	{
		cameraObject = cameraObj;
		ghostObject = ghostObj;
	}

	// 通常時のカメラ更新（元の処理をそのままコピペ）
	public void UpdateCamera(Vector2 rightStickInput, Transform playerTransform)
	{
		// ControllerクラスからRスティックの入力値を取得
		//（※引数 rightStickInput で受け取る形にしています）

		// カメラの横移動
		if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
		{
			cameraObject.transform.RotateAround(playerTransform.position, Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
		}
		// カメラの縦移動
		float camera_angle_x = cameraObject.transform.localEulerAngles.x;
		//Debug.Log(camera_angle_x);
		if (rightStickInput.y > 0.25f && (camera_angle_x >= 0f && camera_angle_x < 180f))
		{
			// 下移動
			cameraObject.transform.RotateAround(playerTransform.position, cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
		}
		if (rightStickInput.y < -0.25f && (camera_angle_x < 60f || camera_angle_x <= 360f && camera_angle_x > 180f))
		{
			// 上移動
			cameraObject.transform.RotateAround(playerTransform.position, cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
		}
	}

	// 観戦モード時のカメラ更新（元の処理をそのままコピペ＋フリーズ対策）
	public void UpdateGhostCamera(Vector2 rightStickInput, Transform playerTransform, ref Quaternion cachedRotate)
	{
		// 死亡時に動かなくなっていた原因（cachedRotateの固定）を解消するため、
		// 横回転したときに、InputPlayer側のcachedRotateも一緒に回るように追記しています。

		// カメラの横移動
		if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
		{
			ghostObject.transform.RotateAround(playerTransform.position, Vector3.up, rightStickInput.x * Time.deltaTime * 200f);

			// Rスティックの横入力に応じて、InputPlayerの向き基準（cachedRotate）も同期させてフリーズを解除
			cachedRotate *= Quaternion.Euler(0, rightStickInput.x * Time.deltaTime * 200f, 0);
		}
		// カメラの縦移動
		float camera_angle_x = ghostObject.transform.localEulerAngles.x;
		//Debug.Log(camera_angle_x);
		if (rightStickInput.y > 0.25f && (camera_angle_x > 280f || (camera_angle_x >= 0f && camera_angle_x < 180f)))
		{
			// 下移動
			ghostObject.transform.RotateAround(playerTransform.position, ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
		}
		if (rightStickInput.y < -0.25f && (camera_angle_x < 80f || (camera_angle_x <= 360f && camera_angle_x > 180f)))
		{
			// 上移動
			ghostObject.transform.RotateAround(playerTransform.position, ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
		}
	}

	// カメラリセット（元の処理をそのままコピペ）
	public void ResetCamera(GameObject activeModel)
	{
		cameraObject.transform.position = activeModel.transform.position + new Vector3(0f, 1f, 0f) + activeModel.transform.forward * -3f;
		cameraObject.transform.rotation = activeModel.transform.rotation;
	}
}