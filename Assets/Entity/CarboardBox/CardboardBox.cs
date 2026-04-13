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

    private static readonly float cleanupTimer = 0.5f;
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
    public override void Death(bool play_audio = true)
    {
        SetDeadState(true);
        StartCoroutine(DelayDeathCoroutine());
    }
    /// <summary>
    ///     Coroutine used for delaying box death
    /// </summary>
    /// <returns>Wait</returns>
    IEnumerator DelayDeathCoroutine()
    {
        yield return new WaitForSeconds(deathTimer + Random.Range(-deathTimerRandom, deathTimerRandom));
        
        // Play visuals
        RunDeathVisuals();
        // Play Effects
        PlayAudio(EffectState.Death);
        PlayVFX(EffectState.Death);

        if (hasReward)
        {
            hasReward = false;
            InventoryHandler.Instance.RewardRandomCollectible();
            InventoryHandler.Instance.AddObjectiveProgress(comboObjective_BreakKey);
        }

        yield return new WaitForSeconds(cleanupTimer);

        CleanupBox();
    }

    public virtual void CleanupBox()
    {
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
            Kill("CarboardBox.SpeedDeath");
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
    public void FlattenBox()
    {
        boxRenderer.gameObject.SetActive(false);
        GetCollision().enabled = false;

        flattenedRenderer.gameObject.SetActive(true);
        gameObject.layer = 10;
    }

    public void UnflattenBox()
    {
        boxRenderer.gameObject.SetActive(true);
        GetCollision().enabled = true;

        flattenedRenderer.gameObject.SetActive(true);
        gameObject.layer = 9;
    }

    public void SetDeadState(bool state) { boxDead = state; }
    public bool isBoxDead() { return boxDead; }
    #endregion
}
