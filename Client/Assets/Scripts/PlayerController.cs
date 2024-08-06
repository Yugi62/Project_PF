using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    private InputActions action;
    private InputAction move_Action;
    private Transform spriteTransform;

    private void Awake()
    {
        action = new InputActions();
        move_Action = action.Player.Move;
        spriteTransform = GetComponentInChildren<SpriteRenderer>().gameObject.transform;
    }   

    private void FixedUpdate()
    {
        Vector2 vec = move_Action.ReadValue<Vector2>();

        if (!inputField.isFocused)
            MovePlayer(vec);
    }    
    
    private void OnEnable()
    {
        move_Action.Enable();
    }

    private void OnDisable()
    {
        move_Action.Disable();
    }

    private void MovePlayer(Vector2 vec)
    {
       transform.position = new Vector2(
           transform.position.x + (vec.x * Player.player.movement_Speed),
           transform.position.y + (vec.y * Player.player.movement_Speed));

        if (vec.x < 0)
            spriteTransform.localScale = new Vector3(-1, 1, 1);
        else if(vec.x > 0)
            spriteTransform.localScale = new Vector3(1, 1, 1);
    }
}
