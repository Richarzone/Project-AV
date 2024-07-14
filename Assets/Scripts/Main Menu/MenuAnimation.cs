using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimation : MonoBehaviour
{
    [SerializeField] private List<GameObject> menuAnimations = new List<GameObject>();
    [SerializeField] private Animator menuFade;
    [SerializeField] private Animator selection;
    private int currentAnimation;

    void Start()
    {
        Time.timeScale = 1f;

        foreach (GameObject animation in menuAnimations)
        {
            animation.SetActive(false);
        }

        currentAnimation = 0;
        menuAnimations[currentAnimation].SetActive(true);

        menuAnimations[currentAnimation].GetComponent<Animator>().Play("Menu Animation");
        selection.Play("Selection Menu");
        menuFade.Play("Fade In");
    }

    public void ChangeMenuAnimation()
    {
        menuAnimations[currentAnimation].SetActive(false);
        currentAnimation++;

        if (currentAnimation > menuAnimations.Count - 1)
        {
            currentAnimation = 0;
        }

        menuAnimations[currentAnimation].SetActive(true);
        menuAnimations[currentAnimation].GetComponent<Animator>().Play("Menu Animation");
    }

    public void Fade()
    {
        menuFade.Play("Fade Transition", 0, 0f);
    }
}
