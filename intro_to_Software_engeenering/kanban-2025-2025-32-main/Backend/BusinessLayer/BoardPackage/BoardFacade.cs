using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// this class manages user access to thier boards and tasks.
    /// </summary>
    public class BoardFacade
    {
        private readonly Dictionary<int, Board> boards;  
        private readonly UserFacade userFacade;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int BoardNotFound = -1;


        public int BoardCounter { get; private set; }

        public BoardFacade(UserFacade userFacade)
        {
            boards = new Dictionary<int, Board>();
            this.userFacade = userFacade;
            BoardCounter = 0;

            try
            {
                SystemConfigController configController = new SystemConfigController();
                string value = configController.GetValue("BoardCounter");
                BoardCounter = int.Parse(value); 
            }
            catch (Exception e)
            {
                log.Error("Failed to load BoardCounter from SystemConfig. Defaulting to 0.", e);
                BoardCounter = 0;
            }
        }


        /// <summary>
        /// created a new board for the user.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the name of the board to create. </param>
        /// <returns> Response indicating success or failure. </returns>
        public Response CreateBoard(string email, string boardName)
        {
            try
            {
                IsLegal(email, boardName);  
            }
            catch (Exception ex)
            { 
               log.Error("CreateBoard failed: "+ex.Message);
               return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
           if(id == BoardNotFound)
           {
              Board board = new Board(boardName, BoardCounter, email);
              IncrementBoardCounter();
              boards.Add(board.Id, board);
              log.Info($"Board '{boardName}' created successfully for user '{email}'.");
              return new Response();
            }
          log.Error($"CreateBoard failed: board '{boardName}' already exists for user '{email}'.");
          return new Response("Board already exists.");

}

        /// <summary>
        /// this method delete a board for the user.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the name of the board to delete. </param>
        /// <returns> Response indicating success or failure. </returns>
        public Response DeleteBoard(string email, string boardName)
        {
            try
            {
                IsLegal(email, boardName);
                int id = GetBoardIdByName(email, boardName);
                if (id == BoardNotFound)
                {
                    log.Error($"DeleteBoard failed: board '{boardName}' does not exist for user '{email}'.");
                    return new Response("Board does not exist.");
                }
                Board board = boards[id];
                if (!(board.IsOwner(email)))
                {
                    log.Error($"DeleteBoard failed: user '{email}' is not the owner.");
                    return new Response("user is not the owner.");
                }
                board.BoardDTO.Delete(); // Delete the board from the database
                boards.Remove(board.Id);
                log.Info($"Board '{boardName}' deleted successfully for user '{email}'.");
                return new Response();
            }
            catch (Exception ex)
            {
                log.Error("DeleteBoard failed: " + ex.Message);
                return new Response(ex.Message);
            }
            

        }


       

        /// <summary>
        /// this method moves a task from backlog to in progress column.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name that containing the task. </param>
        /// <param name="taskID"> the ID of the task to move. </param>
        /// <returns> Response indicating success or failure. </returns>
        public Response MoveTaskToInProgress(string email, string boardName, int taskID)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("MoveTaskToInProgress failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"MoveTaskToInProgress failed: board '{boardName}' does not exist for user '{email}'.");
                return new Response("Board does not exist.");
            }
            Board board = boards[id];
            try
            {
                board.MoveTaskToProgress(taskID,email);
                log.Info($"Task {taskID} moved to in-progress in board '{boardName}' for user '{email}'.");
                return new Response();
            }
            catch (Exception e)
            {
                log.Error($"MoveTaskToInProgress failed: {e.Message}");
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// this method moves a task from in progress to done column.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name that containing the task. </param>
        /// <param name="taskID"> the ID of the task to move. </param>
        /// <returns> Response indicating success or failure. </returns>
        public Response MoveTaskToDone(string email, string boardName, int taskID)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("MoveTaskToDone failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"MoveTaskToDone failed: board '{boardName}' does not exist for user '{email}'.");
                return new Response("Board does not exist.");
            }
            Board board = boards[id];
            try
            {
                board.MoveTaskToDone(taskID,email);
                log.Info($"Task {taskID} moved to Done in board '{boardName}' for user '{email}'.");
                return new Response();
            }
            catch (Exception e)
            {
                log.Error($"MoveTaskToDone failed: {e.Message}");
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Adds a new task to the specified board for the given user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="boardName">The name of the board to add the task to.</param>
        /// <param name="title">The title of the task (must not be empty, max 50 characters).</param>
        /// <param name="description">The optional description of the task (max 300 characters).</param>
        /// <param name="dueTime">The due date and time of the task.</param>
        /// <returns>A Response object indicating success or failure.</returns>
        public Response AddTask(string email, string boardName, string title, string description, DateTime dueTime)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch(Exception ex)
            {
                log.Error("AddTask failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if(id == BoardNotFound)
            {
                log.Error($"AddTask failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            Board board = boards[id];
            try
            {
                Task task = board.AddTask(email, title, description, dueTime);
                log.Info($"Task '{title}' added successfully to board '{boardName}' with Id -{task.Id} by user '{email}'.");
                return new Response();
            }
            catch (Exception e)
            {
                log.Error($"AddTask failed for user '{email}' on board '{boardName}': {e.Message}");
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Sets the maximum number of tasks allowed in a specified column of a board for a given user.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnOrdinal">The column to set the limit for (Backlog, InProgress, Done).</param>
        /// <param name="limit">The maximum number of tasks allowed (must be non-negative).</param>
        /// <returns>A Response object indicating success or failure.</returns>
        public Response SetColumnLimit(string email, string boardName, int columnOrdinal, int limit)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("SetColumnLimit failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id= GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"SetColumnLimit failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("SetColumnLimit failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory columnName = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            try
            {
                board.SetLimit(columnName, limit);
                log.Info($"SetColumnLimit succeeded: Limit for column '{columnName}' in board '{boardName}' set to {limit} by user '{email}'.");
                return new Response();
            }
            catch (Exception ex)
            {
                log.Error($"SetColumnLimit failed for user '{email}' on board '{boardName}', column '{columnName}': {ex.Message}");
                return new Response(ex.Message);
            }


        }

        /// <summary>
        /// Edits the due time of a given task on the user's board.
        /// </summary>
        /// <param name="email">The user's email. Must be logged in.</param>
        /// <param name="boardName">The name of the board containing the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="newDueTime">The new due time to set for the task.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response EditTaskDueTime(string email, string boardName, int taskID, DateTime newDueTime, int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("EditTaskDueTime failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"EditTaskDueTime failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("EditTaskDueTime failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory columnCategory = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            try
            {
                ColumnCategory column = board.ContainTask(taskID);
                //
                if (column == ColumnCategory.Done)
                {
                    log.Error($"EditTaskDueTime failed: Task {taskID} is in the 'Done' column, which is not editable.");
                    return new Response("Cannot edit tasks that are already Done.");
                }
                //
                if (columnCategory != column)
                {
                    log.Error($"EditTaskDueTime failed: task {taskID} is not in the specified column '{columnCategory}'. Actual column: '{column}'.");
                    return new Response("Task is not in the specified column.");
                }
                Task task = board.GetTask(column, taskID);
                if(task.Assignee != email)
                {

                    log.Error($"EditTaskDueTime failed: user '{email}' is not the assignee of task {taskID}.");
                    return new Response("Only the task assignee can edit the task.");
                }
                task.DueDate = newDueTime;
                log.Info($"EditTaskDueTime succeeded: Task {taskID} due time updated to '{newDueTime}' by user '{email}' on board '{boardName}'.");
                return new Response();
            }
            catch (Exception ex)
            {
                log.Error($"EditTaskDueTime failed for user '{email}' on board '{boardName}', task ID '{taskID}': {ex.Message}");
                return new Response(ex.Message);
            }
        }


        /// <summary>
        /// Edits the title of a task identified by its ID on a user's board.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="taskID">The ID of the task to edit.</param>
        /// <param name="newTitle">The new title to assign to the task.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response EditTaskTitle(string email, string boardName, int taskID, string newTitle,int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("EditTaskTitle failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"EditTaskTitle failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("EditTaskTitle failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory columnCategory = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            try
            {
                ColumnCategory column = board.ContainTask(taskID);
                if (column == ColumnCategory.Done)
                {
                    log.Error($"EditTaskTitle failed: Task {taskID} is in the 'Done' column, which is not editable.");
                    return new Response("Cannot edit tasks that are already Done.");
                }
                if (columnCategory != column)
                {
                    log.Error($"EditTaskTitle failed: task {taskID} is not in the specified column '{columnCategory}'. Actual column: '{column}'.");
                    return new Response("Task is not in the specified column.");
                }
                Task task = board.GetTask(column, taskID);
                if (task.Assignee != email)
                {

                    log.Error($"EditTaskDueTime failed: user '{email}' is not the assignee of task {taskID}.");
                    return new Response("Only the task assignee can edit the task.");
                }
                task.Title = newTitle;
                log.Info($"EditTaskTitle succeeded: Task {taskID} title changed to '{newTitle}' by user '{email}' on board '{boardName}'.");
                return new Response();

            }
            catch (Exception ex)
            {
                log.Error($"EditTaskTitle failed for user '{email}' on board '{boardName}', task ID '{taskID}': {ex.Message}");
                return new Response(ex.Message);
            }
        }

        /// <summary>
        /// Edits the description of a given task on the user's board.
        /// </summary>
        /// <param name="email">The user's email. Must be logged in.</param>
        /// <param name="boardName">The name of the board containing the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="newDescription">The new description to set for the task.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response EditTaskDescription(string email, string boardName, int taskID, string newDescription,int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("EditTaskDescription failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"EditTaskDescription failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("EditTaskDescription failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory columnCategory = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            try
            {
                ColumnCategory column = board.ContainTask(taskID);
                if (column == ColumnCategory.Done)
                {
                    log.Error($"EditTaskDescription failed: Task {taskID} is in the 'Done' column, which is not editable.");
                    return new Response("Cannot edit tasks that are already Done.");
                }
                if (columnCategory != column) {
                    log.Error($"EditTaskDescription failed: task {taskID} is not in the specified column '{columnCategory}'. Actual column: '{column}'.");
                    return new Response("Task is not in the specified column.");
                }
                Task task = board.GetTask(column, taskID);
                if (task.Assignee != email)
                {

                    log.Error($"EditTaskDueTime failed: user '{email}' is not the assignee of task {taskID}.");
                    return new Response("Only the task assignee can edit the task.");
                }
                task.Description = newDescription;

                log.Info($"EditTaskDescription succeeded: Task {taskID} description updated by user '{email}' on board '{boardName}'.");
                return new Response();
            }
            catch (Exception ex)
            {
                log.Error($"EditTaskDescription failed for user '{email}' on board '{boardName}', task ID '{taskID}': {ex.Message}");
                return new Response(ex.Message);
            }
        }

        // <summary>
        /// Gets all 'in progress' tasks for the user across all their boards.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <returns>A Response object indicating success or failure, containing a list of tasks if successful.</returns>
        public Response GetAllInProgress(string email)
        {
            List<Task> tasks = new List<Task>();
            if (email == null)
            {
                log.Error("GetAllInProgress failed: email is null.");
                return new Response("Email cannot be null.");
            }
            if (!userFacade.UserExists(email))
            {
                log.Error($"GetAllInProgress failed: User '{email}' does not exist.");
                return new Response("User doesn't exist.");
            }
            if (!userFacade.IsLoggedIn(email))
            {
                log.Error($"GetAllInProgress failed: User '{email}' is not logged in.");
                return new Response("User is not logged in.");

            }
            foreach (Board board in GetUserBoardsBl(email))
            { 
                tasks.AddRange(board.GetAllTasksOfAssign(email));
            }
            //
            log.Info($"GetAllInProgress succeeded for user '{email}'. Found {tasks.Count} tasks in progress.");
            return new Response(null,tasks);

        }

        /// <summary>
        /// Advances a task to the next column on the specified board for the given user.
        /// </summary>
        /// <param name="email">The user's email. Must be logged in.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnOrdinal">The current column of the task (ignored in logic, kept for signature compatibility).</param>
        /// <param name="taskId">The ID of the task to be advanced.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response AdvanceTask(string email, string boardName, int columnOrdinal, int taskId)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("AdvanceTask failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"AdvanceTask failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("AdvanceTask failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory column = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            try
            {
                ColumnCategory currentColumn = board.ContainTask(taskId);
                if (currentColumn != column)
                {
                    log.Error($"AdvanceTask failed: task {taskId} is not in the specified column '{column}'. Actual column: '{currentColumn}'.");
                    return new Response("Task is not in the specified column.");
                }
                switch (currentColumn)
                {
                    case ColumnCategory.Backlog:
                        return MoveTaskToInProgress(email, boardName, taskId);
                    case ColumnCategory.InProgress:
                        return MoveTaskToDone(email, boardName, taskId);
                    case ColumnCategory.Done:
                        log.Error($"AdvanceTask failed: Task {taskId} is already in the Done column.");
                        return new Response("Task is already in the Done column. Cannot advance further.");
                    default:
                        log.Error($"AdvanceTask failed: Unknown column for task {taskId}.");
                        return new Response("Invalid column state for task.");
                }
            }
            catch (Exception ex)
            {
                log.Error($"AdvanceTask failed for user '{email}' on board '{boardName}', task ID {taskId}: {ex.Message}");
                return new Response(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the task limit for a specified column on the user's board.
        /// </summary>
        /// <param name="email">The user's email. Must be logged in.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnOrdinal">The column for which to retrieve the limit.</param>
        /// <returns>A Response containing the limit if successful, or an error message if failed.</returns>
        public Response GetLimit(string email, string boardName, int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("GetLimit failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"GetLimit failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("GetLimit failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory column = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            int limit = board.GetLimit(column);
            log.Info($"GetLimit succeeded: User '{email}', board '{boardName}', column '{column}' has limit {limit}.");
            return new Response(null, limit);
            
        }

        public Response GetColumn(string email, string boardName, int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("GetColumn failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"GetColumn failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("GetColumn failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory column = ColumnToCategory(columnOrdinal);
            Board board = boards[id];
            List<Task> tasks = board.GetAllTasks(column);
            log.Info($"GetColumn succeeded: User '{email}', board '{boardName}', column '{column}'.");
            return new Response(tasks);
        }

        public Response GetColumnName(string email, string boardName, int columnOrdinal)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("GetColumnName failed: " + ex.Message);
                return new Response(ex.Message);
            }
            int id = GetBoardIdByName(email, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"GetColumnName failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("GetColumnName failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            ColumnCategory column = ColumnToCategory(columnOrdinal);
            if (column == ColumnCategory.Backlog)
            {
                return new Response(null, "Backlog");
            }
            else if (column == ColumnCategory.InProgress)
            {
                return new Response(null, "In Progress");
            }
            else
            {
                return new Response(null,"Done");
            }

        }

        private void IsLegal(string email, string boardName) 
        { 
            if (string.IsNullOrWhiteSpace(email))
            { 
                throw new ArgumentException("email is empty or null.");
                
            }
            if (!userFacade.UserExists(email))
            { 
                throw new ArgumentException($"user '{email}' does not exist.");
            }
            if (!userFacade.IsLoggedIn(email))
            {
                throw new ArgumentException($"user {email} is not logged in.");
            }
            if (string.IsNullOrWhiteSpace(boardName))
            {
                throw new ArgumentException($"board name is empty or null for user '{email}'.");
            }
        }

        private int GetBoardIdByName(string email, string boardName)
        {
            foreach (KeyValuePair<int, Board> entry in boards)
            {
                if (string.Equals(boardName, entry.Value.Name, StringComparison.OrdinalIgnoreCase) &&(entry.Value.IsMember(email) || entry.Value.IsOwner(email)))

                {
                    return entry.Key;
                }
            }
            return BoardNotFound;
        }

        private void IncrementBoardCounter()
        {
            BoardCounter++;

            try
            {
                bool success = new SystemConfigController().Update("BoardCounter", "Value", BoardCounter.ToString());

                if (!success)
                {
                    log.Warn($"Failed to update BoardCounter in DB. Current value: {BoardCounter}");
                }
                else
                {
                    log.Info($"BoardCounter incremented and updated in DB. New value: {BoardCounter}");
                }
            }
            catch (Exception e)
            {
                log.Error("Exception occurred while updating BoardCounter in DB.", e);
            }
        }


        private bool IllegalColumn(int column)
        {
            if ((column > 2) || (column < 0))
            {
                return false;
            }
            return true;
        }
        private ColumnCategory ColumnToCategory(int columnOrdinal)
        {
            if (columnOrdinal == 0)
                return ColumnCategory.Backlog;
            else if (columnOrdinal == 1)
                return ColumnCategory.InProgress;
            return ColumnCategory.Done;

        }

        /// <summary>
        /// Allows a user to join a board.
        /// </summary>
        /// <param name="email">The email of the user attempting to join.</param>
        /// <param name="boardID">The ID of the board to join.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response JoinBoard(string email, int boardID)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                log.Error("JoinBoard failed: email is null or empty.");
                return new Response("Email cannot be empty.");
            }
            if (!userFacade.UserExists(email))
            {
                log.Error($"JoinBoard failed: user '{email}' does not exist.");
                return new Response("User does not exist.");
            }
            if (!userFacade.IsLoggedIn(email))
            {
                log.Error($"JoinBoard failed: user '{email}' is not logged in.");
                return new Response("User is not logged in.");
            }
            if (!boards.ContainsKey(boardID))
            {
                log.Error($"JoinBoard failed: board with ID '{boardID}' does not exist.");
                return new Response("Board with ID does not exist.");
            }
            Board board = boards[boardID];
            //
            if (board.IsMember(email) || (board.IsOwner(email)))
            {
                log.Error($"JoinBoard failed: user '{email}' is already a member of board '{board.Name}'.");
                return new Response("User is already a member of this board.");
            }
            BoardMemberDTO member = new BoardMemberDTO(boardID, email);
            member.Save();  
            board.AddMember(email);
            log.Info($"User '{email}' joined board '{board.Name}' (ID: {boardID}).");
            return new Response();
        }
        public Response TransferOwnerShip(string currentOwnerEmail, string newOwnerEmail, string boardName)
        {
            try
            {
                IsLegal(currentOwnerEmail, boardName);
            }
            catch (Exception ex)
            {
                log.Error("TransferOwnerShip failed: " + ex.Message);
                return new Response(ex.Message);
            }
            //
            if (string.IsNullOrWhiteSpace(newOwnerEmail))
            {
                log.Error("TransferOwnerShip failed: newOwnerEmail is empty or null.");
                return new Response("newOwnerEmail is empty or null.");
            }
            if (!userFacade.UserExists(newOwnerEmail))
            {
                log.Error($"TransferOwnerShip failed: user '{newOwnerEmail}' does not exist.");
                return new Response("user does not exists");
            }
            //
            int id = GetBoardIdByName(currentOwnerEmail, boardName);
            if (id == BoardNotFound)
            {
                log.Error($"TransferOwnerShip failed: board '{boardName}' not found for user '{currentOwnerEmail}'.");
                return new Response("Board does not exist.");
            }
            Board board = boards[id];
            if (!(board.IsOwner(currentOwnerEmail)))
            {
                log.Error($"TransferOwnerShip failed: User '{currentOwnerEmail}' is not the owner of board '{boardName}'.");
                return new Response("user is not the owner.");
            }
            if (!(board.IsMember(newOwnerEmail)))
            {
                log.Error($"TransferOwnerShip failed: User '{newOwnerEmail}' is not a member of board '{boardName}'.");
                return new Response("user is not member");
            }
            board.BoardDTO.Owner = newOwnerEmail;
            board.Owner = newOwnerEmail;
            //
            board.AddMember(currentOwnerEmail);
            log.Info($"Ownership of board '{boardName}' transferred from '{currentOwnerEmail}' to '{newOwnerEmail}'.");
            return new Response();
        }
        public Response AssignTask(string email, string boardName, int columnOrdinal, int taskID, string emailAssignee)
        {
            try
            {
                IsLegal(email, boardName);
            }
            catch (Exception ex)
            {
                log.Error("AssignTask failed: " + ex.Message);
                return new Response(ex.Message);
            }
            //
            if (string.IsNullOrWhiteSpace(emailAssignee))
            {
                log.Error("AssignTask failed: emailAssign is empty or null.");
                return new Response("emailAssign is empty or null.");
            }
            if (!userFacade.UserExists(emailAssignee))
            {
                log.Error($"AssignTask failed: user '{emailAssignee}' does not exist.");
                return new Response("user does not exists");
            }
            //
            if (!(IllegalColumn(columnOrdinal)))
            {
                log.Error("AssignTask failed:Illegal columnOrdinal");
                return new Response("Illegal columnOrdinal");
            }
            int id = GetBoardIdByName(email, boardName);
            if(id == BoardNotFound)
            {
                log.Error($"AssignTask failed: board '{boardName}' not found for user '{email}'.");
                return new Response("Board does not exist.");
            }
            Board board = boards[id];
            if (!(board.IsMember(emailAssignee)) &&!(board.IsOwner(emailAssignee)))
            {
                log.Error($"AssignTask failed: user '{emailAssignee}' is not a member of board '{boardName}'.");
                return new Response("The assignee must be a member of the board.");
            }
            ColumnCategory column = ColumnToCategory(columnOrdinal);
            try
            {
                board.AssignTask(email, column, taskID, emailAssignee);
                log.Info($"AssignTask succeeded: task {taskID} in column '{column}' assigned to '{emailAssignee}' by '{email}'.");
                return new Response();
            }
            catch (Exception ex)
            {
                log.Error($"AssignTask failed during assignment: {ex.Message}");
                return new Response(ex.Message);
            }
        }
        
        private List<Board> GetUserBoardsBl (string email)
        {
            List<Board> output = new List<Board>();
            foreach (KeyValuePair<int, Board> entry in boards)
            {
                if ((entry.Value.IsMember(email) || entry.Value.IsOwner(email)))

                {
                    output.Add(entry.Value);
                }
            }
            return output;

        }

        public Response LeaveBoard(string email, int boardID)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                log.Error("LeaveBoard failed: email is null or empty.");
                return new Response("Email cannot be empty.");
            }

            if (!userFacade.UserExists(email))
            {
                log.Error($"LeaveBoard failed: user '{email}' does not exist.");
                return new Response("User does not exist.");
            }

            if (!userFacade.IsLoggedIn(email))
            {
                log.Error($"LeaveBoard failed: user '{email}' is not logged in.");
                return new Response("User is not logged in.");
            }

            if (!boards.ContainsKey(boardID))
            {
                log.Error($"LeaveBoard failed: board with ID '{boardID}' does not exist.");
                return new Response("Board with ID does not exist.");
            }

            Board board = boards[boardID];

            if (board.IsOwner(email))
            {
                log.Error($"LeaveBoard failed: user '{email}' is the owner of board '{board.Name}'.");
                return new Response("Owner cannot leave its board.");
            }

            if (!board.IsMember(email))
            {
                log.Error($"LeaveBoard failed: user '{email}' is not a member on board '{board.Name}'.");
                return new Response("User is not a member.");
            }
            BoardMemberController controller = new BoardMemberController();
            BoardMemberDTO memberDTO = controller.Select(boardID, email);
            if (memberDTO != null)
            {
                controller.DeleteMember(boardID, email);
                log.Info($"Board member entry deleted from DB: BoardID={boardID}, User={email}");
            }
            board.UnAssignTasks(email);
            board.RemoveMember(email);
            log.Info($"User '{email}' left board '{board.Name}' (ID: {boardID}).");
            return new Response();
        }


        public Response GetUserBoards(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                log.Error("GetUserBoards failed: email is null or empty.");
                return new Response("Email cannot be null or empty.");
            }
            if (!userFacade.UserExists(email))
            {
                log.Error($"GetUserBoards failed: User '{email}' does not exist.");
                return new Response("User doesn't exist.");
            }
            if (!userFacade.IsLoggedIn(email))
            {
                log.Error($"GetUserBoards failed: User '{email}' is not logged in.");
                return new Response("User is not logged in.");
            }
            List<int> boardsId = new List<int>();
            foreach (Board board in GetUserBoardsBl(email))
            {
                boardsId.Add(board.Id);
            }

            log.Info($"GetUserBoards succeeded for user '{email}', boards count: {boardsId.Count}.");
            return new Response(null, boardsId);
        }

        public Response GetBoardName(int boardId)
        {
            if (!boards.ContainsKey(boardId))
            {
                log.Error($"GetBoardName failed: board with ID '{boardId}' does not exist.");
                return new Response("Board with ID does not exist.");
            }
            log.Info($"GetBoardName succeeded: board ID '{boardId}', name '{GetBoard(boardId).Name}'.");
            return new Response(null, GetBoard(boardId).Name);
        }
        private Board GetBoard(int boardId) 
        {
            return boards[boardId];
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------
        public void LoadAllBoards()
        {
            List<BoardDTO> allBoards = new BoardController().SelectAll();
            foreach (BoardDTO dto in allBoards)
            {
                Board loaded = new Board(dto);
                boards.Add(dto.Id, loaded);
            }
            BoardCounter=new BoardController().GetMaxBoardId()+1;
            log.Info("All boards loaded successfully.");
        }
        public void SaveAllBoards()
        {
            foreach (Board board in boards.Values)
            {
                board.BoardDTO.Save();
            }

            SystemConfigController configController = new SystemConfigController();
            configController.Update("BoardCounter", SystemConfigDTO.ValueColumn, BoardCounter.ToString());

            log.Info("All boards and BoardCounter saved successfully.");
        }


        public void ResetBoardData()
        {
            var boardMemberController = new BoardMemberController();
            var boardController = new BoardController();
            var TaskController = new TaskController();
            var ColumnController = new ColumnController();

            boardMemberController.DeleteAllBoardMembers();
            TaskController.DeleteAllTasks();
            ColumnController.DeleteAllColumns();    
            boardController.DeleteAllBoards();
            new SystemConfigController().DeleteAll();

            log.Info("All boards and board members deleted.");
        }

        public void prints()
        {
            /*foreach(Board b in boards.Values)
            {
                Console.WriteLine(b.ToString());
            }*/
            Console.WriteLine(BoardCounter);
        }


        /// <summary>
        /// Retrieves all boards associated with the specified user email.
        /// </summary>
        /// <param name="email">The email of the user whose boards are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Response"/> object containing:
        /// <list type="bullet">
        ///   <item>If the email is null, empty, or invalid, an error message response.</item>
        ///   <item>If the user does not exist or is not logged in, an error message response.</item>
        ///   <item>Otherwise, a list of <see cref="BoardSL"/> objects representing the user's boards.</item>
        /// </list>
        /// </returns>
        public Response GetAllBoards(string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                log.Error("GetUserBoards failed: email is null or empty.");
                return new Response("Email cannot be null or empty.");
            }
            if (!userFacade.UserExists(email))
            {
                log.Error($"GetUserBoards failed: User '{email}' does not exist.");
                return new Response("User doesn't exist.");
            }
            if (!userFacade.IsLoggedIn(email))
            {
                log.Error($"GetUserBoards failed: User '{email}' is not logged in.");
                return new Response("User is not logged in.");
            }
            List<Board> boards = GetUserBoardsBl(email);
            return new Response(null,boards.Select(board => new BoardSL(board)).ToList());

        }
    }
}