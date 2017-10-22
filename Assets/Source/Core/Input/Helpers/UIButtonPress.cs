using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

sealed internal class UIButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _pressedColor;

    private bool _pressed = false;
    private bool _hovering = false;

    [SerializeField] private UnityEvent _onPressed;
    [SerializeField] private UnityEvent _onHover;

    private MaskableGraphic _maskableGraphic;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        ChangeColor(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        Canvas.ForceUpdateCanvases();
        ChangeColor(false);
    }

    private void Awake()
    {
        _maskableGraphic = this.GetComponent<MaskableGraphic>();
    }

    private void Update()
    {
        if (_pressed && _hovering)
            _onPressed.Invoke();
        if (_hovering)
            _onHover.Invoke();
    }

    private void ChangeColor(bool enter)
    {
        if (_maskableGraphic == null)
            return;

        _maskableGraphic.color = enter ? _pressedColor : _defaultColor;
    }
}
