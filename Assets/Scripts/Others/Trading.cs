using UnityEngine;

public class Trading : MonoBehaviour
{
    public int multiplier = 2;
    public int basewoodprice = 3;
    public int basestoneprice = 5;
    public int basemetalprice = 5;

    public int minWoodPrice = 2;
    public int maxWoodPrice = 5;
    public int minStonePrice = 4;
    public int maxStonePrice = 8;
    public int minMetalPrice = 4;
    public int maxMetalPrice = 8;

    public int baseminsold = 1;
    public int basemaxsold = 5;
    //stone , metal , wood
    public int tradeamt1, tradeamt2, tradeamt3;
    public GameObject Slot2, Slot3;
    public GameManager gameManager;

    private int prevlvl;

    void Start()
    {
        gettradeamt();
    }
    //randomiser for getting the amount sold this turn
    public void gettradeamt()
    {
        basewoodprice = UnityEngine.Random.Range(minWoodPrice, maxWoodPrice + 1);
        basestoneprice = UnityEngine.Random.Range(minStonePrice, maxStonePrice + 1);
        basemetalprice = UnityEngine.Random.Range(minMetalPrice, maxMetalPrice + 1);

        tradeamt1 = UnityEngine.Random.Range(baseminsold, basemaxsold + 1);
        tradeamt2 = UnityEngine.Random.Range(baseminsold, basemaxsold + 1);
        tradeamt3 = UnityEngine.Random.Range(baseminsold, basemaxsold + 1);
    }

    //purchasing items
    public void purchasingstuff(int slot)
    {
        //check if theres anymore stock
        if (tradeamt1 > 0 && slot == 1)
        {
            //check if they have enough cash
            if (gameManager.HasEnoughFunds(basestoneprice))
            {
                //trade for the goods
                gameManager.AddFunds(-basestoneprice);

                //reduce the cost
                tradeamt1 -= 1;

                //give the item the the player
                gameManager.AddStone(1);

                //play sound effect
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
        if (tradeamt2 > 0 && slot == 2)
        {
            //check if they have enough cash
            if (gameManager.HasEnoughFunds(basemetalprice))
            {
                //trade for the goods
                gameManager.AddFunds(-basemetalprice);

                //reduce the cost
                tradeamt2 -= 1;

                //give the item the the player
                gameManager.AddMetal(1);

                //play sound effect
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
        if (tradeamt3 > 0 && slot == 3)
        {
            //check if they have enough cash
            if (gameManager.HasEnoughFunds(basewoodprice))
            {
                //trade for the goods
                gameManager.AddFunds(-basewoodprice);

                //reduce the cost
                tradeamt3 -= 1;

                //give the item the the player
                gameManager.AddWood(1);

                //play sound effect
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
    }

    //call upon upgrading docks
    public void tradingSlots(int currentlvl)
    {
        if (currentlvl != prevlvl)
        {
            baseminsold *= multiplier;
            basemaxsold *= multiplier;
        }


        if (currentlvl >= 2)
        {
            Slot2.SetActive(true);
        }
        else
        {
            Slot2.SetActive(false);
        }

        if (currentlvl >= 4)
        {
            Slot3.SetActive(true);
        }
        else
        {
            Slot3.SetActive(false);
        }

        prevlvl = currentlvl;
    }
}