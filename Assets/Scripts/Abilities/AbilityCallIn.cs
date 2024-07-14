using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityCallIn : MonoBehaviour
{
    [SerializeField] private GameObject ability;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float range;

    private Transform indicator;

    private void Start()
    {
        indicator = transform.GetChild(0);
        indicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            indicator.gameObject.SetActive(true);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, groundLayer))
            {
                indicator.position = new Vector3(raycastHit.point.x, 0.01f, raycastHit.point.z);

                if (Input.GetMouseButtonDown(0))
                {
                    Instantiate(ability).GetComponent<Ability>().ActivateAbility();
                }
            }
        }
        else
        {
            indicator.gameObject.SetActive(false);
        }
    }
}