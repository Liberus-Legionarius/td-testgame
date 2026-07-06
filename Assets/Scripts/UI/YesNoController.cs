using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YesNoController : MonoBehaviour
{
    [SerializeField] Button yes; 
    [SerializeField] TextMeshProUGUI text; 

    public void Init(string txt, Action Yes, int i = -1)
    {
        text.text = txt;
        yes.onClick.AddListener(() => Yes.Invoke());
        yes.onClick.AddListener(Cancel);
    }

    public void Cancel()
    {
        foreach(var i in FindObjectsByType<YesNoController>(FindObjectsSortMode.None))
            Destroy(i.gameObject);
    }



    
}
