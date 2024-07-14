using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MissionManager : MonoBehaviour
{
    [Serializable]
    struct ObjectiveInfo
    {
        public string eventTrigger;
        public string statusText;
        public int maxValue;
        public List<GameObject> objectiveInstance;
        public ObjectiveTrigger objectiveTrigger;
        public Image objectiveMarker;
        public Vector3 objectiveMarkerOffset;
    }

    [Header("Mission Manager")]
    [SerializeField] private float objectiveDelay; 
    [SerializeField] private List<ObjectiveInfo> objectives = new List<ObjectiveInfo>();

    [Header("UI")]
    [SerializeField] private Canvas missionCanvas;
    [SerializeField] private GameObject currentObjectiveUI;
    [SerializeField] private TextMeshProUGUI currentObjectiveText;
    [SerializeField] private TextMeshProUGUI objectiveStatusText;
    [SerializeField] private GameObject objectiveCompleteUI;
    [SerializeField] private Camera objectiveCamera;

    private PlayerStatus playerStatus;
    private ObjectiveManager objectiveManager;
    private Objective objective;
    private int currentObjective;


    private void Start()
    {
        objectiveManager = new ObjectiveManager();
        currentObjective = 0;

        currentObjectiveUI.SetActive(false);
        objectiveCompleteUI.SetActive(false);
        objectiveStatusText.gameObject.SetActive(false);

        Invoke("CreateObjective", objectiveDelay);
    }

    private void CreateObjective()
    {
        if (currentObjective == objectives.Count)
        {
            playerStatus.CallMissionEnd();
            return;
        }

        ObjectiveInfo info = objectives[currentObjective];
        objective = new Objective(info.eventTrigger, info.statusText, info.maxValue);
        objective.OnComplete += OnObjectiveComplete;
        objective.OnValueChange += OnObjectiveValueChange;
        objectiveManager.AddObjective(objective);

        foreach (GameObject objInstance in info.objectiveInstance)
        {
            foreach (Transform obj in objInstance.transform.GetChild(0))
            {
                UnitHealth unitHealth = obj.GetComponent<UnitHealth>();
                SubOjectiveManager subOjective = obj.GetComponent<SubOjectiveManager>();

                if (unitHealth)
                {
                    unitHealth.SetToObjective(objective);
                }
                
                if (subOjective)
                {
                    subOjective.SetToObjective(objective);

                    foreach (GameObject subObj in subOjective.GetSubObjectives())
                    {
                        Image objMarker = Instantiate(info.objectiveMarker.gameObject).GetComponent<Image>();
                        objMarker.transform.SetParent(missionCanvas.transform);
                        objectiveCamera.AddComponent<MissionWaypoint>().SetData(objectiveCamera, objMarker, subObj.transform, info.objectiveMarkerOffset);
                    }
                }
                else
                {
                    Image objMarker = Instantiate(info.objectiveMarker.gameObject).GetComponent<Image>();
                    objMarker.transform.SetParent(missionCanvas.transform);
                    objectiveCamera.AddComponent<MissionWaypoint>().SetData(objectiveCamera, objMarker, obj, info.objectiveMarkerOffset);
                }
            }
        }

        currentObjectiveText.text = info.eventTrigger;
        objectiveStatusText.text = objective.GetStatusText();

        currentObjectiveUI.SetActive(true);
        objectiveStatusText.gameObject.SetActive(true);

        currentObjectiveUI.GetComponent<Animator>().Play("Current Objective");
    }

    private void OnObjectiveComplete()
    {
        if (objectives[currentObjective].objectiveTrigger)
        {
            objectives[currentObjective].objectiveTrigger.FireTrigger();
        }

        currentObjective++;

        objectiveCompleteUI.SetActive(true);
        objectiveStatusText.gameObject.SetActive(false);
        Invoke("CreateObjective", objectiveDelay);
    }

    private void OnObjectiveValueChange()
    {
        objectiveStatusText.text = objective.GetStatusText();
    }

    public void SetPlayerStatus(PlayerStatus player)
    {
        playerStatus = player;
    }
}
