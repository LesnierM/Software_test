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
    static TaskManager  _instance;
    SqliteManager _sqlManager;
    tasksGroupItem _currentOpenTaskGroup;
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

    #region Methods
    private void addTaskGroups()
    {
        foreach (var taskGroup in _sqlManager.getTasksGroups())
        {
            instantiateNewTaskGroup(taskGroup);
        }
    }
    private void instantiateNewTaskGroup(TaskData taskData,bool isFirstTaskGroup=false)
    {
        var _go=Instantiate(_taskGroupItem, _taskGroupitemsContainer);
        _go.setData(taskData,isFirstTaskGroup);
    }
    public bool addNewTask(TaskData taskData,bool isNewTaskGroup=false)
    {
        taskData.Id =isNewTaskGroup?_sqlManager.insertTaskGroup(taskData):_sqlManager.updateTaskGroup(taskData);
        if (taskData.Id == -1)
            return false;
        if(isNewTaskGroup)
        instantiateNewTaskGroup(taskData);
        return true;
    }
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
                _currentOpenTaskGroup.closeTaskList();
            _currentOpenTaskGroup = value;
        }
    }
    
    #endregion
}
