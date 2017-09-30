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

    private GameObject[] targetPowerups;
	private ObjectiveScript[] targetObjectives;
	private Quaternion spinQuat; // used to syncronize spinning
	private List<Vector3> knownEnemyLocs;
	private Vector3 teamVectorFactor;

	// TODO: figure out what this should be
	private const float MAX_NEAR_DIST = 30; // maximum distance to be considered 'near' to another player; probably needs to be adjusted

    public delegate void CharacterAIMethod(CharacterScript character, int characterIndex);
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
        InitializeStrategies();
        SetOverallStrategy(STRAT_FIFTY_KITE);

        // populate the objectives
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();
		if (ourTeamColor == team.red) {
			teamVectorFactor = new Vector3 (1, 1, 1);
		} else {
			teamVectorFactor = new Vector3 (-1, 1, -1);
		}

        targetPowerups = new GameObject[3];
        for(int i = 0; i < 3; i++)
        {
            targetPowerups[i] = null;
        }

		targetObjectives = new ObjectiveScript[3];
		for (int i = 0; i < targetObjectives.Length; i++) {
			targetObjectives [i] = middleObjective;
		}
		spinQuat = Quaternion.identity;
		knownEnemyLocs = new List<Vector3> ();

        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);
    }

    public class Strategy
    {
        public string name;
        public CharacterAIMethod[] strategyAIMethods;

        public Strategy(string strategyName, CharacterAIMethod[] aiMethods)
        {
            name = strategyName;
            strategyAIMethods = aiMethods;
        }
    }

    List<Strategy> allStrategies;
    Strategy STRAT_PURE_KILL_SQUAD; // All characters work in kill squad
    Strategy STRAT_SPAWN_KILL_WITH_HUNT; // 2 characters spawn kill, 1 hunts middle
    Strategy STRAT_CAP_AND_CAMP; // Cap and camp AI for all players
    Strategy STRAT_FIFTY_KITE;  // Kite for days

    void InitializeStrategies()
    {
        STRAT_PURE_KILL_SQUAD = new Strategy("STRAT_PURE_KILL_SQUAD", new CharacterAIMethod[] {KillSquadAI, KillSquadAI, KillSquadAI});
        STRAT_SPAWN_KILL_WITH_HUNT = new Strategy("STRAT_SPAWN_KILL", new CharacterAIMethod[] {spawnTrap, KillSquadAI, spawnTrap});
        STRAT_CAP_AND_CAMP = new Strategy("STRAT_CAP_AND_CAMP", new CharacterAIMethod[] {CapAndCamp, CapAndCamp, CapAndCamp});
        STRAT_FIFTY_KITE = new Strategy("STRAT_FIFTY_KITE", new CharacterAIMethod[] { kiteEnemies, kiteEnemies, kiteEnemies });

        allStrategies = new List<Strategy>();
        allStrategies.Add(STRAT_PURE_KILL_SQUAD);
        allStrategies.Add(STRAT_SPAWN_KILL_WITH_HUNT);
        allStrategies.Add(STRAT_CAP_AND_CAMP);
        allStrategies.Add(STRAT_FIFTY_KITE);
    }

    void SetOverallStrategy(Strategy strategyToSet)
    {
        for (int i = 0; i < allStrategies.Count; i++)
        {
            if (strategyToSet.name == allStrategies[i].name)
            {
                for (int j = 0; j < 3; j++)
                {
                    aiMethods[j] = allStrategies[i].strategyAIMethods[j];
                }

                allStrategies.RemoveAt(i);
                return;
            }
        }

        // Strategy not found
    }

    // Need to pass in the name of the powerup because reasons
    // Valid typeName parameters: "HealthPackItem(Clone)", PROBABLY NEED Item(Clone) too: "Points", "SpeedUp", "Power"
    // Returns null if cannot find an item of that type
    GameObject findClosestItemOfType(CharacterScript character, string typeName)
    {
        float closestDistance = 9001;
        GameObject closestObject = null;

        foreach (GameObject item in character.getItemList())
        {
            if (item.name == typeName)
            {
                float distanceToItem = Vector3.Distance(item.transform.position, character.getPrefabObject().transform.position);
                if (closestDistance > distanceToItem)
                {
                    closestDistance = distanceToItem;
                    closestObject = item;
                }
            }
        }

        return closestObject;
    }

    void spawnTrap(CharacterScript character, int characterIndex)
    {
        // Setup loadout for characters
        if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
            character.setLoadout(loadout.SHORT);

        // Rush to middle point
        if (timer <= 15)
        {
            character.MoveChar(middleObjective.transform.position);
            character.SetFacing(middleObjective.transform.position);
        }



        // Have other two characters near enemy spawn and camp
        if (timer > 15)
        {
            if (characterIndex == 0)
            { 
				character.MoveChar(Vector3.Scale(new Vector3(40.0f, 1.5f, -29.0f), teamVectorFactor));
                //SlowLookout(character, characterIndex);
                Guard(character, characterIndex, Vector3.Scale(new Vector3(40.0f, 1.5f, -29.0f), teamVectorFactor));
            }
            else if (characterIndex == 2)
            {
				character.MoveChar(Vector3.Scale(new Vector3(50.0f, 1.5f, -20.0f), teamVectorFactor));
                //SlowLookout(character, characterIndex);
                Guard(character, characterIndex, Vector3.Scale(new Vector3(50.0f, 1.5f, -20.0f), teamVectorFactor));
            }
        }

    } 


    void kiteEnemies(CharacterScript character, int characterIndex)
    {
        if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase)
        {
            character.setLoadout(loadout.LONG);
            character.SetFacing(middleObjective.transform.position);
        }

        if (characterIndex == 0)
        {
            if (middleObjective.getControllingTeam() == ourTeamColor)
            {
                Guard(character, characterIndex, middleObjective.transform.position);
                character.SetFacing(middleObjective.transform.position);
                character.MoveChar(middleObjective.transform.position + Vector3.Scale(new Vector3(-5, 0, 5), teamVectorFactor));
            }
            else
                character.MoveChar(middleObjective.transform.position);
        }
        else if (characterIndex == 1)
        {
            if(rightObjective.getControllingTeam() == ourTeamColor)
            {
                Guard(character, characterIndex, rightObjective.transform.position);
                character.SetFacing(rightObjective.transform.position);
                character.MoveChar(rightObjective.transform.position + Vector3.Scale(new Vector3(-5, 0, 5), teamVectorFactor));
            }
            else
                character.MoveChar(rightObjective.transform.position);
        }
        else
        {
            if (leftObjective.getControllingTeam() == ourTeamColor)
            {
                Guard(character, characterIndex, leftObjective.transform.position);
                character.SetFacing(leftObjective.transform.position);
                character.MoveChar(leftObjective.transform.position + Vector3.Scale(new Vector3(-5, 0, 5), teamVectorFactor));
            }
            else
                character.MoveChar(leftObjective.transform.position);
        }

        for (int i = 0; i < 3; i++)
        {
            MoveCharAwayEnemy(character, i);
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

		//TODO: should only be able to seek health after target point is capped.
        if (character.getHP() < 99)
        {
            if (targetPowerups[characterIndex] != null && 
                Vector3.Distance(targetPowerups[characterIndex].transform.position, character.getPrefabObject().transform.position) > 1)
            {
                return;
            }
            
            GameObject closestHealthPack = findClosestItemOfType(character, "HealthPackItem(Clone)");
            if (closestHealthPack != null)
            {
                //character.MoveChar(leftObjective.transform.position);
                targetPowerups[characterIndex] = closestHealthPack;
                character.MoveChar(closestHealthPack.transform.position);
                Guard(character, characterIndex, closestHealthPack.transform.position);
                return;
            }
        }

        ObjectiveScript currentObjective = targetObjectives[characterIndex];
		character.MoveChar(currentObjective.transform.position);
		Guard(character, characterIndex, currentObjective.transform.position);

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
			character.setLoadout(loadout.SHORT);

		ObjectiveScript currentObjective = targetObjectives [characterIndex];

		if (currentObjective.getControllingTeam () == ourTeamColor) {
			if (character.getHP () < 70) {
				if (targetPowerups[characterIndex] != null && 
					Vector3.Distance(targetPowerups[characterIndex].transform.position, character.getPrefabObject().transform.position) > 1)
				{
					return;
				}

				GameObject closestHealthPack = findClosestItemOfType(character, "HealthPackItem(Clone)");
				if (closestHealthPack != null)
				{
					//character.MoveChar(leftObjective.transform.position);
					targetPowerups[characterIndex] = closestHealthPack;
					character.MoveChar(closestHealthPack.transform.position);
					character.SetFacing(closestHealthPack.transform.position);
					return;
				}
			} if (middleObjective.getControllingTeam () != ourTeamColor) {
				// must capture middle objective
				targetObjectives [characterIndex] = middleObjective;

			} else if (GetLeastNeighborIndex (character, characterIndex) == characterIndex) {
				// leave least index neighboring ally to guard
				// -- move to watch location --
				character.MoveChar (currentObjective.transform.position + Vector3.Scale(new Vector3 (-5, 0, 5), teamVectorFactor));
				// -- and watch --
				Guard(character, characterIndex, currentObjective.transform.position);
			} else if (rightObjective.getControllingTeam () != ourTeamColor) {
				targetObjectives [characterIndex] = rightObjective;
			} else {
				targetObjectives [characterIndex] = leftObjective;
			}
		} else {
			character.MoveChar(currentObjective.transform.position);
            Guard(character, characterIndex, currentObjective.transform.position);
		}
    }





    // Make sure Update is the final method that is changed to accomadate the strategy that we want. Make sure there are no test cases running inside it.



    void Update()
    {
        if (timer == 1)
        {
            SetOverallStrategy(STRAT_FIFTY_KITE);
        }

        if (character1.getZone() == zone.BlueBase || character1.getZone() == zone.RedBase)
            character1.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character2.setLoadout(loadout.SHORT);
        if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase)
            character3.setLoadout(loadout.SHORT);

		knownEnemyLocs.Clear ();
		for (int i = 0; i < characters.Length; i++) {
			knownEnemyLocs.AddRange(characters[i].visibleEnemyLocations); // might be something wrong with visibleEnemyLocations breaking Lookout()
			knownEnemyLocs.AddRange(characters[i].attackedFromLocations); //                - -            attackedFromLocations        - -
			characters[i].attackedFromLocations.Clear();
		}

        // Run the individual AI methods for each character as specified by delegation 
        for (int i = 0; i < 3; i++)
        {
            aiMethods[i](characters[i], i);
        }
    }











    // a simple function to track game time
    public void gameTimer()
    {
        timer += 1;
	}

	void Guard(CharacterScript character, int characterIndex, Vector3 target)
	{
		bool enemyNear = false;
		for (int i = 0; i < knownEnemyLocs.Count; i++) {
			if (Vector3.Distance (knownEnemyLocs [i], character.getPrefabObject ().transform.position) < MAX_NEAR_DIST) {
				character.SetFacing ((knownEnemyLocs [i] - character.getPrefabObject ().transform.position).normalized);
				enemyNear = true;
			}
		}
	}

	void SlowLookout(CharacterScript character, int characterIndex)
	{
		bool enemyNear = false;
		for (int i = 0; i < knownEnemyLocs.Count; i++) {
			if (Vector3.Distance (knownEnemyLocs [i], character.getPrefabObject ().transform.position) < MAX_NEAR_DIST) {
				character.SetFacing ((knownEnemyLocs [i] - character.getPrefabObject ().transform.position).normalized);
				enemyNear = true;
			}
		}
		if (!enemyNear) {
			SlowSpin (character, characterIndex);
		}
	}

	void Lookout(CharacterScript character, int characterIndex)
	{
		bool enemyNear = false;
		for (int i = 0; i < knownEnemyLocs.Count; i++) {
			if (Vector3.Distance (knownEnemyLocs [i], character.getPrefabObject ().transform.position) < MAX_NEAR_DIST) {
				character.getPrefabObject().transform.rotation = Quaternion.LookRotation((knownEnemyLocs [i] - character.getPrefabObject().transform.position).normalized);
				enemyNear = true;
			}
		}
		if (!enemyNear) {
			Spin (character, characterIndex);
		}
	}

	void SlowSpin(CharacterScript character, int characterIndex)
	{
		int leastNeighborIndex = GetLeastNeighborIndex(character, characterIndex);
		if (leastNeighborIndex == characterIndex) {
			character.rotateAngle (90);
			spinQuat = character.getPrefabObject ().transform.rotation;
		} else if (GetNeighborCount (character, characterIndex) == 2) {
			character.SetFacing (character.getPrefabObject().transform.position + (spinQuat * Quaternion.Euler (0, 180, 0)) * Vector3.forward);
		} else {
			// characters should face at thirds...
			if (characterIndex == 2) {
				character.SetFacing (character.getPrefabObject().transform.position + (spinQuat * Quaternion.Euler (0, 120, 0)) * Vector3.forward);
			} else {
				character.SetFacing (character.getPrefabObject().transform.position + (spinQuat * Quaternion.Euler (0, 240, 0)) * Vector3.forward);
			}
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

		spinQuat = spinQuat * Quaternion.Euler (0, 30, 0); // change 40 to 1 if you want to see that they are facing the right way relative to one another.
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



    // moves character to last known position of enemy
    void MoveCharAwayEnemy(CharacterScript character, int characterIndex)
    {
        for (int i = 0; i < knownEnemyLocs.Count; i++)
        {                                                                                                      
            if (Vector3.Distance(knownEnemyLocs[i], character.getPrefabObject().transform.position) <= 36)  
            {
                character.SetFacing(knownEnemyLocs[i]);

                Vector3 value = (character.getPrefabObject().transform.position) - (knownEnemyLocs[i]);

                value = (value.normalized * 35.0f) + knownEnemyLocs[i];

                character.MoveChar(value);
            }
        }
    }
}

