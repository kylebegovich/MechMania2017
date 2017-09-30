﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEAM_BLUE_SCRIPT : MonoBehaviour
{
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
    int i = 0;
    /// <summary>
    /// ^^^^^^^^
    /// </summary>
    /// 
    public static TEAM_BLUE_SCRIPT AddYourselfTo(GameObject host) {
        return host.AddComponent<TEAM_BLUE_SCRIPT>();
    }

    void Start()
    {
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();
    }



    void Update()
	{
        
        character1.FaceClosestWaypoint();
        character2.FaceClosestWaypoint();
        character3.FaceClosestWaypoint();
        character1.MoveChar(new Vector3(-21.5f, 2f, 19.3f));
        if (i == 0) {
            character2.MoveChar(character2.FindClosestCover(character1.transform.position));
            i++;
        }
        character3.MoveChar(new Vector3(29.0f, 0f, 10f));

        //GetComponent<PathFinder>().location = position;

    }

    
}
