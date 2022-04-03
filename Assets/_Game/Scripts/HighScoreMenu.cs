using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HighScoreMenu : MonoBehaviour
{
    public List<TextMeshProUGUI> scoreTexts = new List<TextMeshProUGUI>();
    // Start is called before the first frame update
    void Start()
    {
        foreach (TextMeshProUGUI text in scoreTexts) {
            text.SetText("");
        }

        List<int> scores = GameController.Instance.getHighScores();
        for (int i = 0; i < scores.Count; i++)
        {
            int s = scores[i];
            scoreTexts[i].SetText((i+1) + ". " + s);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
