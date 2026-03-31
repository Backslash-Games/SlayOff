using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class WV_Stamp : WeaponVisual
{
    [SerializeField] private LineRenderer scan_line = null;
    [SerializeField] private float line_time = 0.1f;
    private Cooldown line_cooldown;
    [Space]
    [SerializeField] private GameObject stamp = null;
    [SerializeField] private SpriteRenderer stamp_sprite = null;
    [SerializeField] private Sprite[] stamp_sprites = null;
    [SerializeField] private LayerMask stamp_layers;
    [SerializeField] private float stamp_alive_time = 5;
    [SerializeField] private float stamp_fade_time = 5;
    [Space]
    [SerializeField] private VisualEffectAsset dust_vfx;
    [SerializeField] private string dust_event_name = "OnDust";
    private Cooldown stamp_fade_cooldown;

    private void Awake()
    {
        line_cooldown = new Cooldown(this, line_time, 1);
        stamp_fade_cooldown = new Cooldown(this, stamp_fade_time, 1);

        OnInitialization += () => DrawLine(false);
        OnHitSet += SetStamp;
    }
    private void Update()
    {
        if (line_cooldown.Active())
            DrawLine(true);
    }
    private void OnDestroy()
    {
        OnInitialization -= () => DrawLine(false);
        OnHitSet -= SetStamp;
    }

    private void DrawLine(bool ignore_coroutine = false) 
    {
        Vector3[] positions = new Vector3[] { GetNozzle().position, GetHitPosition() };
        //Debug.Log($"Setting positions {positions[0]} || {positions[1]}");
        scan_line.enabled = true;
        scan_line.SetPositions(positions);
        
        if(!ignore_coroutine)
            StartCoroutine(enum_DestroyLine());
    }
    private IEnumerator enum_DestroyLine()
    {
        line_cooldown.Start();
        while (line_cooldown.Active())
            yield return new WaitForEndOfFrame();
        scan_line.enabled = false;

        // Check if the stamp is active
        if (stamp == null || !stamp.activeSelf)
            Destroy(gameObject);
    }

    private void SetStamp()
    {
        RaycastHit hit = GetHit();

        // Check if we activate stamp
        if (((1 << hit.transform.gameObject.layer) & stamp_layers) == 0)
            return;

        stamp.SetActive(true);
        stamp.transform.position = hit.point; 
        stamp.transform.forward = hit.normal;
        stamp.transform.parent = hit.transform;

        stamp_sprite.sprite = stamp_sprites[Random.Range(0, stamp_sprites.Length)];

        if (dust_vfx != null)
            VisualEffectManager.Instance.PlayVisual(dust_vfx, hit.point, 0.5f, dust_event_name);

        StartCoroutine(enum_DestroyStamp());
    }

    private IEnumerator enum_DestroyStamp()
    {
        yield return new WaitForSeconds(stamp_alive_time);

        stamp_fade_cooldown.Start();
        while (stamp_fade_cooldown.Active())
        {
            if (stamp == null)
                break;
            stamp_sprite.color = new Color(stamp_sprite.color.r, stamp_sprite.color.g, stamp_sprite.color.b, 1f - stamp_fade_cooldown.GetPercentComplete());
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
