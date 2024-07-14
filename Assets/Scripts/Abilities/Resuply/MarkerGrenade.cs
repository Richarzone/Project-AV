using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerGrenade : MonoBehaviour
{
    [SerializeField] private GameObject markerFunctionality;
    [SerializeField] private ParticleSystem markerVFX;
    [SerializeField] private float fireDelay;
    [SerializeField] private float callInDelay;

    [Header("SFX")]
    [SerializeField] private AudioClip grenadeBounce;
    [SerializeField] private AudioClip grenadeFire;
    [SerializeField] private float lowerPitch;
    [SerializeField] private float upperPitch;

    private Rigidbody rb;
    private CapsuleCollider grenadeCollider;
    private Transform vfxPivot;
    private AudioSource audioSource;
    private bool collided;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grenadeCollider = GetComponent<CapsuleCollider>();
        //vfxPivot = transform.GetChild(0).GetComponent<Transform>();
        audioSource = GetComponent<AudioSource>();

        Debug.Log(transform.rotation.eulerAngles);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collided)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            SetAudioSettings();
            audioSource.PlayOneShot(grenadeBounce);

            StartCoroutine(Fire());
            collided = true;
        }
    }

    private IEnumerator Fire()
    {
        yield return new WaitForSeconds(fireDelay);

        //vfxPivot.localRotation = Quaternion.Euler(0f, (-transform.rotation.eulerAngles.z + transform.rotation.eulerAngles.y), 0f);

        SetAudioSettings();
        audioSource.PlayOneShot(grenadeFire);
        markerVFX.Play();

        rb.useGravity = false;
        grenadeCollider.enabled = false;

        yield return new WaitForSeconds(callInDelay);

        Instantiate(markerFunctionality, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(markerVFX.main.duration + 3f);

        Destroy(gameObject);
    }

    private void SetAudioSettings()
    {
        float pitch = Random.Range(lowerPitch, upperPitch);
        //float volume = Random.Range(lowerVolume, upperVolume);

        audioSource.pitch = pitch;
        //audioSource.volume = volume;
    }
}
