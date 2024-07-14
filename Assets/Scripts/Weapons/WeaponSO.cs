using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon")]
public class WeaponSO : ScriptableObject
{
    public enum WeaponType
    {
        NormalShot,
        BurstShot,
        ShotgunShot
    }

    [Header("Weapon Configuration")]
    public WeaponType weaponType;
    public float rateOfFire;
    public float reloadTime;
    public int ammo;
    public int reserveAmmo;
    public int shotsPerClick;

    [Header("Spread")]
    public float spread;
    [Range(0f, 1f)]
    public float spreadXMultiplier;
    [Range(0f, 1f)]
    public float spreadYMultiplier;
    [Range(0f, 1f)]
    public float spreadZMultiplier;

    [Header("Projectile")]
    public GameObject projectile;
    public float projectileVelocity;
    public bool useGravity;

    [Header("UI")]
    public Sprite crosshair;
    public Material material;
    public float size;

    [Header("Audio")]
    //--- Turn into lists ---
    public List<AudioClip> weaponSound = new List<AudioClip>();
    public List<AudioClip> reloadSound = new List<AudioClip>();
    [Range(0f, 1f)]
    public float upperVolume;
    [Range(0f, 1f)]
    public float lowerVolume;
    [Range(-3f, 3f)]
    public float upperPitch;
    [Range(-3f, 3f)]
    public float lowerPitch;
}
