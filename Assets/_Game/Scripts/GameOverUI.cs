using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreValue;
    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.OnScoreChange += updateScore;
        updateScore(GameController.Instance.score);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void updateScore(int score)
    {
        scoreValue.SetText(score.ToString());
    }
}
