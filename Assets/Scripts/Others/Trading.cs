using System;
using TMPro;
using UnityEngine;

public class Trading : MonoBehaviour
{
    public int mutliplier = 2;
    public int basewoodprice = 3;
    public int basestoneprice = 5;
    public int basemetalprice = 5;
    public int baseminsold = 1;
    public int basemaxsold = 2;
    //stone , metal , wood
    public int tradeamt1, tradeamt2, tradeamt3;
    public GameObject Slot2, Slot3;
    public GameManager gameManager;

    //randomiser for getting the amount sold this turn
    public void gettradeamt()
    {
        tradeamt1 = UnityEngine.Random.Range(baseminsold, basemaxsold);
        tradeamt2 = UnityEngine.Random.Range(baseminsold, basemaxsold);
        tradeamt3 = UnityEngine.Random.Range(baseminsold, basemaxsold);
    }

    //purchasing items
    public void purchasingstuff(int slot)
    {
        //check if theres anymore stock
        if (tradeamt1 > 0 && slot == 1)
        {
            //trade for the goods
            gameManager.AddFunds(-basestoneprice);

            //reduce the cost
            tradeamt1 -= 1;

            //give the item the the player
            gameManager.AddStone(1);
        }
        if (tradeamt2 > 0 && slot == 2)
        {
            //trade for the goods
            gameManager.AddFunds(-basemetalprice);

            //reduce the cost
            tradeamt2 -= 1;

            //give the item the the player
            gameManager.AddMetal(1);
        }
        if (tradeamt3 > 0 && slot == 3)
        {
            //trade for the goods
            gameManager.AddFunds(-basewoodprice);

            //reduce the cost
            tradeamt3 -= 1;

            //give the item the the player
            gameManager.AddWood(1);
        }
    }

    //call after turn change (maybe in this code)
    public void erachange()
    {
        basewoodprice *= mutliplier;
        basestoneprice *= mutliplier;
        basemetalprice *= mutliplier;
    }

    //call upon upgrading docks
    public void amountchange(int currentlvl)
    {
        baseminsold *= mutliplier;
        basemaxsold *= mutliplier;

        if (currentlvl == 2)
        {
            Slot2.SetActive(true);
        }
        if (currentlvl == 4)
        {
            Slot3.SetActive(true);
        }
    }
}
