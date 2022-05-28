using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class taskItem : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] TextMeshProUGUI _taskInfo;
    [SerializeField] Toggle _toggle;
    int _id;
    tasksGroupItem _taskGroupItemReference;
    public int Id { get => _id; }
    public void OnPointerDown(PointerEventData eventData)
    {
        _taskGroupItemReference.editTask(_id);
    }
    public void onCheckedStateChange()
    {
        if(_toggle.isOn)
        OnPointerDown(null);
    }
    public void toggleO()
    {
        //TODO peddiente apagar todos los togles al cambiar rl foco
    }
    public void setData(string data, int lineIndex,tasksGroupItem _taskGroup)
    {
        _taskGroupItemReference = _taskGroup;
        _taskInfo.text = data;
        _id = lineIndex;
    }
}
