using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;
using System;

public class SqliteManager
{
    string _databaseFilePath;
    const string _tableString = "CREATE TABLE tasks (id INTEGER PRIMARY KEY AUTOINCREMENT, tasklist VARCHAR NOT NULL) ";
    SqliteConnection _connection;
    public SqliteManager()
    {
        _databaseFilePath = Application.persistentDataPath + "\\data\\data.db3";
        bool _createNew = !File.Exists(_databaseFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(_databaseFilePath));
        _connection = new SqliteConnection("Data Source=" + _databaseFilePath);
        _connection.Open();
        if (_createNew)
            new SqliteCommand(_tableString, _connection).ExecuteNonQuery();
    }
    public void closeConnection()
    {
        _connection.Clone();
    }
    /// <summary>
    /// Gets all tasks form database.
    /// </summary>
    /// <returns>A list of taskdata.</returns>
    public List<TaskData> getTasksGroups()
    {
        List<TaskData> _tasks=new List<TaskData>();
        SqliteDataReader _reader = new SqliteCommand("SELECT * FROM tasks ORDER BY id ASC", _connection).ExecuteReader();
        while (_reader.Read())
            _tasks.Add(new TaskData(_reader.GetString(1),_reader.GetInt32(0)));
        _reader.Close();
        return _tasks;
    }
    /// <summary>
    /// Saves the task in the database.
    /// </summary>
    /// <param name="taskGroup">The task to insert.</param>
    /// <returns>The id of the last inserted data or -1 if failed.</returns>
    public int insertTaskGroup(TaskData taskGroup)
    {
        return insertUpdteData($"INSERT INTO tasks (tasklist)VALUES('{taskGroup.Tasks}')");
    }
    /// <summary>
    /// Updates the task group.
    /// </summary>
    /// <param name="taskData">The group to update.</param>
    /// <returns>TRUE if the update is successfully done, FALSE otherwise.</returns>
    public int updateTaskGroup(TaskData taskData)
    {
        return insertUpdteData($"UPDATE tasks SET tasklist='{taskData.Tasks}' WHERE id={taskData.Id}");
    }
    int insertUpdteData(string query)
    {
        int _id = -1;
        SqliteDataReader _reader = null;
        try
        {
            new SqliteCommand(query, _connection).ExecuteNonQuery();
            _reader = new SqliteCommand("SELECT * FROM tasks WHERE id = (SELECT MAX(id) FROM tasks)", _connection).ExecuteReader();
            if (_reader.Read())
                _id = _reader.GetInt32(0);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            _reader.Close();
            return _id;
        }
        _reader.Close();
        return _id;
    }

}

