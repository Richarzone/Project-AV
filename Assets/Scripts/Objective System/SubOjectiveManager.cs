using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubOjectiveManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> subObjectives;

    private Objective objective;
    private int currentProgress = 0;

    private void Start()
    {
        foreach (GameObject subObjective in subObjectives)
        {
            subObjective.GetComponent<UnitHealth>().SetToSubObjective(this);
        }
    }

    public void AddProgress()
    {
        currentProgress++;

        if (currentProgress == subObjectives.Count)
        {
            objective.AddProgress(1);
            //Destroy(gameObject);
        }
    }

    public List<GameObject> GetSubObjectives()
    {
        return subObjectives;
    }

    public void SetToObjective(Objective value)
    {
        objective = value;
    }
}
