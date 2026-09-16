using HFHandyUtils.Physics;
using UnityEngine;

public class SE_GroundEffect : SpawnableEffect
{
    private static float s_GroundDetectionHeight = 0.75f;

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        OnEntityTracking_Added += entity => 
        {
            Quaternion rot = Quaternion.FromToRotation(transform.up, Vector3.up);
            float distance = (rot * entity.transform.position).y - (rot * transform.position).y;
            distance = Mathf.Abs(distance);

            if(Physics.Raycast(entity.transform.position, -transform.up, distance + 0.05f))
                HitGround(entity, GetStatblock());
        };
    }


    public override void OnSetHitbox(float size)
    {
        Vector3 decalSize = GetDecalProjector().size;
        hitbox = new Hitbox_Cube(transform)
        {
            size = new Vector3(decalSize.x, s_GroundDetectionHeight, decalSize.y),
            localEuler = transform.localEulerAngles
        };
        hitbox.SetOffset(transform.up * (s_GroundDetectionHeight / 2f));
    }
    #endregion

    /// <summary>
    ///     Method run when the entity hits the ground
    /// </summary>
    /// <param name="entity">Entity hit</param>
    /// <param name="stats">Passthrough stats</param>
    public virtual void HitGround(EntityData entity, Statblock stats) { }
}
