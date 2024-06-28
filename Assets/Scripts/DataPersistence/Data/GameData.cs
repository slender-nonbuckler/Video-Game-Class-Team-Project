using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{   
    public int Score;
    public int Money;
    public List<string> UnlockedCars;

    public int getScore { get { return Score; } }
    public int getMoney { get { return Money; } }
    public List<string> getUnlockedCars { get { return UnlockedCars; } }

    public GameData()
    {
        this.Score = 0;
        this.Money = 0;
        //Player only gets the three bad cars
        this.UnlockedCars = new List<string> { "Ambulance", "DeliveryTruck", "FireTruck" };
    }



    public void UnlockCar(string car)
    {
        if(!this.UnlockedCars.Contains(car))
        {
            this.UnlockedCars.Add(car);
        }
    }
    public bool IsCarUnlocked(string car)
    {
        return this.UnlockedCars.Contains(car);
    }

    public void SetScore(int newScore)
    {
        this.Score = newScore;
    }

    public void SetMoney(int newMoney)
    {
        this.Money = newMoney;
    }
}
