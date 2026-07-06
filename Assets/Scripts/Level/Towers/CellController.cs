using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CellController : MonoBehaviour, IPointerClickHandler
{
    LevelController level;
    UILevelController ui;
    [SerializeField] bool canChange;

    public bool CanChange
    {
        get { return canChange; }
    }
    
    GameObject weapon; 
    public GameObject Weapon
    {
        get => weapon;
        set
        {
            weapon = value;
        }
    }
    void Start()
    {
        level = LevelController.GetLevel();
        ui = level.GetUI();
    }
    
    public void OnPointerClick(PointerEventData eventData) 
    {
        if( Weapon == null && ui.SelectedWeapon != null)
        {
            Weapon = level.PlaceWeapon(transform.parent);
            return;
        }
    }
    public void SelfUpdate(GameObject newCell)
    {
        var cell = Instantiate(newCell, transform.parent.parent);
        cell.transform.position = transform.position;
        cell.transform.rotation = transform.rotation;
        Destroy(transform.parent.gameObject);
    }
}
