using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActions action;
    private InputAction move_Action;
    private Player player;

    private void Awake()
    {
        action = new InputActions();
        move_Action = action.Player.Move;
    }   
    
    private void Start()
    {
        player = GetComponent<Player>();
    }
    
    private void Update()
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
           this.transform.position.x + (vec.x * player.movement_Speed),
           this.transform.position.y + (vec.y * player.movement_Speed));
    }
}
