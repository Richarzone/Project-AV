using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private void DestroyMe()
    {
        Destroy(gameObject);
    }

    private void DeactivateMe()
    {
        gameObject.SetActive(false);
    }
}
