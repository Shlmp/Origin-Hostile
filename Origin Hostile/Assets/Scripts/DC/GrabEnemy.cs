using UnityEngine;

public class GrabEnemy : MonoBehaviour
{
    private CharacterController controller;
    private MyActions actions;

    private PlayerMovement player;
    public GameObject enemy;

    void Start()
    {
        gameObject.TryGetComponent(out controller);

        actions = new MyActions();
        actions.Gameplay.Enable();

        player = gameObject.GetComponent<PlayerMovement>();
    }

    public void Interact()
    {
        //enemy = player.enemy;
        if (actions.Gameplay.Interact.IsPressed())
        {
            Debug.Log("E was pressed");
            Debug.Log("player.enemy = " + enemy);
            Debug.Log("this.transform = " + this.transform);
            enemy.transform.parent = this.transform;
            enemy.transform.position = new Vector3(this.transform.position.x, 1, this.transform.position.z);
            enemy.GetComponent<BobSPA>().enabled = false;
        }
    }
}
