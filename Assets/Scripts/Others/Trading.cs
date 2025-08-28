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
    public UICoreScript uiCore;

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
        int amountToBuy = 1;
        if (uiCore != null && uiCore.tradeAmountInput != null && !string.IsNullOrEmpty(uiCore.tradeAmountInput.text) && int.TryParse(uiCore.tradeAmountInput.text, out int parsedAmount))
        {
            amountToBuy = parsedAmount;
        }

        if (amountToBuy <= 0) return;

        //check if theres anymore stock
        if (slot == 1)
        {
            if (amountToBuy > tradeamt1) return;
            int totalCost = basestoneprice * amountToBuy;
            if (gameManager.HasEnoughFunds(totalCost))
            {
                gameManager.RemoveFunds(totalCost);
                tradeamt1 -= amountToBuy;
                gameManager.AddStone(amountToBuy);
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
        if (slot == 2)
        {
            if (amountToBuy > tradeamt2) return;
            int totalCost = basemetalprice * amountToBuy;
            if (gameManager.HasEnoughFunds(totalCost))
            {
                gameManager.RemoveFunds(totalCost);
                tradeamt2 -= amountToBuy;
                gameManager.AddMetal(amountToBuy);
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
        if (slot == 3)
        {
            if (amountToBuy > tradeamt3) return;
            int totalCost = basewoodprice * amountToBuy;
            if (gameManager.HasEnoughFunds(totalCost))
            {
                gameManager.RemoveFunds(totalCost);
                tradeamt3 -= amountToBuy;
                gameManager.AddWood(amountToBuy);
                AudioManager.Instance.ForcePlaceSFX(4);
            }
        }
    }

    public void tradingSlots(int currentlvl)
    {
        if (currentlvl >= 2 && currentlvl < 4)
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
    }

    public void UpdateTradeCapacity(int newLevel)
    {
        baseminsold = 1;
        basemaxsold = 5;

        for (int i = 1; i < newLevel; i++)
        {
            baseminsold *= multiplier;
            basemaxsold *= multiplier;
        }
    }
}