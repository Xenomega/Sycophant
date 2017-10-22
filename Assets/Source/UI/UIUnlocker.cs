using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlocker : MonoBehaviour
{
    [SerializeField] private GameObject _buyCoinsScreen;
    [SerializeField] private Text _priceText;
    [SerializeField] private RawImage _previewRawImage;


    private Unlockable _unlockable;
    private UIListItemUnlockable _uIListItemUnlockable;

    internal void Show(Unlockable unlockable, UIListItemUnlockable uIListItemUnlockable)
    {
        this.gameObject.SetActive(true);
        _unlockable = unlockable;
        _uIListItemUnlockable = uIListItemUnlockable;
        _priceText.text = _unlockable.Price.ToString();
        _previewRawImage.texture = _unlockable.Preview.texture;
    }

    public void Unlock()
    {
        if (GameManager.singleton.Profile.Coins >= _unlockable.Price)
        {
            GameManager.singleton.Profile.Coins -= _unlockable.Price;
            _unlockable.Unlocked = true;
            GameManager.singleton.SaveProfile();
            _uIListItemUnlockable.Refresh();
            this.gameObject.SetActive(false);
        }
        else
        {
            _buyCoinsScreen.SetActive(true);
        }
    }
}
