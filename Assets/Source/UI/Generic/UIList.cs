using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
internal class UIList : MonoBehaviour
{
    [SerializeField] private AxisDirection _direction;
    [SerializeField] private List<UIListItem> _items;

    private RectTransform _rectTransform;
    private float _offset = 0;

    protected virtual void Awake()
    {
        _rectTransform = this.gameObject.GetComponent<RectTransform>();
    }

    internal void Add(UIListItem item)
    {
        if (_items.Count == 0)
            _rectTransform.sizeDelta = _direction == AxisDirection.Horizontal ? new Vector2(0, _rectTransform.sizeDelta.y) : new Vector2(_rectTransform.sizeDelta.x, 0);
        
        _items.Add(item);
        RectTransform rectTransform = item.gameObject.GetComponent<RectTransform>();
        rectTransform.SetParent(_rectTransform);
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = (_direction == AxisDirection.Horizontal ? Vector2.right : Vector2.up) * _offset;

        _offset += _direction == AxisDirection.Horizontal ? rectTransform.sizeDelta.x : rectTransform.sizeDelta.y;
        _rectTransform.sizeDelta = _direction == AxisDirection.Horizontal ? new Vector2(_offset, _rectTransform.sizeDelta.y) : new Vector2(_rectTransform.sizeDelta.x, _offset);

    }
    internal void Add(UIListItem[] items)
    {
        foreach (UIListItem item in items)
            Add(item);
    }

    internal void Clear()
    {
        _items = new List<UIListItem>();
        _offset = 0;
        foreach (RectTransform childRectTransform in _rectTransform)
            Destroy(childRectTransform.gameObject);
    }
}
