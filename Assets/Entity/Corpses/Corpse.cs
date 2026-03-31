using UnityEngine;

public class Corpse : MonoBehaviour
{
    [SerializeField] private Transform eyes;
    private Cooldown break_timer;
    private static readonly float bt_time = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        break_timer = new Cooldown(this, bt_time, 1);

        break_timer.OnCooldownSuccess += BreakCorpse;

        break_timer.Start();
    }
    private void OnDestroy()
    {
        break_timer.OnCooldownSuccess -= BreakCorpse;
    }

    private void BreakCorpse()
    {
        Destroy(gameObject);
    }

    public void SetEyes(Transform other)
    {
        eyes.rotation = other.rotation;
    }
}
