using UnityEngine;

public class AnimeReplay : MonoBehaviour
{
    private InputPlayer playerInput;

    void Start()
    {
        // 親（または親の親）に付いている InputPlayer を探して取得
        playerInput = GetComponentInParent<InputPlayer>();
    }

    // アニメーションイベントから呼び出す関数
    public void OnMoveResume()
    {
        if (playerInput != null)
        {
            playerInput.MoveFlagFalse(); // MoveFlag を true に戻す
        }
    }


    public void OnDeath()
    {
        playerInput.SetDeath();
    }

    public void ResetRatel()
    {
        playerInput.ResetRatelSkillParam();
    }
}