using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkipShopTilesButton : MonoBehaviour
{
    public Button skipButton;
    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.OnUpdatePlayerFunds += toggleButton;
        skipButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Pay $" + GameController.Instance.skipPrice + " to skip");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void toggleButton(int funds)
    {
        if(funds < GameController.Instance.skipPrice)
        {
            skipButton.interactable = false;
        } else
        {
            skipButton.interactable = true;
        }
    }
}
