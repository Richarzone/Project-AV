using UnityEngine;
using UnityEngine.UIElements;

public class ThrowProjectile : Ability
{
    [SerializeField] private GameObject objectToThrow;
    [SerializeField] private float throwForce;

    [Header("SFX")]
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private float lowerPitch;
    [SerializeField] private float upperPitch;

    private Vector3 mousePosition;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateAbility();
        }
    }

    public override void ActivateAbility()
    {
        SetAudioSettings();
        audioSource.PlayOneShot(fireClip);

        // Get mouse position in screen space
        Vector3 mousePos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity))
        {
            mousePosition = raycastHit.point;
        }

        // Calculate direction from object to mouse position
        Vector3 throwDirection = (mousePosition - transform.position).normalized;

        // Instantiate object to throw
        GameObject thrownObject = Instantiate(objectToThrow, transform.position, Quaternion.identity);

        // Apply force to the thrown object
        thrownObject.GetComponent<Rigidbody>().AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    private void SetAudioSettings()
    {
        float pitch = Random.Range(lowerPitch, upperPitch);
        //float volume = Random.Range(lowerVolume, upperVolume);

        audioSource.pitch = pitch;
        //audioSource.volume = volume;
    }
}