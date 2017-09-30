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

    delegate void CharacterAIMethod(CharacterScript character, int characterIndex);
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
	aiMethods[0] = spawnTrap;
	aiMethods[1] = spawnTrap;
	aiMethods[2] = spawnTrap;

        // populate the objectives
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();
        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);

    }

    void character1AI(CharacterScript character, int characterIndex)
    {
        character.MoveChar(leftObjective.transform.position);
        character.SetFacing(leftObjective.transform.position);
    }

    void character2AI(CharacterScript character, int characterIndex)
    {
        character.MoveChar(leftObjective.transform.position);
        character.SetFacing(leftObjective.transform.position);
    }

    void character3AI(CharacterScript character, int characterIndex)
    {
        character.MoveChar(rightObjective.transform.position);
        character.SetFacing(rightObjective.transform.position);
    }

    void spawnTrap(CharacterScript character, int characterIndex)
    {
    	// Setup loadout for characters
    	if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
    		if (characterIndex == 0 || characterIndex == 2)
    			character.setLoadout(loadout.MEDIUM);
    		else
    			character.setLoadout(loadout.SHORT);

        // Rush to middle point
        if (middleObjective.getControllingTeam() != character1.getTeam() || middleObjective.getControllingTeam() == null)
        {
            character.MoveChar(middleObjective.transform.position);
            character.SetFacing(middleObjective.transform.position);
        }

        if (middleObjective.getControllingTeam() == character1.getTeam())
    	{
            if (characterIndex == 0)
            {
                character.MoveChar(new Vector3(40.0f, 1.5f, -29.0f));
                spin(character, characterIndex);
            }
            else if (characterIndex == 2)
            {
                character.MoveChar(new Vector3(50.0f, 1.5f, -20.0f));
                spin(character, characterIndex);
            }
            else
            {
                if (rightObjective.getControllingTeam() != character1.getTeam())
                {
                    character.MoveChar(rightObjective.transform.position);
                    character.SetFacing(rightObjective.transform.position);
                }
                else
                {
                    character.MoveChar(leftObjective.transform.position);
                    character.SetFacing(leftObjective.transform.position);
                }
                
            }
    	}
    }

    private ObjectiveScript[] targetObjectives = null;
    void KillSquadAI(CharacterScript character, int characterIndex)
    {
        // Initialize necessary data
        if (targetObjectives == null)
        { 
            targetObjectives = new ObjectiveScript[3];

            for (int i = 0; i < 3; i++)
            {
                targetObjectives[i] = leftObjective;
            }
        }

        // Ensure all characters have SHORT layout
        if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
            character.setLoadout(loadout.SHORT);

        ObjectiveScript currentObjective = targetObjectives[characterIndex];

        if (currentObjective.getControllingTeam() == ourTeamColor)
        {
            // Current objective, capped, update and try again next frame
            if (currentObjective == leftObjective)
                targetObjectives[characterIndex] = middleObjective;
            else
            if (currentObjective == middleObjective)
                targetObjectives[characterIndex] = rightObjective;
            else
                targetObjectives[characterIndex] = leftObjective;

            return;
        }

        character.MoveChar(currentObjective.transform.position);
        character.SetFacing(currentObjective.transform.position);
    }

    void CapAndCamp(CharacterScript character, int characterIndex) {
        bool hasMiddleObjective = true;
        bool allThreeAlive = true;
        bool hasBottomObjective = true;

        if (!hasMiddleObjective) {

            // go get middle objective
        
        } else if (allThreeAlive) {
            if (hasBottomObjective) {

                // go get top objective with low health players

                while (!allThreeAlive) {
                    // bring all charaters to center
                }
            } else {
				
                // go get bottom objective with low health players
				
                while (!allThreeAlive)
				{
					// bring all charaters to center
				}
			}
        }
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
            aiMethods[i](characters[i], i);
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

	void spin(CharacterScript character, int characterIndex)
	{
		character.rotateAngle (600f);
	}

	// returns: bool[] where *true* denotes index of an ally within 35m of character
	bool[] getNearAllies(CharacterScript character, int characterIndex)
	{
		bool[] isNearArr = new bool[3];
		for (int i = 0; i < characters.Length; i++) {
			if (i != characterIndex) {
				isNearArr [i] = Vector3.Distance (character.transform.position, characters [i].transform.position) < 35;
			} else {
				isNearArr [i] = false;
			}
		}
		return isNearArr;
	}
}

