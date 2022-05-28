using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TaskManager : MonoBehaviour
{
    [SerializeField] tasksGroupItem _taskGroupItem;
    [SerializeField] Transform _taskGroupitemsContainer;
    [SerializeField] Texture2D _handMouseTexture;
    static TaskManager  _instance;
    SqliteManager _sqlManager;
    tasksGroupItem _currentOpenTaskGroup;

    #region Mono Methods
    private void Awake()
    {
        _sqlManager = new SqliteManager();
        instantiateNewTaskGroup(new TaskData(),true);
        addTaskGroups();
    }
    private void OnApplicationQuit()
    {
        _sqlManager.closeConnection();
    }
    
    #endregion

    #region Methods
    /// <summary>
    /// Change default cursor for a hand.
    /// </summary>
    public void showMouseCursor()
    {
        Cursor.SetCursor(_handMouseTexture, Vector2.zero, CursorMode.Auto);
    }
    /// <summary>
    /// Change back to the default cursor.
    /// </summary>
    public void showDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    /// <summary>
    /// Fills task group list.
    /// </summary>
    private void addTaskGroups()
    {
        foreach (var taskGroup in _sqlManager.getTasksGroups())
        {
            instantiateNewTaskGroup(taskGroup);
        }
    }
    /// <summary>
    /// Instantiate a task group into the list
    /// </summary>
    /// <param name="taskData">The data to set.</param>
    /// <param name="isFirstTaskGroup">indicates if it is the main group that adds others.Can only be 1 of them</param>
    private void instantiateNewTaskGroup(TaskData taskData,bool isFirstTaskGroup=false)
    {
        var _go=Instantiate(_taskGroupItem, _taskGroupitemsContainer);
        _go.setData(taskData,isFirstTaskGroup);
    }
    /// <summary>
    /// Creates a new task group adn saves it in the database.
    /// </summary>
    /// <param name="taskData">The task group to create with at least 1 task item in the list.</param>
    /// <param name="isNewTaskGroup">If it is the task group creter task group or not.only the first one instantiated otehrwise is always false.</param>
    /// <returns>TRUE if the aperation is success FALSE otehrwise.</returns>
    public bool addNewTask(TaskData taskData,bool isNewTaskGroup=false)
    {
        taskData.Id =isNewTaskGroup?_sqlManager.insertTaskGroup(taskData):_sqlManager.updateTaskGroup(taskData);
        if (taskData.Id == -1)
            return false;
        if(isNewTaskGroup)
        instantiateNewTaskGroup(taskData);
        return true;
    }
    /// <summary>
    /// Updates a taskgroup data in the database.
    /// </summary>
    /// <param name="taskData">The updated task group.</param>
    /// <returns>TRUE if the operation is success FALSE otherwise.</returns>
    public bool updateEdittedTask(TaskData taskData)
    {
        return _sqlManager.updateTaskGroup(taskData) != -1;
    }
    #endregion

    #region Properties
    /// <summary>
    /// To not hold a reference.
    /// </summary>
    public static TaskManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<TaskManager>();
            return _instance;
        }
    }
    /// <summary>
    /// Stores and the last task group selected and close the previous one.
    /// </summary>
    public tasksGroupItem CurrentOpenTaskGroup
    {
         get=> _currentOpenTaskGroup;
        set
        {
            if (_currentOpenTaskGroup != null&&_currentOpenTaskGroup!=value)
                _currentOpenTaskGroup.closeTaskGroup();
            _currentOpenTaskGroup = value;
        }
    }
    
    #endregion

}
