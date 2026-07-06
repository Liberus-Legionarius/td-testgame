using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] protected GameObject[] hoverShow;
    public GameObject HoverShow {  get { return hoverShow[0]; } }
    [SerializeField] protected GameObject[] clickShow;
    [SerializeField] bool rewriteRotation = false;

    protected virtual void Update()
    {
        if (rewriteRotation)
            hoverShow[0].transform.rotation = Quaternion.identity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (GameObject go in hoverShow)
        {
            go.SetActive(true);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (GameObject go in hoverShow)
        {
            go.SetActive(false);
        }
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        foreach (var go in clickShow)
        {
           go.gameObject.SetActive(!go.gameObject.activeSelf);
        }

    }
}
