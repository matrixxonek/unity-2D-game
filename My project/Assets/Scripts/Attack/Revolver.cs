using UnityEngine;
using UnityEngine.InputSystem; 


public class Revolver : Gun
{
    public override void Attack()
    {
        base.Attack();

    }
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Attack();
        }
    }
}
