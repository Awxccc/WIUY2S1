using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SuddenEventSystem : MonoBehaviour
{

    public SuddenEvent suddenEvent;
    private int suddeneventoccur;
    private int whichsuddenevent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //testing
    void Start()
    {
        var interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += ctx =>
        {
            //using the first item in the list
            SuddenEventInstance sevnet = suddenEvent.GetEvent(0);
            if (suddenEvent != null)
            {
                sevnet.suddenEventEffect.Effect(this.gameObject);
                Debug.Log("Event active");
            }
        };
    }

    //on turn start call this to set the sudden event for the turn
    public void suddeneventstart()
    {
        //check if want to call a sudden event or not
        //randomiser
        suddeneventoccur = UnityEngine.Random.Range(1, 3);

        //check if sudden event occurs or not
        if (suddeneventoccur == 1)
        {
            //no sudden event
            return;
        }
        else if (suddeneventoccur == 2)
        {
            //check which random event to call
            whichsuddenevent = UnityEngine.Random.Range(0, suddenEvent.Itemcount());
            suddeneventselector(whichsuddenevent);
        }
    }

    //call the random sudden event
    private void suddeneventselector(int num)
    {
        SuddenEventInstance sevnet = suddenEvent.GetEvent(num);
        if (suddenEvent != null)
        {
            sevnet.suddenEventEffect.Effect(this.gameObject);
            Debug.Log("Event active");
        }
    }
    
    //call the scripted event for the year

    //on turn end call this to clear the scripted events event for the turn (do ltr)
    // public void eventclear()
    // {

    // }
}
