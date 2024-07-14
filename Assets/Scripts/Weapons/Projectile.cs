using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    // WILL CHANGE FOR FRIENDLY FIRE
    [SerializeField] private bool ignoreColision;
    [SerializeField] private bool hasSplashDamage;
    [SerializeField] private bool destroyOnImpact;
    //[SerializeField] private LayerMask playerLayer;

    [Header("Projectile Values")]
    [SerializeField] private int damage;
    [SerializeField] private bool useLifeTime;
    [SerializeField] private float lifeTime;
    [SerializeField] private float splashRange;
    [SerializeField] private float delayBeforeDetonation;
    [SerializeField] private float rotationCorrection;

    // Make audio clips variables list of audio clips for random variation
    [Header("Audio")]
    [SerializeField] private bool hasAudio;
    [SerializeField] private List<AudioClip> travelSound = new List<AudioClip>();
    [SerializeField] private float travelLowerPitch;
    [SerializeField] private float travelUpperPitch;
    [SerializeField] private float travelLowerVolume;
    [SerializeField] private float travelUpperVolume;
    [SerializeField] private bool hasImpactSound;
    [SerializeField] private List<AudioClip> impactSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> detonationSounds = new List<AudioClip>();
    [SerializeField] private AudioSource impactAudioSource;
    [SerializeField] private float impactLowerPitch;
    [SerializeField] private float impactUpperPitch;
    [SerializeField] private float impactLowerVolume;
    [SerializeField] private float impactUpperVolume;

    [Header("Impact VFX")]
    [SerializeField] private bool hasImpactVFX;
    [SerializeField] private ParticleSystem impactVFX;
    
    private AudioSource audioSource;
    private Collider playerCollider;
    private float deactivateTime;
    private float currentLifeTime;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        //impactAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
        //impactVFX = transform.GetChild(2).GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        //Destroy(gameObject, lifeTime);
        if (ignoreColision)
        {
            Physics.IgnoreCollision(playerCollider, transform.GetComponent<Collider>());
        }

        ModifyAudioSettings(audioSource, travelLowerPitch, travelUpperPitch, travelLowerVolume, travelUpperVolume);

        if (hasAudio)
        {
            impactAudioSource = Instantiate(impactAudioSource.gameObject).GetComponent<AudioSource>();
            impactAudioSource.gameObject.SetActive(false);

            impactAudioSource.transform.SetParent(GameObject.FindGameObjectWithTag("ObjectPool").transform);
        }

        if (hasImpactVFX)
        {
            impactVFX = Instantiate(impactVFX.gameObject).GetComponent<ParticleSystem>();
            impactVFX.gameObject.SetActive(false);

            impactVFX.transform.SetParent(GameObject.FindGameObjectWithTag("ObjectPool").transform);
        }

        currentLifeTime = lifeTime;
    }

    private void Update()
    {
        if (useLifeTime)
        {
            if (hasSplashDamage && currentLifeTime <= 0f)
            {
                StartCoroutine(ImpactExplosive());
            }

            currentLifeTime -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasSplashDamage)
        {
            StartCoroutine(ImpactExplosive());
        }
        else
        {
            Impact(collision);
        }
    }

    private void Impact(Collision collision)
    {
        if (hasAudio && hasImpactSound)
        {
            impactAudioSource.gameObject.SetActive(true);

            ModifyAudioSettings(impactAudioSource, impactLowerPitch, impactUpperPitch, impactLowerVolume, impactUpperVolume);

            int randomSound = Random.Range(0, impactSounds.Count);

            impactAudioSource.transform.position = transform.position;
            impactAudioSource.PlayOneShot(impactSounds[randomSound]);
        }

        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        if (hasAudio)
        {
            impactAudioSource.gameObject.SetActive(true);

            // Play projectile's detonation sound
            ModifyAudioSettings(impactAudioSource, impactLowerPitch, impactUpperPitch, impactLowerVolume, impactUpperVolume);

            int randomSound = Random.Range(0, detonationSounds.Count);

            impactAudioSource.transform.position = transform.position;
            impactAudioSource.PlayOneShot(detonationSounds[randomSound]);
        }

        if (hasImpactVFX)
        {
            impactVFX.gameObject.SetActive(true);

            // Play projectile's impact VFX
            impactVFX.Play();
            impactVFX.transform.position = transform.position;
        }

        UnitHealth hitObjectHealth = collision.transform.GetComponent<UnitHealth>();

        if (hitObjectHealth)
        {
            hitObjectHealth.DealDamage(damage);
        }

        if (destroyOnImpact)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetComponent<CapsuleCollider>().enabled = false;
            audioSource.enabled = false;

            StartCoroutine(DestroyProjectile());
        }
        else
        {
            transform.gameObject.SetActive(false);
        }
    }

    private IEnumerator ImpactExplosive()
    {
        if (hasAudio && hasImpactSound)
        {
            impactAudioSource.gameObject.SetActive(true);

            ModifyAudioSettings(impactAudioSource, impactLowerPitch, impactUpperPitch, impactLowerVolume, impactUpperVolume);

            int randomSound = Random.Range(0, impactSounds.Count);

            impactAudioSource.transform.position = transform.position;
            impactAudioSource.PlayOneShot(impactSounds[randomSound]);
        }

        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(delayBeforeDetonation);

        if (hasAudio)
        {
            impactAudioSource.gameObject.SetActive(true);

            // Play projectile's detonation sound
            ModifyAudioSettings(impactAudioSource, impactLowerPitch, impactUpperPitch, impactLowerVolume, impactUpperVolume);

            int randomSound = Random.Range(0, detonationSounds.Count);

            impactAudioSource.transform.position = transform.position;
            impactAudioSource.PlayOneShot(detonationSounds[randomSound]);
        }

        if (hasImpactVFX)
        {
            impactVFX.gameObject.SetActive(true);

            // Play projectile's impact VFX
            impactVFX.Play();
            impactVFX.transform.position = transform.position;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, splashRange);

        foreach (Collider collider in hitColliders)
        {
            UnitHealth hitObjectHealth = collider.transform.GetComponent<UnitHealth>();

            if (hitObjectHealth)
            {
                if (ignoreColision && collider == playerCollider)
                {
                    continue;
                }

                hitObjectHealth.DealDamage(damage);
            }
        }

        if (destroyOnImpact)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetComponent<CapsuleCollider>().enabled = false;
            audioSource.enabled = false;

            StartCoroutine(DestroyProjectile());
        }
        else
        {
            currentLifeTime = lifeTime;
            transform.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, splashRange);
    }

    private void ModifyAudioSettings(AudioSource audioSource, float lowerPitch, float upperPitch, float lowerVolume, float upperVolume)
    {
        float pitch = Random.Range(lowerPitch, upperPitch);
        float volume = Random.Range(lowerVolume, upperVolume);
        audioSource.pitch = pitch;
        audioSource.volume = volume;
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(7f);

        if (hasAudio)
        {
            Destroy(impactAudioSource.gameObject);
        }

        if (hasImpactVFX)
        {
            Destroy(impactVFX.gameObject);
        }

        Destroy(gameObject);
    }

    public void SetPlayerCollider(Collider value)
    {
        playerCollider = value;
    }

    public void ClearEffects()
    {
        Debug.Log(impactAudioSource.gameObject);

        if (hasAudio)
        {
            Destroy(impactAudioSource.gameObject);
        }

        if (hasImpactVFX)
        {
            Destroy(impactVFX.gameObject);
        }
    }
    /*private void DeactivateAudio()
    {
        impactAudioSource.gameObject.SetActive(false);
    }

    private void DeactivateVFX()
    {
        impactVFX.gameObject.SetActive(false);
    }*/
}