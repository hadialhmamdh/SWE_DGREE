using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    /// <summary>
    /// this class provides all task services (create task, edit task, etc..).
    /// </summary>
    public class TaskService
    {
        private readonly BoardFacade boardFacade;

        public TaskService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
        }

        /// <summary>
        /// This method creates a new task in the board.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board to create the task in. </param>
        /// <param name="title"> the title of the task. </param>
        /// <param name="description"> the task description (not mandatory from the user). </param>
        /// <param name="dueTime"> the task creation time. </param>
        /// <returns> A string for a success adding or an error message. </returns>
        public string AddTask(string email, string boardName, string title, string description, DateTime dueTime)
        {
            Response response = boardFacade.AddTask(email, boardName, title, description, dueTime);
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// This method edit task title.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board to edit the task title on. </param>
        /// <param name="taskID"> the ID of the task to edit. </param>
        /// <param name="newTitle"> the task new title. </param>
        /// <param name="columnName"> the column name to set the tasks title for </param>
        /// <returns> A string for a success title edit or an error message. </returns>
        public string EditTaskTitle(string email, string boardName, int taskID, string newTitle, int columnOrdinal)
        {
            Response response= boardFacade.EditTaskTitle(email,boardName,taskID,newTitle, columnOrdinal);
            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// This method edit task due time.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board to edit the task due time on. </param>
        /// <param name="taskID"> the ID of the task to edit. </param>
        /// <param name="newDueTime"> the task new due time. </param>
        /// <param name="columnName"> the column name to set the tasks dueTime for </param>
        /// <returns> A string for a success due time edit or an error message. </returns>
        public string EditTaskDueTime(string email, string boardName, int taskID, DateTime newDueTime, int columnOrdinal)
        {
            Response response = boardFacade.EditTaskDueTime(email,boardName,taskID,newDueTime, columnOrdinal);
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// This method edit task description.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board to edit the task description on. </param>
        /// <param name="taskID"> the ID of the task to edit. </param>
        /// <param name="newDescription"> the task new description. </param>
        /// <param name="columnName"> the column name to set the tasks Description for </param>
        /// <returns> A string for a success description edit or an error message. </returns>
        public string EditTaskDescription(string email, string boardName, int taskID, string newDescription, int columnOrdinal)
        {
            Response response=boardFacade.EditTaskDescription(email,boardName,taskID,newDescription,columnOrdinal);
            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// This method get all in progress tasks.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <returns> A string contains the all in progress tasks or an error message. </returns>
        public string GetAllInProgress(string email)
        {
            Response response = boardFacade.GetAllInProgress(email);
            return JsonSerializer.Serialize(response);
        }

        public string AdvanceTask(string email, string boardName, int columnOrdinal, int taskId)
        {
            Response response = boardFacade.AdvanceTask(email,boardName,columnOrdinal,taskId); 
            return JsonSerializer.Serialize(response);  
        }

        /// <summary>
        /// Assigns a task to a user. Any board member can assign an unassigned task to any other board member.
        /// Only the current assignee can reassign an already assigned task.
        /// </summary>
        /// <param name="email">The email of the user performing the assignment.</param>
        /// <param name="boardName">The name of the board containing the task.</param>
        /// <param name="columnOrdinal">The index of the column where the task currently resides.</param>
        /// <param name="taskID">The ID of the task to be assigned.</param>
        /// <param name="emailAssignee">The email of the user to assign the task to.</param>
        /// <returns>A JSON-formatted string indicating success or containing an error message.</returns>
        public string AssignTask(string email, string boardName, int columnOrdinal, int taskID, string emailAssignee)
        {
            Response response = boardFacade.AssignTask(email, boardName, columnOrdinal, taskID,emailAssignee);
            return JsonSerializer.Serialize(response);
        }
    }
}