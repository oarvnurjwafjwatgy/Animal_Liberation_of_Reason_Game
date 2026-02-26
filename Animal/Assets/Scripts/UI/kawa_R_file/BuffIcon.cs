using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
	private Image iconImage;
	private float remainingTime;
	private const float BLINK_START_TIME = 3.0f; // 残り3秒で点滅開始

	public void Setup(Sprite sprite, float duration)
	{
		iconImage = GetComponent<Image>();
		iconImage.sprite = sprite;
		remainingTime = duration;
		// 点滅中に新しいバフを受けた時のために色を戻す
		if (iconImage != null) iconImage.color = Color.white;
	}

	void Update()
	{
		remainingTime -= Time.deltaTime;

		if (remainingTime <= BLINK_START_TIME)
		{
			// 残り時間が少なくなったらチカチカさせる
			float alpha = Mathf.Abs(Mathf.Sin(Time.time * 10f));
			iconImage.color = new Color(1, 1, 1, alpha);
		}

		if (remainingTime <= 0)
		{
			Destroy(gameObject); // 時間が来たら自分を消す
		}
	}


	//手動でバフアイコンを消すための関数（例：バフが解除されたとき）
	public void ForceDestroy()
	{
		Destroy(gameObject);
	}
}