using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TEAM_RED_SCRIPT : MonoBehaviour
{
    //private Vector3 position = new Vector3(20.0f, 0.0f, 20.0f);

    /// <summary>
    /// DO NOT MODIFY THIS! 
    /// vvvvvvvvv
    /// </summary>
    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;
    /// <summary>
    /// ^^^^^^^^
    /// </summary>
    /// 


    // USEFUL VARIABLES
    private ObjectiveScript middleObjective;
    private ObjectiveScript leftObjective;
    private ObjectiveScript rightObjective;
    private float timer = 0;

    private team ourTeamColor;
    public static TEAM_RED_SCRIPT AddYourselfTo(GameObject host)
    {
        return host.AddComponent<TEAM_RED_SCRIPT>();
    }

    delegate void CharacterAIMethod(CharacterScript character);
    CharacterScript[] characters;
    CharacterAIMethod[] aiMethods;

    void Start()
    {
        // Set up code. This populates your characters with their controlling scripts
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();

        characters = new CharacterScript[3];
        characters[0] = character1;
        characters[1] = character2;
        characters[2] = character3;

        aiMethods = new CharacterAIMethod[3];
	aiMethods[0] = character1AI;
	aiMethods[1] = character2AI;
	aiMethods[2] = character3AI;

        // populate the objectives
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();
        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);

    }

    void character1AI(CharacterScript character)
    {
        character.MoveChar(leftObjective.transform.position);
        character.SetFacing(leftObjective.transform.position);
    }

    void character2AI(CharacterScript character)
    {
        character.MoveChar(leftObjective.transform.position);
        character.SetFacing(leftObjective.transform.position);
    }

    void character3AI(CharacterScript character)
    {
        character.MoveChar(rightObjective.transform.position);
        character.SetFacing(rightObjective.transform.position);
    }

    void Update()
    {
        if (character1.getZone() == zone.BlueBase || character1.getZone() == zone.RedBase)
            character1.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character2.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character3.setLoadout(loadout.SHORT);

        for (int i = 0; i < 3; i++)
        {
            aiMethods[i](characters[i]);
        }


        //Set caracter loadouts, can only happen when the characters are at base.
        /*if (character1.getZone() == zone.BlueBase || character1.getZone() == zone.RedBase) 
            character1.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character2.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character3.setLoadout(loadout.SHORT);*/

        // in the first couple of seconds we just scan around
      //  if (timer < 10)
            /*     {
                     character1.FaceClosestWaypoint();
                     character2.FaceClosestWaypoint();
                     character3.FaceClosestWaypoint();
                     character1.MoveChar(new Vector3(-8.8f, 1.5f, 13.5f));
                 }
                 // place sniper in position, run to cover if attacked
                 if (character1.attackedFromLocations.Capacity == )
                 {
                     character1.MoveChar(new Vector3(-8.8f, 1.5f, 13.5f));
                     character1.SetFacing(middleObjective.transform.position);
                 }
                 else
                 {
                     character1.MoveChar(character1.FindClosestCover(character1.attackedFromLocations[0]));
                 }
                 // send other two to capture
            */
        /*if (middleObjective.getControllingTeam() != character1.getTeam())
        {
            character1.MoveChar(middleObjective.transform.position);
            character1.SetFacing(middleObjective.transform.position);
            character2.MoveChar(middleObjective.transform.position);
            character2.SetFacing(middleObjective.transform.position);
            character3.MoveChar(middleObjective.transform.position);
            character3.SetFacing(middleObjective.transform.position);
        }
        else if (leftObjective.getControllingTeam() != character1.getTeam())
        {
                // Then left
                //   if (leftObjective.getControllingTeam() != character1.getTeam())
                //   {
            character1.MoveChar(leftObjective.transform.position);
            character1.SetFacing(leftObjective.transform.position);
            character2.MoveChar(leftObjective.transform.position);
            character2.SetFacing(leftObjective.transform.position);
            character3.MoveChar(leftObjective.transform.position);
            character3.SetFacing(leftObjective.transform.position);
                //  }
         }
            // Then RIght
         else
         {
            character1.MoveChar(rightObjective.transform.position);
            character1.SetFacing(rightObjective.transform.position);
            character2.MoveChar(rightObjective.transform.position);
            character2.SetFacing(rightObjective.transform.position);
            character3.MoveChar(rightObjective.transform.position);
            character3.SetFacing(rightObjective.transform.position);
         }*/
      
    }

    // a simple function to track game time
    public void gameTimer()
    {
        timer += 1;
    }

}

