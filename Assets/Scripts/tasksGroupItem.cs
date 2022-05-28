using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using static TMPro.TMP_InputField;

public class tasksGroupItem : MonoBehaviour
{
    [SerializeField] float _buttonCanvasDeactivationValue = .5f;
    [Header("References")]
    [SerializeField] GameObject _addTaskObject;
    [SerializeField] GameObject _tasktListSeparator;
    [SerializeField] Image _PictureImage;
    [SerializeField] GameObject _tasksListObject;
    [SerializeField] GameObject _toolsObject;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] taskItem _taskItem;
    [SerializeField] Transform _taskListContainer;
    [SerializeField] TextMeshProUGUI _submitButtonText;
    [SerializeField] Button[] _buttons;
    [SerializeField] Button _submitButton;
    [Header("String formatting")]
    [SerializeField]List<StringFormattingData> _stringFormattData;
     bool _isFirstTaskGroup;
    TaskStates _currentState;
    CanvasGroup[] _buttonCanvasGroups;
    VerticalLayoutGroup _LayoutGroup;
    [HideInInspector]
    public TaskData _taskData;
    int _editingLineId;
    bool _openLinkTag;
    bool _formattedStringDetected;
    string _lastStringBeforeChange=string.Empty;
    SubmitButtonStates _submitButtonState;
    private void Awake()
    {
        setButtonsCanvasGroups();
        _LayoutGroup = transform.parent.GetComponent<VerticalLayoutGroup>();
    }
    private void OnEnable()
    {
        _inputField.onValidateInput += onInputValueChange;
        enableInputOnValueChangeCallback();
    }
    private void OnDisable()
    {
        _inputField.onValidateInput -= onInputValueChange;
        disableInputOnVlueChangeCallback();
    }
    private void Update()
    {
        
    }

    #region Methods
    private void disableInputOnVlueChangeCallback()
    {
        _inputField.onValueChanged.RemoveAllListeners();
    }
    private void enableInputOnValueChangeCallback()
    {
        _inputField.onValueChanged.AddListener(delegate { onInputValueChange(); });
    }
    void onInputValueChange()
    {
        if (_formattedStringDetected)
        {
            _inputField.stringPosition = _inputField.text.Length;
            _formattedStringDetected = false;
        }
        clearEmptyInputTags();
        if (!isEditingTask)
            changeButtonsState(!isInputFieldEmpty);
        changeSubmitButtonText(submitButtonCorrectState);
        Debug.Log(_inputField.text);
        _lastStringBeforeChange = _inputField.text;
    }
    private void clearEmptyInputTags()
    {
        bool _checkCharacters = false;
        foreach (var stringFormat in _stringFormattData)
        {
            if (_lastStringBeforeChange.Contains(stringFormat.Char))
            {
                _checkCharacters = true;
                break;
            }
        }
        //check de diference of the two string if is missing any key char them locate the tags adn delete them
        if (_checkCharacters)
        {
            string _inputString = _inputField.text;
            for (int i = 0; i < _lastStringBeforeChange.Length; i++)
            {
                char _oldChar = _lastStringBeforeChange[i];
                if (isKeyChar(_oldChar)) {
                    if (i==_inputString.Length||_oldChar != _inputString[i])
                    {
                        int _linkStartIndex = _inputString.LastIndexOf("<link", i);
                        disableInputOnVlueChangeCallback();
                        _inputField.text = _inputString.Remove(_linkStartIndex, i - _linkStartIndex);
                        enableInputOnValueChangeCallback();
                        _openLinkTag = false;
                    }
                }
            }
        }
    }

    internal void setData(TaskData taskData,bool isFirstTaskGroup)
    {
        _isFirstTaskGroup = isFirstTaskGroup;
        _taskData = taskData;
        addTasks();
        changeTaskState(TaskStates.Close);
    }
    private void addTasks(bool clearList=false)
    {
        if (clearList)
        {
            clearTasksList();
        }
        string[] _tasks = _taskData.TaskList;
        for (int i = 0; i < _tasks.Length; i++)
        {
                addTaskToList(_tasks[i], i);
        }
    }
    /// <summary>
    /// Clears the task list.
    /// </summary>
    private void clearTasksList()
    {
        foreach (Transform task in _taskListContainer)
        {
            Destroy(task.gameObject);
        }
    }
    private void addTaskToList(string _task, int lineIndex)
    {
        Instantiate(_taskItem, _taskListContainer).setData(_task, lineIndex, this);
    }
    public void OnSubmit()
    {
        if (isInputFieldEmpty)
        {
            closeTaskList();
            return;
        }
        bool _operationResult = false;
        TaskData _tempTaskData = null;
        switch (_submitButtonState)
        {
            case SubmitButtonStates.Add:
                if (_isFirstTaskGroup)
                {
                    _tempTaskData = new TaskData(_inputField.text);
                    if (TaskManager.Instance.addNewTask(_tempTaskData, true))
                    {
                        _operationResult = true;
                        _taskData = _tempTaskData;
                    }
                }
                else
                {
                    _tempTaskData = new TaskData(_taskData);
                    _tempTaskData.addTask(_inputField.text);
                    if (TaskManager.Instance.addNewTask(_tempTaskData))
                    {
                        _operationResult = true;
                        _taskData = _tempTaskData;
                        //add new task
                        var _lastInsertedTask = _taskData.LastInsertedTask;
                        addTaskToList(_lastInsertedTask.LineData, _lastInsertedTask.LineIndex);
                    }
                }
                break;
            case SubmitButtonStates.Save:
                _tempTaskData = new TaskData(_taskData);
                _tempTaskData.editTask(new TaskLine(_editingLineId,_inputField.text));
                if (TaskManager.Instance.updateEdittedTask(_tempTaskData))
                {
                    _taskData = _tempTaskData;
                    _operationResult = true;
                    addTasks(true);
                }
                break;
        }
        //close taskgroup
        if(_operationResult)
        {
            closeTaskList();
            clearInputField();
        }
    }
    private void setButtonsCanvasGroups()
    {
        _buttonCanvasGroups = new CanvasGroup[_buttons.Length];
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttonCanvasGroups[i] = _buttons[i].GetComponent<CanvasGroup>();
        }
    }
    public char onInputValueChange(string a, int position, char character)
    {
        string _stringToAdd = string.Empty;
        if (!_openLinkTag)
        {
            if (char.Equals(character, '@'))
            {
                _openLinkTag = true;
                StringFormattingData _stringFormattData = getstringFormattData(StringFormattingKeywords.At_Sign);
                _formattedStringDetected = true;
                _stringToAdd = $"<link=''><color=#{ColorUtility.ToHtmlStringRGBA(_stringFormattData.Color)}>@";
                _lastStringBeforeChange = _inputField.text + _stringToAdd;
                _inputField.text += _stringToAdd;
                character = new char();
            }
        }
        if (character == ' ')
        {
            if (_openLinkTag)
                _stringToAdd = "</color></link> ";
            else
                _stringToAdd = " ";
            _lastStringBeforeChange = _inputField.text + _stringToAdd;
            _inputField.text += _stringToAdd;
            _formattedStringDetected = true;
            _openLinkTag = false;
            character = new char();
        }
        return character;
    }
    private void changeSubmitButtonText(SubmitButtonStates state)
    {
        _submitButtonText.text =state.ToString();
    }
    IEnumerator fixeObjectsAligment()
    {
        yield return new WaitForEndOfFrame();
        _LayoutGroup.enabled = true;
    }
    public void editTask(int taskIndex)
    {
        changeButtonsState(true);
        _editingLineId = taskIndex;
        _inputField.text = _taskData.TaskList[_editingLineId];
        changeTaskState(TaskStates.Open);
    }
    public void addTask()
    {
        _editingLineId = -1;
        changeButtonsState(false);
        changeTaskState(TaskStates.Open);
        clearInputField();
    }
    public void onInputFieldClicked()
    {
        if (_currentState == TaskStates.Close)
            addTask();
    }
    private void changeButtonsState(bool interactable)
    {
        float _alpha = interactable ? 1 : _buttonCanvasDeactivationValue;
        for (int i = 0; i < _buttons.Length; i++)
        {

            _buttons[i].interactable = interactable;
            _buttonCanvasGroups[i].alpha = _alpha;
        }
        changePictureState(_alpha);        
    }
    private void changePictureState(float _alpha)
    {
        var _color = _PictureImage.color;
        _color.a = _alpha;
        _PictureImage.color = _color;
    }
    private void changesubmitButtonActivationState(bool activate)
    {
        _submitButton.interactable = activate;
    }
    public void closeTaskList()
    {
        changeTaskState(TaskStates.Close);
        clearInputField();
    }
    private void clearInputField()
    {
        _inputField.text = "";
    }
    void changeTaskState(TaskStates state)
    {
        _LayoutGroup.enabled = false;
        _currentState = state;
        switch (_currentState)
        {
            case TaskStates.Close:
                _addTaskObject.SetActive(_isFirstTaskGroup);
                _tasksListObject.SetActive(!_isFirstTaskGroup);
                _PictureImage.transform.parent.gameObject.GetComponent<Image>().enabled=false;
                _PictureImage.gameObject.SetActive(false);
                _toolsObject.SetActive(false);
                _tasktListSeparator.SetActive(_addTaskObject.activeSelf);
                break;
            case TaskStates.Open:
                _PictureImage.transform.parent.gameObject.GetComponent<Image>().enabled = true;
                _PictureImage.gameObject.SetActive(true);
                _addTaskObject.SetActive(true);
                _tasksListObject.SetActive(true);
                _toolsObject.SetActive(true);
                TaskManager.Instance.CurrentOpenTaskGroup = this;
                break;
        }
        StartCoroutine(fixeObjectsAligment());
    }
    StringFormattingData getstringFormattData(StringFormattingKeywords keyWord) => _stringFormattData.Find(_stringFormatt=>_stringFormatt.FormattKeyword==keyWord);
    public bool isKeyChar(char charact)
    {
        foreach (var stringFormat in _stringFormattData)
        {
            if (charact == stringFormat.Char)
                return true;
        }
        return false;
    }
    #endregion

    #region Proiperties
    private SubmitButtonStates submitButtonCorrectState
    {
        get
        {
            if (!isEditingTask)
                _submitButtonState = isInputFieldEmpty ? SubmitButtonStates.Ok : SubmitButtonStates.Add;
            else
            {
                _submitButtonState = SubmitButtonStates.Save;
                changesubmitButtonActivationState(!isInputFieldEmpty);
            }
            return _submitButtonState;
        }
    }
    public bool isInputFieldEmpty => _inputField.text.Length == 0;
    public bool isEditingTask => _editingLineId != -1;
    #endregion
}
enum SubmitButtonStates
{
    None,
    Ok,
    Add,
    Save
}
[Serializable]
 struct StringFormattingData
{
    public StringFormattingKeywords FormattKeyword;
    public Color Color;
    public Char Char;
}
enum StringFormattingKeywords
{
    None,
    At_Sign
}
