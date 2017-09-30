using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEAM_RED_SCRIPT : MonoBehaviour
{

    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;


    private ObjectiveScript middleObjective;
    private ObjectiveScript leftObjective;
    private ObjectiveScript rightObjective;
    private float timer = 0;

    private team ourTeamColor;
    //---------- CHANGE THIS NAME HERE -------
    public static TEAM_RED_SCRIPT AddYourselfTo(GameObject host)
    {
        //---------- CHANGE THIS NAME HERE -------
        return host.AddComponent<TEAM_RED_SCRIPT>();
    }

    void Start()
    {
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();

        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();
        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);
    }


    void Update()
    {
        //choose loadouts
        if (character1.getZone() == zone.BlueBase || character1.getZone() == zone.RedBase)
            character1.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character2.setLoadout(loadout.LONG);
        if (character3.getZone() == zone.BlueBase || character3.getZone() == zone.RedBase)
            character3.setLoadout(loadout.MEDIUM);

        //choose priorities
        character1.priority = firePriority.LOWHP;
        character2.priority = firePriority.LOWHP;
        character3.priority = firePriority.LOWHP;


        //character 1
        character1.SetFacing(leftObjective.transform.position);
        if (leftObjective.getControllingTeam() == team.blue)
        {
            character1.MoveChar(leftObjective.transform.position);
        }
        else
        {
            character1.MoveChar(new Vector3(-45f, 1.5f, -40f));
        }

        //character 2
        character2.SetFacing(middleObjective.transform.position);
        if (middleObjective.getControllingTeam() == team.blue)
        {
            character2.MoveChar(middleObjective.transform.position);
        }
        else
        {
            character2.MoveChar(new Vector3(-2f, 1.5f, 22f));
        }

        //character 3
        character3.SetFacing(rightObjective.transform.position);
        if (rightObjective.getControllingTeam() == team.blue)
        {
            character3.MoveChar(rightObjective.transform.position);
        }
        else
        {
            character3.MoveChar(new Vector3(45f, 1.5f, 40f));
        }
    }

    
    public void gameTimer()
    {
        timer += 1;
    }
}