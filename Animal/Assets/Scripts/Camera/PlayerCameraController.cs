using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
	private GameObject cameraObject;
	private GameObject ghostObject;

	// 初期化：InputPlayerからカメラオブジェクトの参照を受け取ります
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
			cameraObject.transform.RotateAround(playerTransform.position,
			Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
		}
		// カメラの縦移動
		float camera_angle_x = cameraObject.transform.localEulerAngles.x;

		if (rightStickInput.y > 0.25f && (camera_angle_x >= 0f && camera_angle_x < 180f))
		{
			cameraObject.transform.RotateAround(playerTransform.position,
			cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);// 下移動
		}

		if (rightStickInput.y < -0.25f && (camera_angle_x < 60f ||
		camera_angle_x <= 360f && camera_angle_x > 180f))
		{
			cameraObject.transform.RotateAround(playerTransform.position,
			cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);// 上移動
		}
	}

	// 観戦モード時のカメラ更新
	public void UpdateGhostCamera(Vector2 rightStickInput, Transform playerTransform, ref Quaternion cachedRotate)
	{
		// カメラの横移動
		if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
		{
			ghostObject.transform.RotateAround(playerTransform.position,
			Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
			cachedRotate *= Quaternion.Euler(0, rightStickInput.x * Time.deltaTime * 200f, 0);
		}
		// カメラの縦移動
		float camera_angle_x = ghostObject.transform.localEulerAngles.x;

		if (rightStickInput.y > 0.25f && (camera_angle_x > 280f ||
		(camera_angle_x >= 0f && camera_angle_x < 180f)))
		{
			ghostObject.transform.RotateAround(playerTransform.position, 
			ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);// 下移動
		}
		if (rightStickInput.y < -0.25f && (camera_angle_x < 80f ||
		(camera_angle_x <= 360f && camera_angle_x > 180f)))
		{
			ghostObject.transform.RotateAround(playerTransform.position,
			ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);// 上移動
		}
	}

	// カメラリセット
	public void ResetCamera(GameObject activeModel)
	{
		cameraObject.transform.position = activeModel.transform.position + new Vector3(0f, 1f, 0f) + activeModel.transform.forward * -3f;
		cameraObject.transform.rotation = activeModel.transform.rotation;
	}
}