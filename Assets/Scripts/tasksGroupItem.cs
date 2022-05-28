using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    /// <summary>
    /// Indicates if this is the main task adder.Which doesnt have any tasklist is only the one creating other taskgroups.
    /// </summary>
     bool _isFirstTaskGroup;
    /// <summary>
    /// Controlles the visibility of the task group.
    /// </summary>
    TaskStates _currentState;
    /// <summary>
    /// To controll transparency of object to disable when needed.
    /// </summary>
    CanvasGroup[] _buttonCanvasGroups;
    /// <summary>
    /// Just a correction because layout controllers and 
    /// content fitters are incompatible between them when updating so i 
    /// have to desable this and reenable to refresh auto layout functions.
    /// (I dindt find a better way to do it.need more experience with canvas tasks.)
    /// </summary>
    VerticalLayoutGroup _LayoutGroup;
    /// <summary>
    /// The task group it represents.
    /// </summary>
    [HideInInspector]
    public TaskData _taskData;
    /// <summary>
    /// The editing line index of task.
    /// </summary>
    int _editingLineId;
    /// <summary>
    /// To controll the submit button texts.
    /// </summary>
    SubmitButtonStates _submitButtonState;

    #region Mono Methods
    private void Awake()
    {
        setButtonsCanvasGroups();
        _LayoutGroup = transform.parent.GetComponent<VerticalLayoutGroup>();
    }
    private void OnEnable()
    {
        _inputField.onValueChanged.AddListener(delegate { onInputValueChange(); });
    }
    private void OnDisable()
    {
        _inputField.onValueChanged.RemoveAllListeners();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Called everytime the input text changes.
    /// </summary>
    void onInputValueChange()
    {
        if (!isEditingTask)
            changeButtonsState(!isInputFieldEmpty);
        changeSubmitButtonText(submitButtonCorrectState);
    }
    /// <summary>
    /// Initializes data.
    /// </summary>
    /// <param name="taskData">Task group.</param>
    /// <param name="isFirstTaskGroup">If it is the main task adder or not.</param>
    internal void setData(TaskData taskData,bool isFirstTaskGroup)
    {
        _isFirstTaskGroup = isFirstTaskGroup;
        _taskData = taskData;
        addTasks();
        changeTaskGroupState(TaskStates.Close);
    }
    /// <summary>
    /// Fill the task group task list.
    /// </summary>
    private void addTasks()
    {
        clearTasksList();
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
    /// <summary>
    /// instantiate task object and initializes its data.
    /// </summary>
    /// <param name="_task">The task to add to the task group task list</param>
    /// <param name="lineIndex">The line index to be able to edit it later.</param>
    private void addTaskToList(string _task, int lineIndex)
    {
        Instantiate(_taskItem, _taskListContainer).setData(_task, lineIndex, this);
    }
    /// <summary>
    /// When pressed.
    /// </summary>
    public void OnSubmit()
    {
        if (isInputFieldEmpty)
        {
            closeTaskGroup();
            return;
        }
        //the result of database operation
        bool _operationResult = false;
        //the temporal taskgroup in case database operation fails.
        TaskData _tempTaskData = null;
        //perform the correct action
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
                    addTasks();
                }
                break;
        }
        //close taskgroup
        if(_operationResult)
        {
            closeTaskGroup();
            clearInputField();
        }
    }
    /// <summary>
    /// Load the canvasgroup of objects that need disable.
    /// </summary>
    private void setButtonsCanvasGroups()
    {
        _buttonCanvasGroups = new CanvasGroup[_buttons.Length];
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttonCanvasGroups[i] = _buttons[i].GetComponent<CanvasGroup>();
        }
    }
    /// <summary>
    /// Changes sumbmit button text.
    /// </summary>
    /// <param name="state">The text state to set.</param>
    private void changeSubmitButtonText(SubmitButtonStates state)
    {
        _submitButtonText.text =state.ToString();
    }
    /// <summary>
    /// Wait for changes to take place to reenable tasklayout to aviod the incopability with container fitter component.
    /// (The best way i could find).
    /// </summary>
    /// <returns></returns>
    IEnumerator fixeObjectsAligment()
    {
        yield return new WaitForEndOfFrame();
        _LayoutGroup.enabled = true;
    }
    /// <summary>
    /// open the task group and enable it to edit selected task item.
    /// </summary>
    /// <param name="taskIndex">The index of the task item to be edited.</param>
    public void editTask(int taskIndex)
    {
        changeButtonsState(true);
        _editingLineId = taskIndex;
        _inputField.text = _taskData.TaskList[_editingLineId];
        changeTaskGroupState(TaskStates.Open);
    }
    /// <summary>
    /// Prepare task group for adding a new task item to the group.
    /// </summary>
    public void addTask()
    {
        _editingLineId = -1;
        changeButtonsState(false);
        changeTaskGroupState(TaskStates.Open);
        clearInputField();
    }
    /// <summary>
    /// Executed when input field is clicked.
    /// </summary>
    public void onInputFieldClicked()
    {
        if (_currentState == TaskStates.Close)
            addTask();
    }
    /// <summary>
    /// Activates or deactivates the buttons and picture when input field is empty or not.
    /// </summary>
    /// <param name="interactable">TRUE ebales the interaction with buttons FALSE disables it</param>
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
    /// <summary>
    /// Enable or disable picture.
    /// </summary>
    /// <param name="_alpha">the alpha value set in the others object disabled or enabled.</param>
    private void changePictureState(float _alpha)
    {
        var _color = _PictureImage.color;
        _color.a = _alpha;
        _PictureImage.color = _color;
    }
    /// <summary>
    /// Activates or deactivates the submit button.
    /// </summary>
    /// <param name="activate"></param>
    private void changeSubmitButtonActivationState(bool activate)
    {
        _submitButton.interactable = activate;
    }
    /// <summary>
    /// Closes the task group
    /// </summary>
    public void closeTaskGroup()
    {
        changeTaskGroupState(TaskStates.Close);
        clearInputField();
    }
    /// <summary>
    /// Clears input field.
    /// </summary>
    private void clearInputField()
    {
        _inputField.text = "";
    }
    /// <summary>
    /// Changes task group state (open it or close it).
    /// </summary>
    /// <param name="state">The state to set to tsak group.</param>
    void changeTaskGroupState(TaskStates state)
    {
        //as i previous mentioned this avoid some visual problems when resizing objects when all the resizing finish i enable it again.
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
    #endregion

    #region Proiperties
    /// <summary>
    /// The text the sumbmit button should show.
    /// </summary>
    private SubmitButtonStates submitButtonCorrectState
    {
        get
        {
            if (!isEditingTask)
                _submitButtonState = isInputFieldEmpty ? SubmitButtonStates.Ok : SubmitButtonStates.Add;
            else
            {
                _submitButtonState = SubmitButtonStates.Save;
                changeSubmitButtonActivationState(!isInputFieldEmpty);
            }
            return _submitButtonState;
        }
    }
    /// <summary>
    /// Whenever the input field is empty or not.
    /// </summary>
    public bool isInputFieldEmpty => _inputField.text.Length == 0;
    /// <summary>
    /// Whenever is editing task or adding.
    /// </summary>
    public bool isEditingTask => _editingLineId != -1;
    #endregion
}
