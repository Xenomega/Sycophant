using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class UIListNinjaDesign : UIList
{
    [SerializeField] private UIListItemBipedDesign _listItemBipedDesignPrefab;
    [SerializeField] private UIBipedDesigner _uIBipedDesigner;


    protected override void Awake ()
    {
        base.Awake();
        foreach (BipedDesign bipedDesign in GameManager.singleton.Profile.bipedDesigns)
        {
            GameObject newGameObject = Instantiate(_listItemBipedDesignPrefab.gameObject);
            UIListItemBipedDesign newUIListItem = newGameObject.GetComponent<UIListItemBipedDesign>();
            newUIListItem.Populate(bipedDesign, ClickCallback);
            Add(newUIListItem);
        }
    }

    private void ClickCallback(BipedDesign bipedDesign, UIListItemBipedDesign uIListItemBipedDesign)
    {
        this.gameObject.SetActive(false);
        _uIBipedDesigner.gameObject.SetActive(true);
        _uIBipedDesigner.Populate(bipedDesign, uIListItemBipedDesign);
    }
}
