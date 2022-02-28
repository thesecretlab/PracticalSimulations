using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScore : MonoBehaviour
{
    public BlockSorterAgent agent;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("goal"))
        {
            agent.GoalScored();
        }
    }
}
