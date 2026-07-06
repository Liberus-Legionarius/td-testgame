using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LvlButtonController : ButtonController
{
    [SerializeField] TextMeshProUGUI lvl;
    [SerializeField] TextMeshProUGUI exp;
    [SerializeField] Image stars;

    public void OnLevelClick()
    {
        if(GetComponent<Button>().interactable)
            SceneManager.LoadScene(int.Parse(lvl.text) + 1);
        else isError = true;
    }

    public void Init(int x, int xp)
    {
        lvl.text = x.ToString();
        if (xp >= 0)
        {
            exp.text = xp.ToString();
            stars.fillAmount = xp/(float)GameController.LevelsGoal[x - 1];
        }
        else
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
