using System.Collections;
using UnityEngine;

public class Elevator_End : Elevator
{
    [SerializeField] private Hitbox_Cube end_trigger;
    [SerializeField] private bool debug_DrawTrigger = false;

    private float teleport_stall = 1;
    private bool teleport_lock = false;

    private void Start()
    {
        end_trigger.SetOffset(transform.rotation * end_trigger.GetOffset());
        end_trigger.SetLocalEuler(transform.eulerAngles);
    }
    private void Update()
    {
        if (!teleport_lock && end_trigger.CheckCollision())
        {
            teleport_lock = true;
            StartCoroutine(enum_EndFloor());
        }
    }

    #region Events
    private IEnumerator enum_EndFloor()
    {
        // Start by closing the doors
        TriggerAnimation();
        yield return new WaitForSeconds(teleport_stall);
        // Teleport the player
        GetArcadeModeManager().WP_MoveToPoint(2, 0);
        // Trigger results
        GetArcadeModeManager().GetArcadeResults().DisplayResults();
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (debug_DrawTrigger)
            end_trigger.DrawGizmos();
    }
    #endregion
}
