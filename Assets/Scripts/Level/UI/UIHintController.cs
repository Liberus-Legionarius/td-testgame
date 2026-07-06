using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHintController : UIController, IPointerMoveHandler
{
    [SerializeField] GameObject hint;
    [SerializeField] [Multiline] string[] hintText;
    private void Start()
    {
        hoverShow = new GameObject[] { Instantiate(hint, transform) };
        var txt = hoverShow[0].GetComponentInChildren<TextMeshProUGUI>();
        txt.text = hintText[GameController.Language];
        var img = hoverShow[0].GetComponent<RectTransform>();
        var size = txt.GetPreferredValues();
        img.sizeDelta = new Vector2(size.x + 20, size.y + 20);
        if (img.rect.width + img.transform.position.x  + 100> Screen.width)
        {
            img.pivot = new Vector2(1,0);
        }
        hoverShow[0].SetActive(false);

    }
    public void OnPointerMove(PointerEventData eventData)
    {
        foreach(var obj in hoverShow)
        {
            obj.transform.position = eventData.position;
        }
    }
}
