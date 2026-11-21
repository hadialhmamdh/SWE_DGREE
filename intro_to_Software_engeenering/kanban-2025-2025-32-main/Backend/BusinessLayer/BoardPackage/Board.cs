using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using IntroSE.Kanban.Backend.ServiceLayer;
using log4net;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Board
    {
        public string Name { get; }
        public string Owner { get; set; }
        public int Id { get; }
        public List<string> Members { get; }
        internal BoardDTO BoardDTO { get; set; }
        public int TaskCounter { get; private set; }
        private readonly Dictionary<ColumnCategory, Column> columns;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// this method constructs a new board with the given name and creates 3 initial default columns (Backlog, InProgress, Done).
        /// </summary>
        /// <param name="name"> the name of the board to create. </param>
        public Board(string name, int id, string owner)
        {
            BoardDTO = new BoardDTO(id, name, owner, 0);
            Name = name;
            this.Owner = owner;
            TaskCounter = 0;
            columns = new Dictionary<ColumnCategory, Column> {
                {ColumnCategory.Backlog, new Column(ColumnCategory.Backlog) },
                {ColumnCategory.InProgress, new Column(ColumnCategory.InProgress) },
                {ColumnCategory.Done, new Column(ColumnCategory.Done) },
                };
            Members = new List<string>();
            Id = id;
            this.BoardDTO.Save();
            InitColumns();
        }

        private void InitColumns()
        {
            BoardDTO.AddColumn((columns[ColumnCategory.Backlog].ColumnDto));
            BoardDTO.AddColumn((columns[ColumnCategory.InProgress].ColumnDto));
            BoardDTO.AddColumn((columns[ColumnCategory.Done].ColumnDto));
        }

        public Board(BoardDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Owner = dto.Owner;
            TaskCounter = dto.TaskCounter;
            BoardDTO = dto;
            this.columns = LoadColumns(dto.Id);
            //
            this.Members = new BoardMemberController().GetMembersOfBoard(Id);
        }
        private Dictionary<ColumnCategory, Column> LoadColumns(int boardId)
        {
            List<ColumnDTO> columnDTOs = new ColumnController().GetColumnById(boardId);
            Dictionary<ColumnCategory, Column> columns = new Dictionary<ColumnCategory, Column>();

            foreach (ColumnDTO dto in columnDTOs)
            {
                Column column = new Column(dto);
                column.LoadTasks(boardId); 
                columns[column.Name] = column;
            }

            log.Info($"Loaded {columns.Count} columns for board ID {boardId}.");
            return columns;
        }

        /// <summary>
        /// this method creates a new task and add it to the backlog column.
        /// </summary>
        /// <param name="email"> the email of the user. </param>
        /// <param name="title"> the title of the task. </param>
        /// <param name="description">the description of the task (not mandatory). </param>
        /// <param name="dueDate"> the due date of the task. </param>
        /// <returns> the created task. </returns>
        public Task AddTask(string email, string title, string description, DateTime dueDate)
        {
            if (!(IsFull(ColumnCategory.Backlog)))
            {
                try
                {
                    Task task = new Task(TaskCounter, title, description, DateTime.Now, dueDate);
                    IncrementTaskCounter();
                    columns[ColumnCategory.Backlog].AddTask(task);
                    return task;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
            else
            {
                log.Error($"AddTask failed: backlog column is full for board '{Name}' of user {email}.");
                throw new ArgumentException("Backlog column is full.");
            }
        }

        /// <summary>
        /// this method moves the task from the backlog column to the in progress column.
        /// </summary>
        /// <param name="id"> the ID of the task we want to move.</param>
        /// <returns> true if successfully moved and false otherwise. </returns>
        public bool MoveTaskToProgress(int id, string email)
        {
            try
            {
                Task task = columns[ColumnCategory.Backlog].GetTask(id);
                if (task.Assignee != email)
                {

                    log.Error($"MoveTask to progress failed: user '{email}' is not the assignee of task {id}.");
                    throw new ArgumentException("Only the task assignee can move the task.");
                }
                if (columns[ColumnCategory.InProgress].IsFull())
                {
                    log.Error($"MoveTask {id}To Progress failed: InProgress column is full.");
                    throw new ArgumentException("Cannot move task: InProgress column is full.");
                }
                columns[ColumnCategory.Backlog].RemoveTask(id);
                columns[ColumnCategory.InProgress].AddTask(task);

                return true;

            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }

        /// <summary>
        /// this method moves the task from the in prgoress column to the done column.
        /// </summary>
        /// <param name="id"> the ID of the task we want to move.</param>
        /// <returns> true if successfully moved and false otherwise. </returns>
        public bool MoveTaskToDone(int id, string email)
        {
            try
            {
                Task task = columns[ColumnCategory.InProgress].GetTask(id);
                if (task.Assignee != email)
                {

                    log.Error($"MoveTask to done failed: user '{email}' is not the assignee of task {id}.");
                    throw new ArgumentException("Only the task assignee can move the task.");
                }
                if (columns[ColumnCategory.Done].IsFull())
                {
                    log.Error($"MoveTask {id}To Done failed:Done column is full.");
                    throw new ArgumentException("Cannot move task: Done column is full.");
                }
                columns[ColumnCategory.InProgress].RemoveTask(id);
                columns[ColumnCategory.Done].AddTask(task);

                return true;

            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// this method sets the task limit in the given column.
        /// </summary>
        /// <param name="column"> the column we want to set the task limit in. </param>
        /// <param name="limit"> the limit number of tasks to add to the given column. </param>
        public void SetLimit(ColumnCategory column, int limit)
        {
            try
            {
                columns[column].Limit = limit;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }

        /// <summary>
        /// this method gets the task limit in the given column.
        /// </summary>
        /// <param name="column"> the column we want to get the limit task number from. </param>
        /// <returns></returns>
        public int GetLimit(ColumnCategory column)
        {
            return columns[column].Limit;
        }

        /// <summary>
        /// this method gets all the tasks in the in progress column.
        /// </summary>
        /// <returns> a list of the tasks in the in progress column. </returns>
        public List<Task> GetAllInProgress()
        {
            return columns[ColumnCategory.InProgress].GetAllTasks();
        }
        public List<Task> GetAllTasksOfAssign(string email)
        {
            List<Task> list = new List<Task>();
            foreach (Task task in GetAllInProgress())
            {
                if (task.Assignee == email)
                {

                    list.Add(task);
                }
            }
            return list;
        }

        public void IncrementTaskCounter()
        {
            TaskCounter++;
        }
        public bool IsFull(ColumnCategory column)
        {
            return columns[column].IsFull();

        }
        /// <summary>
        /// Determines which column contains the task with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the task to search for.</param>
        /// <returns>The column category in which the task resides.</returns>
        /// <exception cref="ArgumentException">Thrown when the task does not exist in any column.</exception>
        public ColumnCategory ContainTask(int id)
        {
            if (columns[ColumnCategory.Backlog].ContainTask(id))
            {
                return ColumnCategory.Backlog;
            }
            else if (columns[ColumnCategory.InProgress].ContainTask(id))
            {
                return ColumnCategory.InProgress;
            }
            else if (columns[ColumnCategory.Done].ContainTask(id))
            {
                return ColumnCategory.Done;
            }
            throw new ArgumentException($"Task with ID {id} does not exist in any column.");
        }

        /// <summary>
        /// Retrieves the task with the specified ID from the given column.
        /// </summary>
        /// <param name="column">The column category to search in (Backlog, InProgress, Done).</param>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>The task with the specified ID.</returns>
        public Task GetTask(ColumnCategory column, int id)
        {
            return columns[column].GetTask(id);
        }

        public List<Task> GetAllTasks(ColumnCategory column)
        {
            return columns[column].GetAllTasks();
        }

        internal bool IsMember(string email)
        {
            return Members.Contains(email);
        }

        internal bool IsOwner(string email)
        {
            return Owner == email;
        }

        internal void AddMember(string email)
        {
            Members.Add(email);
        }

        public void AssignTask(string email, ColumnCategory column, int taskID, string emailAssignee)
        {
            try
            {
                ColumnCategory columnCategory = ContainTask(taskID);
                //
                if (columnCategory == ColumnCategory.Done)
                {
                    throw new ArgumentException("Cannot edit tasks that are already Done.");
                }
                //
                if (columnCategory != column)
                {
                    throw new ArgumentException("The task is not in the specified column.");
                }
                Task task = GetTask(column, taskID);
                if (task.Assignee == null || task.Assignee == email)
                {
                    if (task.Assignee == email && emailAssignee == email)
                    {
                        throw new ArgumentException("Task already Assign to the specified email");
                    }
                    task.Assignee = emailAssignee;
                }
                else
                {
                    throw new ArgumentException("Only the current assignee can reassign the task.");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        public void UnAssignTasks(string email)
        {
            columns[ColumnCategory.Backlog].UnAssignTasks(email);
            columns[ColumnCategory.InProgress].UnAssignTasks(email);
        }

        public void RemoveMember(string email)
        {
            Members.Remove(email);
        }

        public List<Column> GetAllColumns()
        {
            return columns.Values.ToList();
        }

        // to test loadData 

        public override string ToString()
        {
            return $"Board [Id={Id}, Name={Name}, OwnerEmail={Owner}]\n+{string.Join(", ", columns.Values.Select(c => c.ToString()))}";



        }

    }
}