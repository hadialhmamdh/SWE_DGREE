using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net.Util;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Represents a task column in a board.
    /// </summary>
    internal class Column
    {
        /// <summary>
        /// Gets the category of the column (Backlog, InProgress, Done).
        /// </summary>
        public ColumnCategory Name { get; }
        private readonly Dictionary<int, Task> tasks;
        private const int NoLimit = -1;
        public ColumnDTO ColumnDto { get; set; }

        /// <summary>
        /// Gets or sets the task limit. Null means unlimited.
        /// </summary>
        private int limit;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a column with an empty task list and no limit.
        /// </summary>
        /// <param name="name">The category of the column.</param>
        public Column(ColumnCategory name)
        {
            ColumnDto = new ColumnDTO(name.ToString());
            this.Name = name;
            tasks = new Dictionary<int, Task>();
            Limit = NoLimit;
            ColumnDto.Save();
        }
        /// <summary>
        /// Constructs a column from an existing ColumnDTO. Task list is initialized empty.
        /// </summary>
        /// <param name="dto">The DTO containing persisted column data.</param>
        public Column(ColumnDTO dto)
        {
            ColumnDto = dto;
            this.Name = Enum.Parse<ColumnCategory>(dto.ColumnName);
            tasks = new Dictionary<int, Task>();
            limit = dto.Limit;
        }


        /// <summary>
        /// Gets or sets the task limit. -1 means unlimited.
        /// </summary>
        public int Limit
        {
            get => limit;
            set
            {
                if (value < NoLimit)
                {
                    log.Error($"Invalid column limit value: {value}. Limit must be -1 (unlimited) or a non-negative number.");
                    throw new ArgumentException("Limit must be -1 (unlimited) or a non-negative number.");
                }
                //
                if (value != NoLimit && value < tasks.Count)
                {
                    log.Error($"Limit value {value} is too low: there are already {tasks.Count} tasks in the column.");
                    throw new ArgumentException($"Limit cannot be lower than the current number of tasks ({tasks.Count}).");
                }
                ColumnDto.Limit = value;
                limit = value;
            }
        }

        /// <summary>
        /// Adds a task to the column if the column is not full.
        /// </summary>
        /// <param name="task">The task to add.</param>
        public void AddTask(Task task)
        {
            ColumnDto.AddTask(task.TaskDto);
            tasks[task.Id] = task;
            
        }

        /// <summary>
        /// Retrieves the task with the specified ID from the column.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>
        /// The task with the specified ID if it exists; otherwise, throws an <see cref="ArgumentException"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the task with the specified ID is not found.</exception>
        public Task GetTask(int id)
        {
            if (tasks.ContainsKey(id))
            {
                return tasks[id];
            }
            log.Error($"Task with ID {id} not found in column '{Name}'.");
            throw new ArgumentException("Task not found");
        }

        /// <summary>
        /// this method removes the task from the column by given ID.
        /// </summary>
        /// <param name="id"> the ID of the task to remove. </param>
        /// <returns> true if the task removede and false if there is no task with the given id to remove. </returns>
        public bool RemoveTask(int id)
        {
            return tasks.Remove(id);
        }

        /// <summary>
        /// Gets all tasks in the column.
        /// </summary>
        /// <returns>A list of tasks.</returns>
        public List<Task> GetAllTasks ()
        {
            return new List<Task>(tasks.Values);
        }

        /// <summary>
        /// Returns true if the column has reached its task limit.
        /// </summary>
        public bool IsFull()
        {
            return Limit != NoLimit && tasks.Count >= Limit;
        }
        public bool ContainTask(int id)
        {
            return tasks.ContainsKey(id);
        }

        public void UnAssignTasks(string email)
        {
            foreach (Task task in tasks.Values) 
            {
                if (task.Assignee == email) 
                { 
                    task.Assignee = null;
                }
            }
        }
        public void LoadTasks(int boardId)
        {
            List<TaskDTO> tasksD = new TaskController().GetTaskByIdColumn(boardId, Name.ToString());
            foreach (TaskDTO taskDto in tasksD)
            {
                Task task = new Task(taskDto);
                tasks[task.Id]=task;
            }
        }

        public override string ToString()
        {
            var taskStrings = tasks.Values.Select(t => t.ToString());
            return string.Join(Environment.NewLine + "----" + Environment.NewLine, taskStrings);
        }

    }
}
