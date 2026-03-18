using UnityEngine;

public class tc_MonoControl : MonoBehaviour
{
    [SerializeField] private TimeControl timeControl = null;
    [SerializeField] private float timeScale = 1;

    // Start is called on the first frame
    private void Start()
    {
        timeControl = new TimeControl(this);
    }
    // Update is called once per frame
    void Update()
    {
        timeControl.SetScale(timeScale);
    }
}
