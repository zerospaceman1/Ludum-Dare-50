using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreValue;
    [SerializeField]
    private TextMeshProUGUI shopTimerValue;
    [SerializeField]
    private TextMeshProUGUI shopPlayerFundsValue;
    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.OnScoreChange += updateScore;
        GameController.Instance.OnUpdateShopTimer += updateShopTimer;
        GameController.Instance.OnUpdatePlayerFunds += updatePlayerFunds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void updateScore(int score)
    {
        scoreValue.SetText(score.ToString());
    }

    private void updateShopTimer(int time)
    {
        shopTimerValue.SetText(time.ToString());
    }

    private void updatePlayerFunds(int funds)
    {
        shopPlayerFundsValue.SetText("$" + funds.ToString());
    }
}
