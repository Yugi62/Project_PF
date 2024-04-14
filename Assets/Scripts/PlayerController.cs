using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActions action;
    private InputAction move_Action;

    private void Awake()
    {
        action = new InputActions();
        move_Action = action.Player.Move;
    }   

    private void FixedUpdate()
    {
        Vector2 vec = move_Action.ReadValue<Vector2>();
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
       this.transform.position = new Vector2(
           this.transform.position.x + (vec.x * Player.player.movement_Speed),
           this.transform.position.y + (vec.y * Player.player.movement_Speed));

        if (vec.x < 0)
            this.transform.localScale = new Vector3(-1, 1, 1);
        else if(vec.x > 0)
            this.transform.localScale = new Vector3(1, 1, 1);
    }
}
