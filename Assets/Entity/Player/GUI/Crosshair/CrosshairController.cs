using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    // Singleton
    private static CrosshairController _instance;
    public static CrosshairController Instance { get { return _instance; } }

    #region Data Types
    public enum CrosshairType { None, Main, Hit, Hurt, Heal };
    [System.Serializable]
    private class CrosshairLayer
    {
        [SerializeField] private CrosshairType type;
        [SerializeField] private Transform parent;
        [Space]
        [SerializeField] private float alive_time = 0.1f;
        private Cooldown cooldown;

        public CrosshairLayer(CrosshairType type, Transform parent, MonoBehaviour mono, float alive_time)
        {
            this.type = type;
            this.parent = parent;
            cooldown = new Cooldown(mono, alive_time, 1);
        }

        // Sequencing
        public void Enable(MonoBehaviour mono) 
        {
            cooldown = new Cooldown(mono, alive_time, 1);

            cooldown.OnCooldownStarted += () => SetParentActive(true);
            cooldown.OnCooldownEnded += () => SetParentActive(false);

            SetParentActive(false);
        }
        public void Disable(MonoBehaviour mono)
        {
            cooldown.OnCooldownStarted += () => SetParentActive(true);
            cooldown.OnCooldownEnded += () => SetParentActive(false);

            SetParentActive(false);
        }

        public void Trigger() 
        {
            if (!cooldown.Active())
                cooldown.Start();
            else
                cooldown.AddTime(alive_time);
        }

        // Get Basic
        public CrosshairType GetCrosshairType() { return type; }
        public Transform GetParent() { return parent; }
        public Cooldown GetCooldown() { return cooldown; }

        // Set Methods
        public void SetParentActive(bool state) { parent.gameObject.SetActive(state); }
        public void SetParentRotation(float rotation) { parent.eulerAngles = Vector3.forward * rotation; }
    }
    #endregion

    [SerializeField] private CrosshairLayer[] crosshairs;

    #region Singleton
    private void CreateSingleton()
    {
        // -> Pulled from Out on the Red Sea
        // Checks if the instance of object is first of its type
        // If object is not unique, destroy current instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        // Declares this script as current
        else
        {
            _instance = this;
        }
    }
    #endregion
    #region Unity Methods
    private void Awake()
    {
        CreateSingleton();
        SetupCrosshairs();
    }
    private void OnDestroy()
    {
        BreakCrosshairs();
    }
    #endregion

    #region Cooldown Handling
    private void SetupCrosshairs()
    {
        for (int i = 0; i < crosshairs.Length; i++)
            crosshairs[i].Enable(this);
    }
    private void BreakCrosshairs()
    {
        for (int i = 0; i < crosshairs.Length; i++)
            crosshairs[i].Disable(this);
    }
    #endregion
    #region Crosshair Handling
    private int GetCrosshairIndex(CrosshairType type)
    {
        for(int i = 0; i < crosshairs.Length; i++)
        {
            if (crosshairs[i].GetCrosshairType().Equals(type))
                return i;
        }
        return -1;
    }

    public void RequestCrosshair(CrosshairType type)
    {
        RequestCrosshair(type, 0);
    }
    public void RequestCrosshair(CrosshairType type, float rotation)
    {
        if (type.Equals(CrosshairType.None))
            return;

        int index = GetCrosshairIndex(type);

        // Check if index is in range
        if (index < 0 || index >= crosshairs.Length)
            return;

        // Trigger Crosshair
        crosshairs[index].Trigger();
        crosshairs[index].SetParentRotation(rotation);
    }
    #endregion
}
