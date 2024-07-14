using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Resuply : Ability
{
    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject markerGrenade;
    [SerializeField] private float throwForce;

    [Header("SFX")]
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private float lowerPitch;
    [SerializeField] private float upperPitch;

    private AudioSource audioSource;
    private Vector3 targetPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        rangeIndicator.transform.localScale = new Vector3(aOE, aOE, aOE);
        rangeIndicator.SetActive(false);
    }

    public override void ActivateAbility()
    {
        if (!lockAbility)
        {
            StartCoroutine(SpawnMarker());
        }
    }

    private IEnumerator SpawnMarker()
    {
        lockAbility = true;

        while (!abilityInput)
        {
            GetMousePosition();

            yield return null;
        }

        SetAudioSettings();
        audioSource.PlayOneShot(fireClip);

        GameObject markerGrenadeInstance = Instantiate(markerGrenade, transform.position, GetRandomRotation());

        // Calculate direction from object to mouse position
        Vector3 throwDirection = (targetPosition - transform.position).normalized;

        // Apply force to the thrown object
        markerGrenadeInstance.GetComponent<Rigidbody>().AddForce(throwDirection * throwForce, ForceMode.Impulse);

        abilityInput = false;
        abilityButton.interactable = false;
        rangeIndicator.SetActive(false);

        yield return new WaitForSeconds(cooldown);

        lockAbility = false;
        abilityButton.interactable = true;
    }

    private void GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        rangeIndicator.SetActive(true);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, groundLayer))
        {
            targetPosition = raycastHit.point;
            rangeIndicator.transform.position = new Vector3(targetPosition.x, 0.01f, targetPosition.z);
        }
    }

    private void SetAudioSettings()
    {
        float pitch = Random.Range(lowerPitch, upperPitch);
        //float volume = Random.Range(lowerVolume, upperVolume);

        audioSource.pitch = pitch;
        //audioSource.volume = volume;
    }

    private Quaternion GetRandomRotation()
    {
        float randomXRotation = Random.Range(0f, 360f);
        float randomYRotation = Random.Range(0f, 360f);
        float randomZRotation = Random.Range(0f, 360f);

        return Quaternion.Euler(randomXRotation, randomYRotation, randomZRotation);
    }
}
