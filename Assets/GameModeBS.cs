using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeBS : MonoBehaviour {
    public List<ObjectiveBS> OBJECTIVES = new List<ObjectiveBS>();
    GameStateBS GAMESTATE;
    public enum GameModeState { InProgress, Victory, Defeat, Quit };
    public GameModeState State = GameModeState.InProgress;

    // Use this for initialization
    void Start () {
        OBJECTIVES.AddRange(GetComponentsInChildren<ObjectiveBS>());
        GAMESTATE = GetComponentInParent<GameStateBS>();
    }
	
	// Update is called once per frame
	void Update () {

        // Change state?
        if (State == GameModeState.InProgress)
        {
            foreach (ObjectiveBS obj in OBJECTIVES)
            {
                switch (obj.objType)
                {
                    case ObjectiveBS.ObjType.WinLevel:
                        if (obj.finishedObjective == true)
                        {
                            print("WIN LEVEL!");
                            State = GameModeState.Victory;
                        }
                        break;
                    case ObjectiveBS.ObjType.LoseLevel:
                        if (obj.finishedObjective == true)
                        {
                            print("LOSE LEVEL!");
                            State = GameModeState.Defeat;
                        }
                        break;
                    case ObjectiveBS.ObjType.QuitLevel:
                        if (obj.finishedObjective == true)
                        {
                            print("QUIT LEVEL!");
                            State = GameModeState.Quit;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
