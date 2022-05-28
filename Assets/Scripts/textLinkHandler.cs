using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class textLinkHandler : MonoBehaviour, IPointerMoveHandler
{
    TMP_Text _textField;
    [SerializeField] Texture2D _handCursor;
    private void Awake()
    {
        _textField = GetComponent<TextMeshProUGUI>();
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        if (isCursorOnLink(eventData.position))
            TaskManager.Instance.showMouseCursor();
        else
            TaskManager.Instance.showDefaultCursor();
    }
    bool isCursorOnLink(Vector2 mousePosition)
    {
            int _linkIndex = TMP_TextUtilities.FindIntersectingLink(_textField, mousePosition, Camera.main);
            return _linkIndex != -1;
    }
}

