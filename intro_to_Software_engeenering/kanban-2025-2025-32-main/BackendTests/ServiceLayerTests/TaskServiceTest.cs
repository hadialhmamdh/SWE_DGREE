using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;
using Newtonsoft.Json.Linq;

namespace BackendTests.ServiceLayerTests
{
    internal class TaskServiceTest
    {
        private ServiceFactory serviceFactory;
        public TaskServiceTest()
        {
            serviceFactory = new ServiceFactory();
            (serviceFactory.UserService).Register("user@example.com", "Password1");
            (serviceFactory.BoardService).CreateBoard("user@example.com", "MyBoard");

        }
        public void CleanTests()
        {
            serviceFactory.DeleteData();
        }
    
        /// <summary>
        /// Requirement 18: This function tests the AddTask method for a successful task creation.
        /// </summary>
        public void TestAddTaskSuccess()
        {
            Console.WriteLine("Running TestAddTaskSuccess...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            string title = "New Task";
            string description = "This is a test task.";
            DateTime creationTime = DateTime.Now;
            DateTime dueTime = creationTime.AddDays(3);
            // Act
            string res = (serviceFactory.TaskService).AddTask(email, boardName, title, description, dueTime);
            // Assert using if-else
            Response response = JsonSerializer.Deserialize<Response>(res);
            bool success = response.ErrorMessage == null;

            if (success)
            {
                Console.WriteLine("Passed TestAddTaskSuccess\n");
            }
            else
            {
                Console.WriteLine("Failed TestAddTaskSuccess: " + response.ErrorMessage?.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 20: Tests that a user who is assigned to a task can edit its title successfully.
        /// </summary>
        public void TestEditTaskTitleSuccess()
        {
            Console.WriteLine("Running TestEditTaskTitleSuccess...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            string newTitle = "Updated Title";
            serviceFactory.TaskService.AssignTask(email, boardName, 0, taskID, email);
            // Act
            string res = (serviceFactory.TaskService).EditTaskTitle(email, boardName, taskID, newTitle, 0);

            // Assert using if-else
            Response json = JsonSerializer.Deserialize<Response>(res); ;
            bool success = json.ErrorMessage == null;

            if (success)
            {
                Console.WriteLine("Passed TestEditTaskTitleSuccess\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskTitleSuccess: " + json.ErrorMessage?.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 20: Tests that a user who is not assigned to a task cannot edit its title.
        /// </summary>
        public void TestEditTaskTitleByNonAssigneeFailed()
        {
            Console.WriteLine("Running TestEditTaskTitleByNonAssigneeFailed...");
            string owner = "user@example.com";
            string other = "other11@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            serviceFactory.UserService.Register(other, "Password1");
            serviceFactory.BoardService.JoinBoard(other, 0);
            string res = serviceFactory.TaskService.EditTaskTitle(other, boardName, taskID, "Illegal Edit", 0);
            Response json = JsonSerializer.Deserialize<Response>(res);
            if (json.ErrorMessage != null)
            {
                Console.WriteLine("Passed TestEditTaskTitleByNonAssigneeFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskTitleByNonAssigneeFailed: Unauthorized edit was allowed\n");
            }

        }


        /// <summary>
        /// Requirement 20: Tests that the assignee can successfully edit the task due time.
        /// </summary>
        public void TestEditTaskDueTimeSuccess()
        {
            Console.WriteLine("Running TestEditTaskDueTimeSuccess...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            DateTime newDueTime = DateTime.Now.AddDays(5);

            // Act
            string res = (serviceFactory.TaskService).EditTaskDueTime(email, boardName, taskID, newDueTime, 0);

            // Assert using if-else
            Response response = JsonSerializer.Deserialize<Response>(res);
            bool success = response.ErrorMessage == null;

            if (success)
            {
                Console.WriteLine("Passed TestEditTaskDueTimeSuccess\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDueTimeSuccess: " + response.ErrorMessage?.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 20: Tests that a user who is not assigned to the task cannot edit its due time.
        /// </summary>
        public void TestEditTaskDueTimeByNonAssigneeFailed()
        {
            Console.WriteLine("Running TestEditTaskDueTimeByNonAssigneeFailed...");
            string owner = "user@example.com";
            string other = "notAssign@example.com";
            string boardName = "MyBoard";
            int taskID = 0;

            serviceFactory.UserService.Register(other, "Password1");
            serviceFactory.BoardService.JoinBoard(other, 0);

            DateTime newDueTime = DateTime.Now.AddDays(5);
            string res = serviceFactory.TaskService.EditTaskDueTime(other, boardName, taskID, newDueTime, 0);
            Response json = JsonSerializer.Deserialize<Response>(res);

            if (json.ErrorMessage != null)
            {
                Console.WriteLine("Passed TestEditTaskDueTimeByNonAssigneeFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDueTimeByNonAssigneeFailed: Unauthorized edit was allowed\n");
            }

        }

        /// <summary>
        /// Requirement 20: Tests that the assignee can successfully edit the task description.
        /// </summary>
        public void TestEditTaskDescriptionSuccess()
        {
            Console.WriteLine("Running TestEditTaskDescriptionSuccess...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            string newDescription = "Updated description.";

            // Act
            string response = (serviceFactory.TaskService).EditTaskDescription(email, boardName, taskID, newDescription, 0);

            // Assert using if-else
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool success = json.ErrorMessage == null;

            if (success)
            {
                Console.WriteLine("Passed TestEditTaskDescriptionSuccess\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDescriptionSuccess: " + json.ErrorMessage?.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 20: Tests that a user who is not assigned to the task cannot edit its description.
        /// </summary>
        public void TestEditTaskDescriptionByNonAssigneeFailed()
        {
            Console.WriteLine("Running TestEditTaskDescriptionByNonAssigneeFailed...");
            string owner = "user@example.com";
            string other = "newNotAssign@example.com";
            string boardName = "MyBoard";
            int taskID = 0;

            serviceFactory.UserService.Register(other, "Password1");
            serviceFactory.BoardService.JoinBoard(other, 0);

            string res = serviceFactory.TaskService.EditTaskDescription(other, boardName, taskID, "Illegal Description", 0);
            Response json = JsonSerializer.Deserialize<Response>(res);

            if (json.ErrorMessage != null)
            {
                Console.WriteLine("Passed TestEditTaskDescriptionByNonAssigneeFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDescriptionByNonAssigneeFailed: Unauthorized edit was allowed\n");
            }
        }

        /// <summary>
        /// This function tests the AddTask method with invalid task title.(Requirement 18)
        /// </summary>
        public void TestAddTaskFailed()
        {
            Console.WriteLine("Running TestAddTaskFailed (empty title)...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            string title = ""; // invalid title
            string description = "This task has no title.";
            DateTime creationTime = DateTime.Now;
            DateTime dueTime = creationTime.AddDays(3);
            // Act
            string response = (serviceFactory.TaskService).AddTask(email, boardName, title, description, dueTime);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failed = json.ErrorMessage != null;

            if (failed)
            {
                Console.WriteLine("Passed TestAddTaskFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestAddTaskFailed: task should not have been added\n");
            }
        }

        /// <summary>
        /// This function tests EditTaskTitle with an empty new title, which should fail.(Requirement 20)
        /// </summary>
        public void TestEditTaskTitleFailed()
        {
            Console.WriteLine("Running TestEditTaskTitleFailed (empty title)...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            string newTitle = ""; // invalid title

            // Act
            string response = (serviceFactory.TaskService).EditTaskTitle(email, boardName, taskID, newTitle, 0);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failed = json.ErrorMessage != null;

            if (failed)
            {
                Console.WriteLine("Passed TestEditTaskTitleFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskTitleFailed: title should not have been accepted\n");
            }
        }

        /// <summary>
        /// This function tests EditTaskDueTime with a past date, which should fail.(Requirement 20)
        /// </summary>
        public void TestEditTaskDueTimeFailed()
        {
            Console.WriteLine("Running TestEditTaskDueTimeFailed (past date)...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            DateTime newDueTime = DateTime.Now.AddDays(-4); // invalid past date

            // Act
            string response = (serviceFactory.TaskService).EditTaskDueTime(email, boardName, taskID, newDueTime, 0);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failed = json.ErrorMessage != null;

            if (failed)
            {
                Console.WriteLine("Passed TestEditTaskDueTimeFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDueTimeFailed: due time should not be in the past\n");
            }
        }

        /// <summary>
        /// This function tests EditTaskDescription with a too-long description (over 300 chars), which should fail.(Requirement 15 & 16)
        /// </summary>
        public void TestEditTaskDescriptionFailed()
        {
            Console.WriteLine("Running TestEditTaskDescriptionFailed (too long)...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int taskID = 0;
            string newDescription = new string('a', 301); // 301 characters

            // Act
            string response = (serviceFactory.TaskService).EditTaskDescription(email, boardName, taskID, newDescription, 0);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failed = json.ErrorMessage != null;

            if (failed)
            {
                Console.WriteLine("Passed TestEditTaskDescriptionFailed (expected failure)\n");
            }
            else
            {
                Console.WriteLine("Failed TestEditTaskDescriptionFailed: description should not exceed 300 chars\n");
            }
        }

        /// <summary>
        /// This function tests AdvanceTask for a successful input.
        /// </summary>
        public void TestAdvanceTaskSuccessful()
        {
            Console.WriteLine("Running TestAdvanceTaskSuccessful...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int columnOrdinal = 0;
            int taskID = 0;

            // Act
            string response = serviceFactory.TaskService.AdvanceTask(email, boardName, columnOrdinal, taskID);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool success = json.ErrorMessage == null;

            if (success)
            {
                Console.WriteLine("Passed TestAdvanceTaskSuccessful\n");
            }
            else
            {
                Console.WriteLine("Failed TestAdvanceTaskSuccessful: " + json.ErrorMessage + "\n");
            }
        }

        /// <summary>
        /// This function tests AdvanceTask with a wrong column index (e.g., wrong current column of the task).
        /// </summary>
        public void TestAdvanceTaskFailed()
        {
            Console.WriteLine("Running TestAdvanceTaskFailed...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            int wrongColumnOrdinal = 0;
            int taskID = 0;

            // Act
            serviceFactory.TaskService.AssignTask(email, boardName, 0, taskID, email);
            string response = serviceFactory.TaskService.AdvanceTask(email, boardName, wrongColumnOrdinal, taskID);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failure = json.ErrorMessage != null;

            if (failure)
            {
                Console.WriteLine("Passed TestAdvanceTaskFailed (expected failure): " + "\n");
            }
            else
            {
                Console.WriteLine("Failed TestAdvanceTaskFailed: Task advanced from wrong column without error\n");
            }
        }

        /// <summary>
        /// This function tests GetAllInProgress  with a successful input.(Requirement 22)
        /// </summary>
        public void TestGetAllInProgressSuccessful()
        {
            Console.WriteLine("Running TestGetAllInProgressSuccessful...");

            // Arrange
            string email = "user@example.com";
            string boardName = "MyBoard";
            (serviceFactory.TaskService).AddTask(email, boardName, "secTask", "des", DateTime.Now.AddDays(3));
            serviceFactory.TaskService.AssignTask(email, boardName, 0, 1, email);
            (serviceFactory.TaskService).AdvanceTask(email, boardName, 0, 1);

            // Act
            string response = serviceFactory.TaskService.GetAllInProgress(email);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failure = json.ErrorMessage == null;

            if (failure)
            {
                Console.WriteLine("Passed TestGetAllInProgressSuccessful \n");
            }
            else
            {
                Console.WriteLine("Failed TestGetAllInProgressSuccessful: " + json.ErrorMessage?.ToString() + "\n");
            }
        }
        /// <summary>
        /// This function tests GetAllInProgress with a non-existent user (expected failure)(Requirement 22).
        /// </summary>
        public void TestGetAllInProgressFailed()
        {
            Console.WriteLine("Running TestGetAllInProgressFailed...");

            // Arrange
            string invalidEmail = "nonexistentuser@example.com";

            // Act
            string response = serviceFactory.TaskService.GetAllInProgress(invalidEmail);

            // Assert
            Response json = JsonSerializer.Deserialize<Response>(response);
            bool failure = json.ErrorMessage != null;

            if (failure)
            {
                Console.WriteLine("Passed TestGetAllInProgressFailed (expected failure): "+ "\n");
            }
            else
            {
                Console.WriteLine("Failed TestGetAllInProgressFailed: expected an error for non-existent user\n");
            }
        }
    
        /// <summary>
        /// Requirement 23: An unassigned task can be assigned by any board member to any board member(successful).
        /// </summary>
        public void Test_AssignTask_Successful()
        {
            Console.WriteLine("Running Test_AssignTask_Successful...");
            string ownerEmail = "BoardAndTaskOwner@gmail.com";
            string memberEmail = "NewAssign@gmail.com";
            string password = "Password1";
            string boardName = "TestAssignTask";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.UserService.Register(memberEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, boardName);
            serviceFactory.BoardService.JoinBoard(memberEmail, 1);
            serviceFactory.TaskService.AddTask(ownerEmail, boardName, "newTask", "dec", DateTime.Now.AddDays(3));
            string Res = serviceFactory.TaskService.AssignTask(ownerEmail, boardName, 0, 0, memberEmail);
            Response AssignResp = JsonSerializer.Deserialize<Response>(Res);
            if (AssignResp.ErrorMessage == null)
            {
                Console.WriteLine("Passed Test_AssignTask_Successful\n");
            }
            else
            {
                Console.WriteLine("Test_AssignTask_Successful failed: " + AssignResp.ErrorMessage + "\n");
            }
        }
        /// <summary>
        /// Requirement 23: Tests that assigning a task to a non-board-member fails, as only board members can be assigned tasks.
        /// </summary>
        public void Test_AssignTask_Failed()
        {

            Console.WriteLine("Running Test_AssignTask_Failed...");
            string ownerEmail = "creatorTask@gmail.com";
            string nonMemberEmail = "notMember123@example.com";
            string password = "Password1";
            string boardName = "failToAssign";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.UserService.Register(nonMemberEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, boardName);
            serviceFactory.TaskService.AddTask(ownerEmail, boardName, "newTask", "dec", DateTime.Now.AddDays(3));
            string Res = serviceFactory.TaskService.AssignTask(ownerEmail, boardName, 0, 0, nonMemberEmail);
            Response response = JsonSerializer.Deserialize<Response>(Res);
            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_AssignTask_Failed\n");
            }
            else
            {
                Console.WriteLine("FAILED: Test_AssignTask_Failed — Non-member should not be able to assign task.\n");
            }
        }

    }
}
