using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed internal class UIListItemBipedDesign : UIListItem
{
    private BipedDesign _bipedDesign;
    private Action<BipedDesign, UIListItemBipedDesign> _bipedDesignCallback;

    internal void Populate(BipedDesign bipedDesign, Action<BipedDesign, UIListItemBipedDesign> callback)
    {
        _bipedDesign = bipedDesign;
        _bipedDesignCallback = callback;
        Display();
    }

    private void Display()
    {
        _rawImage.texture = InvestmentManager.singleton.CostumeSprites[_bipedDesign.SpriteIndex].texture;
        _text.text = _bipedDesign.Name;
    }

    internal void Refresh()
    {
        Display();
    }

    public override void CallbackClick()
    {
        _bipedDesignCallback.Invoke(_bipedDesign, this);
    }
}
