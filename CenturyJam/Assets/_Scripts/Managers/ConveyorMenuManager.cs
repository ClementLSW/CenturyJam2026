using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorMenuManager : MonoBehaviour
{
    public static ConveyorMenuManager Instance;
    [SerializeField] private List<ConveyorMenuVisual> conveyors;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void ShowConveyorMenu(int playerId)
    {
        switch (playerId)
        {
            case 0:
                conveyors[0].isConveyorShown = true;
                if (conveyors[2].isConveyorShown)
                {
                    conveyors[2].AnimatePosition(1,2);
                    conveyors[0].AnimatePosition(3, 2);
                }
                else
                {
                    conveyors[0].AnimatePosition(0,1);
                }
                break;
            case 1:
                conveyors[1].isConveyorShown = true;
                if (conveyors[3].isConveyorShown)
                {
                    conveyors[3].AnimatePosition(1,2);
                    conveyors[1].AnimatePosition(3, 2);
                }
                else
                {
                    conveyors[1].AnimatePosition(0,1);
                }
                break;
            case 2:
                conveyors[2].isConveyorShown = true;
                if (conveyors[0].isConveyorShown)
                {
                    Debug.Log("Here");
                    conveyors[0].AnimatePosition(1,2);
                    conveyors[2].AnimatePosition(3, 2);
                }
                else
                {
                    conveyors[2].AnimatePosition(0,1);
                }
                break;
            case 3:
                conveyors[3].isConveyorShown = true;
                if (conveyors[1].isConveyorShown)
                {
                    conveyors[1].AnimatePosition(1,2);
                    conveyors[3].AnimatePosition(3, 2);
                }
                else
                {
                    conveyors[3].AnimatePosition(0,1);
                }
                break;
        }
    }
    
    public void HideConveyorMenu(int playerId)
    {
        switch (playerId)
        {
            case 0:
                conveyors[0].isConveyorShown = false;
                if (conveyors[2].isConveyorShown)
                {
                    conveyors[0].AnimatePosition(2, 3);
                }
                else
                {
                    conveyors[2].AnimatePosition(2, 1);
                    conveyors[0].AnimatePosition(2,3);
                }
                break;
            case 1:
                conveyors[1].isConveyorShown = false;
                if (conveyors[3].isConveyorShown)
                {
                    conveyors[1].AnimatePosition(2, 3);
                }
                else
                {
                    conveyors[3].AnimatePosition(2,1);
                    conveyors[1].AnimatePosition(2,3);
                }
                break;
            case 2:
                conveyors[2].isConveyorShown = false;
                if (conveyors[0].isConveyorShown)
                {
                    conveyors[2].AnimatePosition(2, 3);
                }
                else
                {
                    conveyors[0].AnimatePosition(2,1);
                    conveyors[2].AnimatePosition(2,3);
                }
                break;
            case 3:
                conveyors[3].isConveyorShown = false;
                if (conveyors[1].isConveyorShown)
                {
                    conveyors[3].AnimatePosition(3, 2);
                }
                else
                {
                    conveyors[1].AnimatePosition(2,1);
                    conveyors[3].AnimatePosition(2,3);
                }
                break;
        }
    }
}
