using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CardboardBox : EntityData
{
    [Header("Cardboard Box")]
    [SerializeField] private bool active = true;
    [SerializeField] private bool hasReward = true;
    [Space]
    [SerializeField] private float speedDeathThreshold = 0.8f;
    private static readonly float deathTimer = 0.25f;
    private static readonly float deathTimerRandom = 0.075f;

    private static readonly float cleanupTimer = 3;
    private static readonly float cleanupVelocityThreshold = 0.025f;
    private bool boxDead = false;

    [Space]
    [SerializeField] private MeshRenderer boxRenderer;
    [SerializeField] private MeshRenderer flattenedRenderer;
    [Space]
    [SerializeField] private VisualEffect peanutEffect;

    private static readonly string comboObjective_BreakKey = "Break Cardboard";

    #region Unity Methods
    private void FixedUpdate()
    {
        if (!active)
            return;

        TickObjectHandling();
    }
    #endregion
    #region EntityData Events
    public override void OnEnabled()
    {
        base.OnEnabled();
    }
    public override void OnDeath()
    {
        boxDead = true;
        StartCoroutine(DelayDeathCoroutine());
    }
    /// <summary>
    ///     Coroutine used for delaying box death
    /// </summary>
    /// <returns>Wait</returns>
    IEnumerator DelayDeathCoroutine()
    {
        yield return new WaitForSecondsRealtime(deathTimer + Random.Range(-deathTimerRandom, deathTimerRandom));
        RunDeathVisuals();

        if (hasReward)
        {
            InventoryHandler.Instance.RewardRandomCollectible();
            InventoryHandler.Instance.AddObjectiveProgress(comboObjective_BreakKey);
        }

        yield return new WaitForSecondsRealtime(cleanupTimer);
        //while(GetLinearVelocity().magnitude >= cleanupVelocityThreshold && transform.position.y > -250)
            //yield return new WaitForEndOfFrame();
        //GetRigidbody().isKinematic = true;
        //GetCollision().enabled = false;
        gameObject.SetActive(false);
    }
    #endregion

    #region Object Handling
    /// <summary>
    ///     Updates object handling methods
    /// </summary>
    private void TickObjectHandling()
    {
        CheckSpeedDeath();
    }
    
    /// <summary>
    ///     Checks if we are dead due to a speed death
    /// </summary>
    private void CheckSpeedDeath()
    {
        // Check if we are already dead
        if (boxDead)
            return;
        // Kill the box
        if (GetLinearVelocity().magnitude >= speedDeathThreshold)
            Hurt("CarboardBox.SpeedDeath", 9999);
    }
    #endregion
    #region Visual Handling
    /// <summary>
    ///     Organizes all death visuals
    /// </summary>
    private void RunDeathVisuals()
    {
        PlayPeanutBurst();
        FlattenBox();
    }
    /// <summary>
    ///     Sends event for peanut burst
    /// </summary>
    private void PlayPeanutBurst()
    {
        peanutEffect.SendEvent("OnPeanut");
    }
    private void FlattenBox()
    {
        boxRenderer.gameObject.SetActive(false);
        GetCollision().enabled = false;

        flattenedRenderer.gameObject.SetActive(true);
        gameObject.layer = 10;
    }
    #endregion
}
