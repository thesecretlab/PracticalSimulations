using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalScore : MonoBehaviour
{
    [Header("Trigger Collider Tag To Detect")]
    public string tagToDetect = "goal"; //collider tag to detect

    [Header("Goal Value")]
    public float GoalValue = 1;

    private Collider blockCollider;

    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, float>
    {
    }

    [Header("Trigger Callbacks")]
    public TriggerEvent onTriggerEnterEvent = new TriggerEvent(); 
    public TriggerEvent onTriggerStayEvent = new TriggerEvent();
    public TriggerEvent onTriggerExitEvent = new TriggerEvent();

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            onTriggerEnterEvent.Invoke(blockCollider, GoalValue);
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            onTriggerStayEvent.Invoke(blockCollider, GoalValue);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            onTriggerExitEvent.Invoke(blockCollider, GoalValue);
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        blockCollider = GetComponent<Collider>();
    }
}
