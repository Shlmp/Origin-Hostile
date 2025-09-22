using UnityEngine;

[RequireComponent (typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private MyActions actions;
    public float speed = 1f;

    private GrabEnemy grabEnemy;
    public GameObject enemy;

    private void Start()
    {
        gameObject.TryGetComponent(out controller);

        actions = new MyActions();
        actions.Gameplay.Enable();

        gameObject.TryGetComponent(out grabEnemy);
        grabEnemy = gameObject.GetComponentInChildren<GrabEnemy>();
    }

    private void Update()
    {
        Vector2 input = actions.Gameplay.Move.ReadValue<Vector2>();

        controller.Move(new Vector3(input.x, 0, input.y) * Time.deltaTime * speed);

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<BobSPA>())
        {
            enemy = other.gameObject;
            Debug.Log("enemy = " + enemy + "  --  other = " + other.gameObject);
            Debug.Log("grabEnemy = " + grabEnemy);
            grabEnemy.Interact();
        }
    }
}
