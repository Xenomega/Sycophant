using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class UIListUnlockable : UIList
{
    [SerializeField] private GameObject _screenContainer;

    [SerializeField] private UIUnlocker _uIUnlockScreen;
    [SerializeField] private UnlockableType _unlockableType;

    [SerializeField] private UIListItemUnlockable _uIListItemUnlockablePrefab;
    [SerializeField] private UIBipedDesigner _uIBipedDesigner;
    
    protected override void Awake ()
    {
        base.Awake();
        foreach (Unlockable unlockable in GetUnlockables())
        {
            GameObject newGameObject = Instantiate(_uIListItemUnlockablePrefab.gameObject);
            UIListItemUnlockable newUIListItem = newGameObject.GetComponent<UIListItemUnlockable>();
            newUIListItem.Populate(unlockable, this, ClickCallback);
            Add(newUIListItem);
        }
    }

    private Unlockable[] GetUnlockables()
    {
        switch (_unlockableType)
        {
            case UnlockableType.Costume:
                return InvestmentManager.singleton.UnlockableCostumes;
            case UnlockableType.Effect:
                return InvestmentManager.singleton.UnlockableEffects;
        }
        return null;
    }

    private void ClickCallback(Unlockable unlockable, UIListItemUnlockable uIListItemUnlockable)
    {
        if (unlockable.Unlocked)
        {
            if (_unlockableType == UnlockableType.Costume)
                _uIBipedDesigner.SetCostume(unlockable);
            else if (_unlockableType == UnlockableType.Effect)
                _uIBipedDesigner.SetEffect(unlockable);
            Close();
        }
        else
        {
            _uIUnlockScreen.Show(unlockable, uIListItemUnlockable);
        }
    }

    internal void Close()
    {
        _screenContainer.SetActive(false);
    }
}
