using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class TaskData
{
    int _id;
    string _tasks;
    Toggle _toggle;

    #region Contructors
    public TaskData()
    {
        _tasks=string.Empty;
    }
    public TaskData(TaskData taskData)
    {
        _id= taskData._id;
        _tasks = taskData._tasks;
        _toggle = taskData._toggle;
    }
    public TaskData(string task,int id=0)
    {
        _id = id;
        addTask(task);
        _toggle = null;
    }
    internal void addTask(string newTask)
    {
        _tasks +="\n"+ newTask;
    }
    #endregion

    internal void editTask(TaskLine taskLine)
    {
        string[] _oldtasksList = TaskList;
        _tasks = string.Empty;
        for (int i = 0; i < _oldtasksList.Length; i++)
        {
            _tasks +="\n"+ (i == taskLine.LineIndex ? taskLine.LineData : _oldtasksList[i]);
        }
    }

    #region Proiperties
    public string[] TaskList { 
        get
        {
            List<string> _resut = new List<string>();
            foreach (var task in _tasks.Split("\n"))
            {
                if (!String.IsNullOrEmpty(task))
                    _resut.Add(task);
            }
            return _resut.ToArray();
        }
    }
    public int Id { get => _id; set => _id = value; }
    public Toggle Toggle { get => _toggle;}
    public string Tasks { get => _tasks;}
    public TaskLine LastInsertedTask
    {
        get
        {
            var _taskList = TaskList;
            int _id = _taskList.Length - 1;
            return new TaskLine(_id,TaskList[_id]);
        }
    }

    #endregion
}
public struct TaskLine
{
    public int LineIndex;
    public string LineData;

    public TaskLine(int lineIndex, string lineData)
    {
        LineIndex = lineIndex;
        LineData = lineData;
    }
}
