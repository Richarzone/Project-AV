using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuObject : MonoBehaviour
{
    private MenuAnimation menuAnimation;

    private void Start()
    {
        menuAnimation = transform.parent.GetComponent<MenuAnimation>();
    }

    private void SignalFade()
    {
        menuAnimation.Fade();
    }

    private void SignalChange()
    {
        menuAnimation.ChangeMenuAnimation();
    }
}
