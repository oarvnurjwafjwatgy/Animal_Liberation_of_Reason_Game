using UnityEngine;
using UnityEngine.InputSystem;


// Rigidbody���K�v�ł��邱�Ƃ�����
[RequireComponent(typeof(Rigidbody))]

public class Controller : MonoBehaviour
{
    public PlayerInput PlayerInput;

    private Vector2 InputValueLeftStick;
    private Vector2 InputValueRightStick ;

    private void Awake()
    {
        TryGetComponent(out PlayerInput);
    }


	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        InputValueLeftStick = PlayerInput.actions["Move"].ReadValue<Vector2>();
        InputValueRightStick = PlayerInput.actions["Look"].ReadValue<Vector2>();
    }

    //���t�g�X�e�B�b�N�̌X���̎擾
    public Vector2 GetLeftStick()
    {
        return InputValueLeftStick;
    }

    public Vector2 GetRightStick()
    {
        return InputValueRightStick;
    }

}
