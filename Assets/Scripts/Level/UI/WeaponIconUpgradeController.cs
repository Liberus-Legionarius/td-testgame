using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponIconUpgradeController : SpriteController
{
    int i;
    public void Init(GameObject upg, int n)
    {
        Sprite = upg.GetComponentInChildren<FirearmsController>().Sprite;
        i = n;
    }

    public void OnSelect()
    {
        transform.parent.parent.gameObject.GetComponentInChildren<TowerUIController>().Updating(i);
        foreach(var go in GameObject.FindGameObjectsWithTag("Upgrade"))
        {
            if(go != gameObject)
            {
                Destroy(go);
            }
        }
        Destroy(gameObject);
    }
}
