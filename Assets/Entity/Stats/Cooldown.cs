using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cooldown
{
    MonoBehaviour mono = null;

    float basic = 0;
    float reductionRate = 0;

    float timer = 0;
    bool locked = false;
    bool canceled = false;
    bool paused = false;

    public delegate void CooldownState();
    /// <summary>
    ///     Runs when the cooldown starts
    /// </summary>
    public event CooldownState OnCooldownStarted;
    /// <summary>
    ///     Runs while cooling down
    /// </summary>
    public event CooldownState OnCooldownUpdate;
    /// <summary>
    ///     Runs when the cooldown is paused
    /// </summary>
    public event CooldownState OnCooldownPaused;
    /// <summary>
    ///     Runs when the cooldown completes without being canceled
    /// </summary>
    public event CooldownState OnCooldownSuccess;
    /// <summary>
    ///     Runs when the cooldown ends
    /// </summary>
    public event CooldownState OnCooldownEnded;
    /// <summary>
    ///     Runs when the cooldown is canceled
    /// </summary>
    public event CooldownState OnCooldownCanceled;

    /// <summary>
    ///     Constructor for cooldown
    /// </summary>
    /// <param name="mono_parent">MonoBehaviour that is used to parse coroutines</param>
    /// <param name="basic">The basic time for cooldown</param>
    /// <param name="reductionRate">The reduction rate for the cooldown</param>
    public Cooldown(MonoBehaviour mono_parent, float basic, float reductionRate)
    {
        mono = mono_parent;
        SetRates(basic, reductionRate);
    }

    #region Get Methods
    /// <summary>
    ///     Method to determine if the cooldown is avaliable
    /// </summary>
    /// <returns>True if its off cooldown</returns>
    public bool Avaliable()
    {
        return timer <= 0 && !Active();
    }

    /// <summary>
    ///     Checks if the cooldown is running
    /// </summary>
    /// <returns>Locked state</returns>
    public bool Active()
    {
        return locked;
    }

    /// <summary>
    ///     Gets the percentage complete
    /// </summary>
    /// <returns>A value between 0 and 1 that shows how far through the cooldown is. 1 being finished</returns>
    public float GetPercentComplete()
    {
        return Mathf.Clamp01((basic - timer) / basic);
    }
    #endregion
    #region Set Methods
    /// <summary>
    ///     Sets the rates for the cooldown
    /// </summary>
    /// <param name="basic"></param>
    /// <param name="reductionRate"></param>
    public void SetRates(float basic, float reductionRate)
    {
        this.basic = basic;
        this.reductionRate = Mathf.Clamp(reductionRate, 0.01f, float.MaxValue);
    }
    /// <summary>
    ///     Resets the timer for the cooldown
    /// </summary>
    public void ResetTimer()
    {
        // Reset timer
        timer = basic;
    }
    #endregion

    #region Math
    /// <summary>
    ///     Adds the to the timer
    /// </summary>
    /// <param name="value">Amount of time added</param>
    public void AddTime(float value)
    {
        timer += value;
    }
    #endregion
    #region Events
    private Coroutine activeRoutine = null;

    /// <summary>
    ///     Starts the cooldown, can have a delay
    /// </summary>
    public void Start(float delay = 0)
    {
        // Check if monobehavour is set properly
        if (mono == null)
            return;
        // Check if the coroutine is locked
        if (Active())
            return;
        // Check if the coroutine is paused, if so pass off to unpause
        if (paused)
        {
            Unpause();
            return;
        }


        // Stops the active routine
        StopActiveRoutine();
        // Reset the cooldown
        ResetTimer();
        // Start the cooldown
        activeRoutine = mono.StartCoroutine(Cooldown_Enum(delay));
    }
    /// <summary>
    ///     Cancels the cooldown
    /// </summary>
    public void Cancel()
    {
        // Set flags
        canceled = true;
        OnCooldownCanceled?.Invoke();
        // Stops the coroutine in its tracks
        timer = 0;

        // Stops the active routine
        StopActiveRoutine();
    }
    /// <summary>
    ///     Pauses the cooldown
    /// </summary>
    public void Pause()
    {
        // Set flag
        paused = true;
        // Stops the active routine
        StopActiveRoutine();
        // Run event
        OnCooldownPaused?.Invoke();
    }
    /// <summary>
    ///     Unpauses the cooldown, no delay allowed here
    /// </summary>
    private void Unpause()
    {
        // Set flag
        paused = false;

        // Continue the cooldown
        activeRoutine = mono.StartCoroutine(Cooldown_Enum());
    }
    #endregion
    #region Coroutine
    /// <summary>
    ///     The main logic for a timed cooldown
    /// </summary>
    /// <param name="delay">Defined delay, runs between flags and on cooldown started</param>
    /// <returns>Wait</returns>
    private IEnumerator Cooldown_Enum(float delay = 0)
    {
        // Lock the coroutine
        locked = true;
        canceled = false;

        // Run delay
        yield return new WaitForSeconds(delay);


        // Invoke event
        OnCooldownStarted?.Invoke();

        // Start looping the timer
        while (timer > 0)
        {
            // Reduce the timer
            timer -= Time.deltaTime * reductionRate;
            OnCooldownUpdate?.Invoke();
            yield return new WaitForEndOfFrame();
        }

        // Runs only if the coroutine is successful
        if(!canceled)
            OnCooldownSuccess?.Invoke();

        // Runs no matter how the coroutine ends
        OnCooldownEnded?.Invoke();
        // Unlock the coroutine
        locked = false;
    }


    /// <summary>
    ///     Stops the active routine
    /// </summary>
    private void StopActiveRoutine()
    {
        // Try cancel an active routine
        if (activeRoutine == null)
            return;

        // Stops the routine
        mono.StopCoroutine(activeRoutine);
        // Unlocks the routine
        locked = false;
    }
    #endregion
}
