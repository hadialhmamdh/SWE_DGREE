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
    /// this class provides all board services (create board, delete board, set column limit, etc..).
    /// </summary>
    public class BoardService
    {
        internal BoardFacade boardFacade;

        /// <summary>
        /// this method construct a new BoardFacade.
        /// </summary>
        public BoardService(BoardFacade boardFacade)
        {
          this.boardFacade = boardFacade;  
        }

        /// <summary>
        /// This method creates a new board for the user.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name to create. </param>
        /// <returns> A string for a success creation or an error message. </returns>
        public string CreateBoard(string email, string boardName)
        {
            Response response = boardFacade.CreateBoard(email, boardName);
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// This method deletes an existing board.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name to delete. </param>
        /// <returns> A string for a success deletion or an error message. </returns>
        public string DeleteBoard(string email, string boardName)
        {
            Response response = boardFacade.DeleteBoard(email, boardName);
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// This method sets tasks limit for specefic column.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name to set the tasks in column limit on. </param>
        /// <param name="columnName"> the column name to set the tasks limit for </param>
        /// <param name="limit"> the tasks limit for the column </param>
        /// <returns> A string for a success setting of the column limit or an error message. </returns>
        public string SetColumnLimit(string email, string boardName, int columnOrdinal, int limit)
        {
            Response response = boardFacade.SetColumnLimit(email, boardName, columnOrdinal, limit);
            return JsonSerializer.Serialize(response); 
        }

        public string GetColumn(string email, string boardName, int columnOrdinal)
        { 
            Response response = boardFacade.GetColumn(email, boardName, columnOrdinal);
            return JsonSerializer.Serialize(response);

        } 

        /// <summary>
        /// This method moves task from back log column to in progress column.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name that contains the moved task. </param>
        /// <param name="taskID"> the task ID to move. </param>
        /// <returns> A string for a success moving of the task or an error message. </returns>
        public string MoveTasktoInProgress(string email, string boardName, int taskID)
        {
            Response response = boardFacade.MoveTaskToInProgress(email, boardName, taskID);
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// This method moves task from in progress column to done column.
        /// </summary>
        /// <param name="email"> the user's email. </param>
        /// <param name="boardName"> the board name that contains the moved task. </param>
        /// <param name="taskID"> the task ID to move. </param>
        /// <returns> A string for a success moving of the task or an error message. </returns>
        public string MoveTasktoDone(string email, string boardName, int taskID)
        {
            Response response = boardFacade.MoveTaskToDone(email, boardName, taskID);
            return JsonSerializer.Serialize(response);  
        }


        /// <summary>
        /// Returns all the in progress tasks to spicific user.
        /// </summary>
        public string GetAllInProgress(string email)
        {
            Response response = boardFacade.GetAllInProgress(email);
            return JsonSerializer.Serialize(response);  
        }

        /// <summary>
        /// Returns the task limit of a given column in the specified board.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="Boardname">The name of the Kanban board.</param>
        /// <param name="CloumnName">The name of the column in the board.</param>
        /// <returns>A string with the limit information or an error message.</returns>
        public string GetLimit(string email, string boardName, int columnOrdinal)
        {
            Response response = boardFacade.GetLimit(email, boardName, columnOrdinal);
            return JsonSerializer.Serialize(response);
        }

        public string GetColumnName(string email, string boardName, int columnOrdinal)
        {
            Response response = boardFacade.GetColumnName(email, boardName, columnOrdinal);
            return JsonSerializer.Serialize(response);
            

        }

        /// <summary>
        /// Adds a user as a member to an existing board.
        /// </summary>
        /// <param name="email">The email of the user who is joining the board.</param>
        /// <param name="boardID">The ID of the board to join.</param>
        /// <returns>A JSON string indicating success or containing an error message.</returns>
        public string JoinBoard(string email, int boardID)
        {
            Response response = boardFacade.JoinBoard(email,boardID);
            return JsonSerializer.Serialize(response);
        }



        /// <summary>
        /// Removes a user from the members list of a board.
        /// </summary>
        /// <param name="email">The email of the user who is leaving the board.</param>
        /// <param name="boardID">The ID of the board to leave.</param>
        /// <returns>A JSON string indicating success or containing an error message.</returns>
        public string LeaveBoard(string email, int boardID)
        {
            Response response = boardFacade.LeaveBoard(email, boardID);
            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// Transfers the ownership of a board from one user to another.
        /// </summary>
        /// <param name="currentOwnerEmail">The email of the current board owner.</param>
        /// <param name="newOwnerEmail">The email of the new owner (must be a board member).</param>
        /// <param name="boardName">The name of the board whose ownership is being transferred.</param>
        /// <returns>A JSON string indicating success or containing an error message.</returns>
        public string TransferOwnership(string currentOwnerEmail, string newOwnerEmail, string boardName)
        {
            Response response = boardFacade.TransferOwnerShip(currentOwnerEmail,newOwnerEmail,boardName);
            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// This method returns a list of IDs of all user's boards.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <returns>A response with a list of IDs of all user's boards, unless an error occurs </returns>
        public string GetUserBoards(string email)
        {
            Response response = boardFacade.GetUserBoards(email);
            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// This method returns a board's name
        /// </summary>
        /// <param name="boardId">The board's ID</param>
        /// <returns>A response with the board's name, unless an error occurs </returns>
        public string GetBoardName(int boardId)
        {
            Response response = boardFacade.GetBoardName(boardId);
            return JsonSerializer.Serialize(response);
        }

        public string GetAllBoards(string email) 
        {
            Response response = boardFacade.GetAllBoards(email);
            return JsonSerializer.Serialize(response);
        }

    }
}
