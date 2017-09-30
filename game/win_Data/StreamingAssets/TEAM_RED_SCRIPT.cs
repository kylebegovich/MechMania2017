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

	private ObjectiveScript[] targetObjectives;
	private Quaternion spinQuat; // used to syncronize spinning
	private List<Vector3> knownEnemyLocs;

	// TODO: figure out what this should be
	private const float MAX_NEAR_DIST = 20; // maximum distance to be considered 'near' to another player; probably needs to be adjusted

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
		aiMethods [0] = KillSquadAI;
		aiMethods [1] = KillSquadAI;
		aiMethods [2] = KillSquadAI;

        // populate the objectives
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();

		targetObjectives = new ObjectiveScript[3];
		for (int i = 0; i < targetObjectives.Length; i++) {
			targetObjectives [i] = middleObjective;
		}
		spinQuat = Quaternion.identity;
		knownEnemyLocs = new List<Vector3> ();

        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);

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
				Lookout (character, characterIndex);
            }
            else if (characterIndex == 2)
            {
                character.MoveChar(new Vector3(50.0f, 1.5f, -20.0f));
				Lookout (character, characterIndex);
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

    private bool[] lastWentToLeft = null;
    void KillSquadAI(CharacterScript character, int characterIndex)
    {
        // Initialize necessary data
		if (lastWentToLeft == null)
        {
            lastWentToLeft = new bool[3];

            for (int i = 0; i < 3; i++)
            {
                lastWentToLeft[i] = true;
            }
        }

        // Ensure all characters have SHORT layout
        if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
            character.setLoadout(loadout.SHORT);

        // Enable FIDGET SPINNING
        Lookout(character, characterIndex);

        ObjectiveScript currentObjective = targetObjectives[characterIndex];
        characters[characterIndex].MoveChar(currentObjective.transform.position);
        characters[characterIndex].SetFacing(currentObjective.transform.position);

        if (currentObjective == middleObjective)
        {
            // Continue moving until we are less than 5 distance away
            if (Vector3.Distance(currentObjective.transform.position, character.getPrefabObject().transform.position) > 5)
                return;

            // We are less than 5 distance, cap the point if not capped
            if (middleObjective.getControllingTeam() != ourTeamColor)
            {
                return;
            }

            // We are currently at captured middle position (Home Base) 
            // Evaluate whether should go to left or right
            if (leftObjective.getControllingTeam() != ourTeamColor && rightObjective.getControllingTeam() != ourTeamColor)
            {
                // Should probably choose at random
                if (lastWentToLeft[characterIndex])
                {
                    targetObjectives[characterIndex] = rightObjective;
                    lastWentToLeft[characterIndex] = false;
                }
                else
                {
                    targetObjectives[characterIndex] = leftObjective;
                    lastWentToLeft[characterIndex] = true;
                }
            }
            else
            if (leftObjective.getControllingTeam() != ourTeamColor)
            {
                targetObjectives[characterIndex] = leftObjective;
            }
            else
            {
                targetObjectives[characterIndex] = rightObjective;
            }

            return;
        }

        // Heading towards either left or right objective
        if (currentObjective.getControllingTeam() == ourTeamColor)
        {
            // Objective captured, head back to home base
            targetObjectives[characterIndex] = middleObjective;
            return;
        }
    }

	// TODO: Sometimes characters will move towards already guarded Objective right after it is capped when respawning
	void CapAndCamp(CharacterScript character, int characterIndex) {

		// Ensure all characters have MEDIUM layout
		if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
			character.setLoadout(loadout.MEDIUM);

		ObjectiveScript currentObjective = targetObjectives [characterIndex];

		if (currentObjective.getControllingTeam () == ourTeamColor) {
			if (middleObjective.getControllingTeam () != ourTeamColor) {
				// must capture middle objective
				targetObjectives [characterIndex] = middleObjective;

			} else if (GetLeastNeighborIndex (character, characterIndex) == characterIndex) {
				// leave least index neighboring ally to guard
				// -- move to watch location --
				character.MoveChar (currentObjective.transform.position + new Vector3 (-5, 0, 5));
				// -- and watch --
				character.SetFacing (currentObjective.transform.position);
			} else if (rightObjective.getControllingTeam () != ourTeamColor) {
				targetObjectives [characterIndex] = rightObjective;
			} else {
				targetObjectives [characterIndex] = leftObjective;
			}
		} else {
			character.MoveChar (currentObjective.transform.position);
			Lookout (character, characterIndex);
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

		knownEnemyLocs.Clear ();
		for (int i = 0; i < characters.Length; i++) {
			knownEnemyLocs.AddRange (characters [i].visibleEnemyLocations); // might be something wrong with visibleEnemyLocations breaking Lookout()
			knownEnemyLocs.AddRange (characters [i].attackedFromLocations); //                - -            attackedFromLocations        - -
			characters [i].attackedFromLocations.Clear ();
		}

        for (int i = 0; i < 3; i++)
        {
            aiMethods[i](characters[i], i);
        }


        //Set character loadouts, can only happen when the characters are at base.
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

	void Lookout(CharacterScript character, int characterIndex)
	{
		bool enemyNear = false;
		for (int i = 0; i < knownEnemyLocs.Count; i++) {
			if (Vector3.Distance (knownEnemyLocs [i], character.getPrefabObject ().transform.position) < MAX_NEAR_DIST) {
				character.SetFacing (knownEnemyLocs [i]);
				enemyNear = true;
			}
		}
		if (!enemyNear) {
			Spin (character, characterIndex);
		}
	}

	void Spin(CharacterScript character, int characterIndex)
	{
		int leastNeighborIndex = GetLeastNeighborIndex(character, characterIndex);
		if (leastNeighborIndex == characterIndex) {
			character.getPrefabObject().transform.rotation = spinQuat;
		} else if (GetNeighborCount (character, characterIndex) == 2) {
			character.getPrefabObject().transform.rotation = spinQuat * Quaternion.Euler(0, 180, 0);
		} else {
			// characters should face at thirds...
			if (characterIndex == 2) {
				character.getPrefabObject().transform.rotation = spinQuat * Quaternion.Euler(0, 120, 0);
			} else {
				character.getPrefabObject().transform.rotation = spinQuat * Quaternion.Euler (0, 240, 0);
			}
		}

		spinQuat = spinQuat * Quaternion.Euler (0, 40, 0); // change 40 to 1 if you want to see that they are facing the right way relative to one another.
	}
	
	
	// returns: bool[] where *true* denotes index of an ally within MAX_NEAR_DIST of character (excluding self)
	bool[] GetNearAllies(CharacterScript character, int characterIndex)
	{
		bool[] isNearArr = new bool[3];
		for (int i = 0; i < characters.Length; i++) {
			if (i != characterIndex) {
				isNearArr [i] = Vector3.Distance (character.getPrefabObject().transform.position, characters [i].getPrefabObject().transform.position) < MAX_NEAR_DIST;
			} else {
				isNearArr [i] = false;
			}
		}
		return isNearArr;
	}

	// returns: number of allies in range (including self)
	int GetNeighborCount(CharacterScript character, int characterIndex)
	{
		int count = 0;
		for (int i = 0; i < characters.Length; i++) {
			if (Vector3.Distance (character.getPrefabObject ().transform.position, characters [i].getPrefabObject ().transform.position) < MAX_NEAR_DIST) {
				count++;
			}
		}
		return count;
	}
	
	// return highest index of near allies (including self)
	int GetLeastNeighborIndex(CharacterScript character, int characterIndex)
	{
		for(int i = 0; i < characters.Length; i++) {
			if(Vector3.Distance(character.getPrefabObject().transform.position, characters[i].getPrefabObject().transform.position) < MAX_NEAR_DIST) {
				return i;
			}
		}
		return characterIndex;
	}
}

