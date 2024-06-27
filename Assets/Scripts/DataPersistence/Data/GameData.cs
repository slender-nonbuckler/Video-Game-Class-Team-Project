using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{   
    public int Score;
    public int Money;
    //public Car car;     
    //Not sure if we will have a car class. 


    //The value defined here is the default values 
    //a new game starts with when there's no data to load

    public GameData()
    {
        this.Score = 0;
        this.Money = 0;  //We may give the player a startup money to buy some parts.

    }

}
