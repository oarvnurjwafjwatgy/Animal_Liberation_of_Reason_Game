using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Rigidbody���K�v�ł��邱�Ƃ�����
[RequireComponent(typeof(Rigidbody))]

public class Controller : MonoBehaviour
{
    public PlayerInput playerInput;

    private Vector2 input_value;

    private void Awake()
    {
        TryGetComponent(out playerInput);
    }


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    { 

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        input_value = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    //���t�g�X�e�B�b�N�̌X���̎擾
    public Vector2 GetLeftStick()
    {
        return input_value;
    }
}
