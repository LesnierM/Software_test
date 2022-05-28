using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class taskItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] TextMeshProUGUI _taskInfo;
    [SerializeField] Toggle _toggle;
    [Header("String formatting")]
    [SerializeField] List<StringFormattingData> _stringFormattData;
    int _id;
    /// <summary>
    /// Its task group container.
    /// </summary>
    tasksGroupItem _taskGroupItemReference;
    /// <summary>
    /// The line of the data in the tasks string.
    /// </summary>
    public int Id { get => _id; }
    /// <summary>
    /// Captures pointer down event to edit task.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        _taskGroupItemReference.editTask(_id);
    }
    /// <summary>
    /// Open taskgroup.
    /// </summary>
    public void onCheckedStateChange()
    {
        if (_toggle.isOn)
            OnPointerDown(null);
    }
    /// <summary>
    /// Initializes data fields.
    /// </summary>
    /// <param name="data">The string to show.</param>
    /// <param name="lineIndex">The line in tasks.</param>
    /// <param name="_taskGroup">Task group it belongs to.</param>
    public void setData(string data, int lineIndex, tasksGroupItem _taskGroup)
    {
        _taskGroupItemReference = _taskGroup;
        _id = lineIndex;
        //formatt string
        formatText(ref data);
        _taskInfo.text = data;
    }
    /// <summary>
    /// Formatt the string.
    /// </summary>
    /// <param name="data">The string to formatt</param>
    private void formatText(ref string data)
    {
        string _formattedString = string.Empty;
        for (int i = 0; i < data.Length; i++)
        {
            bool _addChar = false;
            foreach (var _stringFormattData in _stringFormattData)
            {
                _addChar = false;//reset ad character in each check in case encauter a keystring in later items.
                //if char equals to first character in keystring check the rest to confirm if it is a keystring
                string _posibleKeyString = string.Empty;
                if (data[i].Equals(_stringFormattData.KeyString[0]))
                    _posibleKeyString = data.Substring(i, _stringFormattData.KeyString.Length);
                //start formatting
                if (_posibleKeyString.Equals(_stringFormattData.KeyString))
                {
                    int _endIndex = data.IndexOf(' ', i);
                    int _wordStartIndex = -1;
                    if (_endIndex == -1)
                        _endIndex = data.Length;
                    if (_stringFormattData.IncludeWholeWord)
                    {
                        _wordStartIndex = data.LastIndexOf(' ', i);
                        if (_wordStartIndex == -1)
                            _wordStartIndex = 0;
                        else
                            _wordStartIndex++;//ignore space
                        //correct formatted string removing extra characteres
                        _formattedString = _formattedString.Substring(0, _formattedString.Length - (i - _wordStartIndex));
                    }
                    int _tempStartIndex = (_wordStartIndex != -1 ? _wordStartIndex : i);
                    string _stringToFormatt = data.Substring(_tempStartIndex, _endIndex - _tempStartIndex);
                    //conditions to ignore key character
                    if ((!string.IsNullOrEmpty(_stringFormattData.CantContain) && _stringToFormatt.Contains(_stringFormattData.CantContain)) || (
                        !string.IsNullOrEmpty(_stringFormattData.ConditionalString) && !_stringToFormatt.Contains(_stringFormattData.ConditionalString)))
                    {
                        continue;
                    }
                    _formattedString += $"<link=''><color=#{ColorUtility.ToHtmlStringRGBA(_stringFormattData.Color)}>{_stringToFormatt}</color></link>";
                    //go to the end of string minus 1 (due to for increment) to formatt to keep checking character
                    i = _endIndex - 1;
                    break;
                }
                else
                    _addChar = true;
            }
            if (_addChar)
                _formattedString += data[i];
        }
        data = _formattedString;
    }

    //previous try but buggy
    //private void formatText(ref string data)
    //{
    //    int _startIndex = 0;

    //    for (int i = 0; i < _stringFormattData.Count; i++)
    //    {
    //        if (_startIndex == -1)
    //            _startIndex = 0;
    //        _startIndex = data.IndexOf(_stringFormattData[i].KeyString, _startIndex);
    //        while (_startIndex != -1)
    //        {
    //            //take all text to the right
    //            int _endIndex = data.IndexOf(' ', _startIndex);
    //            int _wordStartIndex = -1;
    //            if (_endIndex == -1)
    //                _endIndex = data.Length;
    //            if (_stringFormattData[i].IncludeWholeWord)
    //            {
    //                _wordStartIndex = data.LastIndexOf(' ', _startIndex);
    //                if (_wordStartIndex == -1)
    //                    _wordStartIndex = 0;
    //                else
    //                    _wordStartIndex++;//ignore space
    //            }
    //            int _tempStartIndex = (_wordStartIndex != -1 ? _wordStartIndex : _startIndex);
    //            string _stringToFormatt = data.Substring(_tempStartIndex, _endIndex -_tempStartIndex);
    //            //conditions to ignore key character
    //            if ((!string.IsNullOrEmpty(_stringFormattData[i].CantContain) && _stringToFormatt.Contains(_stringFormattData[i].CantContain)) || (
    //                !string.IsNullOrEmpty(_stringFormattData[i].ConditionalString) && !_stringToFormatt.Contains(_stringFormattData[i].ConditionalString)))
    //            {
    //                break;
    //            }
    //            string _formattedString = $"<link=''><color=#{ColorUtility.ToHtmlStringRGBA(_stringFormattData[i].Color)}>{_stringToFormatt}</color></link>";
    //            data = data.Remove(_tempStartIndex, _stringToFormatt.Length);
    //            data = data.Insert(_tempStartIndex, _formattedString);
    //            int _tempStartIndex2 = data.IndexOf(_stringFormattData[i].KeyString, _tempStartIndex + _formattedString.Length);
    //            if (_tempStartIndex2 == -1)
    //            {
    //                _startIndex = (_wordStartIndex!=-1?_wordStartIndex:_startIndex) + _formattedString.Length;
    //                break;
    //            }
    //            else
    //                _startIndex = _tempStartIndex2;
    //        }
    //    }
    //}
}
