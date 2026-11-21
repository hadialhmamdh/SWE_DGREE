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
    internal class BoardServiceTest
    {
        private readonly ServiceFactory serviceFactory;
        public BoardServiceTest()
        {
            serviceFactory = new ServiceFactory();

        }
        public void CleanTests()
        {
            serviceFactory.DeleteData();
        }
        /// <summary>
        /// Requirement 8: Test successful board creation.
        /// </summary>
        public void Test_CreateBoard_Successful()
        {
            Console.WriteLine("Running Test_CreateBoard_Successful...");

            string email = "user@gmail.com";
            string password = "Password1";
            string boardName = "testBoard";
            serviceFactory.UserService.Register(email, password);
            string createResponse = serviceFactory.BoardService.CreateBoard(email, boardName);
            Response createJson = JsonSerializer.Deserialize<Response>(createResponse);
            if (createJson.ErrorMessage != null)
            {
                Console.WriteLine("Test_CreateBoard_Successful failed: " + createJson.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_CreateBoard_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 8: Fail to create board for unregistered user.
        /// </summary>
        public void Test_CreateBoard_Failed_InvalidUser()
        {
            Console.WriteLine("Running Test_CreateBoard_Failed_InvalidUser...");
            string email = "nonuser@gmail.com";
            string boardName = "boardFail";
            string response = serviceFactory.BoardService.CreateBoard(email, boardName);
            Response json = JsonSerializer.Deserialize<Response>(response);
            if (json.ErrorMessage == null)
            {
                Console.WriteLine("Test_CreateBoard_Failed_InvalidUser failed: Unregistered user should not create board." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_CreateBoard_Failed_InvalidUser" + "\n");
            }
        }
        /// <summary>
        /// Requirement 8: Test successful board Deleation.
        /// </summary>
        public void Test_DeleteBoard_Successful()
        {
            Console.WriteLine("Running Test_DeleteBoard_Successful...");

            string email = "deleteboard@example.com";
            string password = "Password1";
            string boardName = "BoardToDelete";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string result = serviceFactory.BoardService.DeleteBoard(email, boardName);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Test_DeleteBoard_Successful failed: " + response.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_DeleteBoard_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 8 :Fail to delete board for unregistered user.
        /// </summary>
        public void Test_DeleteBoard_Failed_InvalidBoard()
        {
            Console.WriteLine("Running Test_DeleteBoard_Failed_InvalidBoard...");

            string email = "faildelete@example.com";
            string password = "Password1";
            string boardName = "NonExistentBoard";
            serviceFactory.UserService.Register(email, password);
            string result = serviceFactory.BoardService.DeleteBoard(email, boardName);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorMessage == null)
            {
                Console.WriteLine("Test_DeleteBoard_Failed_InvalidBoard failed: Expected an error for deleting nonexistent board." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_DeleteBoard_Failed_InvalidBoard" + "\n");
            }
        }



        /// <summary>
        /// Requirement 10: A user cannot create two boards with the same name.
        /// </summary>
        public void Test_DuplicateBoardName_SameUser()
        {
            Console.WriteLine("Running Test_DuplicateBoardName_SameUser...");

            string email = "duser@example.com";
            string password = "Password1";
            string boardName = "toTestBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string secondAttempt = serviceFactory.BoardService.CreateBoard(email, boardName);
            Response json = JsonSerializer.Deserialize<Response>(secondAttempt);
            if (json.ErrorMessage == null)
            {
                Console.WriteLine("Test_DuplicateBoardName_SameUser failed: Duplicate name should cause error." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_DuplicateBoardName_SameUser" + "\n");
            }
        }


        /// <summary>
        /// Requirement 10: Two users can have boards with the same name.
        /// </summary>
        public void Test_SameBoardName_DifferentUsers()
        {
            Console.WriteLine("Running Test_SameBoardName_DifferentUsers...");
            string boardName = "TeamBoard";
            serviceFactory.UserService.Register("user1@outlook.com", "Password1");
            serviceFactory.UserService.Register("user2@outlook.com", "Password2");
            string res1 = serviceFactory.BoardService.CreateBoard("user1@outlook.com", boardName);
            string res2 = serviceFactory.BoardService.CreateBoard("user2@outlook.com", boardName);
            Response json1 = JsonSerializer.Deserialize<Response>(res1);
            Response json2 = JsonSerializer.Deserialize<Response>(res2);
            if (json1.ErrorMessage != null || json2.ErrorMessage != null)
            {
                Console.WriteLine("Test_SameBoardName_DifferentUsers failed:" + "\n");
                Console.WriteLine(json1.ErrorMessage);
                Console.WriteLine(json2.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Passed Test_SameBoardName_DifferentUsers" + "\n");
            }
        }

        /// <summary>
        /// Requirement 16: Set column task limit.
        /// </summary>
        public void Test_SetColumnLimit_Successful()
        {
            Console.WriteLine("Running Test_SetColumnLimit_Successful...");

            string email = "taskUser@gmail.com";
            string password = "Password1";
            string boardName = "board1";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string result = serviceFactory.BoardService.SetColumnLimit(email, boardName, 1, 4);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Test_SetColumnLimit_Successful failed: " + response.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_SetColumnLimit_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 16: Fail to set limit on invalid column index.
        /// </summary>
        public void Test_SetColumnLimit_Failed_InvalidColumn()
        {
            Console.WriteLine("Running Test_SetColumnLimit_Failed_InvalidColumn...");

            string email = "invalidLimit@gmail.com";
            string password = "Password1";
            string boardName = "boardFail";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string result = serviceFactory.BoardService.SetColumnLimit(email, boardName, 3, 4);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorMessage == null)
            {
                Console.WriteLine("Test_SetColumnLimit_Failed_InvalidColumn failed: Invalid column should return error." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_SetColumnLimit_Failed_InvalidColumn" + "\n");
            }
        }

        /// <summary>
        /// Requirement 16: Get column task limit.
        /// </summary>
        public void Test_GetColumnLimit_Successful()
        {
            Console.WriteLine("Running Test_GetColumnLimit_Successful...");

            string email = "limituser@gmail.com";
            string password = "Password1";
            string boardName = "LimitedBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.BoardService.SetColumnLimit(email, boardName, 1, 1);
            string result = serviceFactory.BoardService.GetLimit(email, boardName, 1);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Test_GetColumnLimit_Successful failed: " + response.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_GetColumnLimit_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 16: Fail to get column limit with invalid column index.
        /// </summary>
        public void Test_GetColumnLimit_Failed_InvalidColumn()
        {
            Console.WriteLine("Running Test_GetColumnLimit_Failed_InvalidColumn...");
            string email = "limitfail@gmail.com";
            string password = "Password1";
            string boardName = "InvalidLimitBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string result = serviceFactory.BoardService.GetLimit(email, boardName, 3);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorMessage == null)
            {
                Console.WriteLine("Test_GetColumnLimit_Failed_InvalidColumn failed: Should return error for invalid column." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_GetColumnLimit_Failed_InvalidColumn" + "\n");
            }
        }


        /// <summary>
        /// Requirement 19: Test valid move from Backlog to In Progress.
        /// </summary>
        public void Test_Move_Backlog_To_InProgress_Successful()
        {
            Console.WriteLine("Running Test_Move_Backlog_To_InProgress_Successful...");
            string email = "MoveTest@gmail.com";
            string password = "Password1";
            string boardName = "MoveTestBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.TaskService.AddTask(email, boardName, "MoveTest", "Des", DateTime.Now.AddDays(2));
            serviceFactory.TaskService.AssignTask(email, boardName, 0, 0, email);
            string res = serviceFactory.BoardService.MoveTasktoInProgress(email, boardName, 0);
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage != null)
            {
                Console.WriteLine("Test_Move_Backlog_To_InProgress_Successful failed: " + o.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_Move_Backlog_To_InProgress_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 19: Test invalid move from Backlog to In Progress.
        /// </summary>
        public void Test_Move_Backlog_To_InProgress_Failed()
        {
            Console.WriteLine("Running Test_Move_Backlog_To_InProgress_Failed...");
            string email = "MoveTestFailed@gmail.com";
            string password = "Password1";
            string boardName = "MoveTestFailedBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.TaskService.AddTask(email, boardName, "MoveTestFailed", "Des", DateTime.Now.AddDays(2));
            serviceFactory.TaskService.AssignTask(email, boardName, 0, 0, email);
            string res = serviceFactory.BoardService.MoveTasktoInProgress(email, boardName, 1);//incorrect task id
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage == null)
            {
                Console.WriteLine("Test_Move_Backlog_To_InProgress_Failed failed: invalid TaskId should return an error." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_Move_Backlog_To_InProgress_Failed" + "\n");
            }
        }

        /// <summary>
        /// Requirement 19: Test valid move from In Progress to Done.
        /// </summary>
        public void Test_Move_InProgress_To_Done_Successful()
        {
            Console.WriteLine("Running Test_Move_InProgress_To_Done_Successful...");
            string email = "MoveTestDone@gmail.com";
            string password = "Password1";
            string boardName = "MoveTestDoneBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.TaskService.AddTask(email, boardName, "MoveTest", "Des", DateTime.Now.AddDays(2));
            serviceFactory.TaskService.AssignTask(email, boardName, 0, 0, email);
            serviceFactory.BoardService.MoveTasktoInProgress(email, boardName, 0);
            string res = serviceFactory.BoardService.MoveTasktoDone(email, boardName, 0);
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage != null)
            {
                Console.WriteLine("Test_Move_InProgress_To_Done_Successful failed: " + o.ErrorMessage.ToString() + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_Move_InProgress_To_Done_Successful" + "\n");
            }
        }

        /// <summary>
        /// Requirement 14: Test invalid move from In Progress to Done .
        /// </summary>
        public void Test_Move_InProgress_To_Done_Failed()
        {
            Console.WriteLine("Running Test_Move_InProgress_To_Done_Failed...");
            string email = "MoveTestDoneFailed@gmail.com";
            string password = "Password1";
            string boardName = "MoveTestDoneFailedBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.TaskService.AddTask(email, boardName, "MoveTestFailed", "Des", DateTime.Now.AddDays(2));
            serviceFactory.TaskService.AssignTask(email, boardName, 0, 0, email);
            string res = serviceFactory.BoardService.MoveTasktoDone(email, boardName, 0);
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage == null)
            {
                Console.WriteLine("Test_Move_InProgress_To_Done_Failed failed: should not allow move from Backlog to Done." + "\n");
            }
            else
            {
                Console.WriteLine("Passed Test_Move_InProgress_To_Done_Failed" + "\n");
            }
        }

        /// <summary>
        /// Requirement 12: join existing board created by someone else.
        /// </summary>
        public void Test_Join_Board_Successful()
        {
            Console.WriteLine("Running Test_Join_Board_Successful...");
            string email = "ExistBoard@gmail.com";
            string password = "Password1";
            string boardName = "ExistBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.UserService.Register("JoinBoard@gmail.com", "Password1");
            string res = serviceFactory.BoardService.JoinBoard("JoinBoard@gmail.com", 13);
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage == null)
            {
                Console.WriteLine("Passed Test_Join_Board_Successful" + "\n");
            }
            else
            {
                Console.WriteLine("Test_Join_Board_Successful failed: " + o.ErrorMessage.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 12: attempt to join a board that the user is already a member of.
        /// </summary>
        public void Test_Join_Board_Already_Member()
        {
            Console.WriteLine("Running Test_Join_Board_Already_Member...");
            string email = "alreadyMember@gmail.com";
            string password = "Password1";
            string boardName = "TeamBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.BoardService.JoinBoard(email, 14);
            string res = serviceFactory.BoardService.JoinBoard(email, 14);
            Response o = JsonSerializer.Deserialize<Response>(res);

            if (o.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Join_Board_Already_Member: " + o.ErrorMessage + "\n");
            }
            else
            {
                Console.WriteLine("Test_Join_Board_Already_Member failed: user should not be able to join a board they are already in\n");
            }
        }


        /// <summary>
        /// Requirement 12: Fail joining a non-existent board.
        /// </summary>
        public void Test_Join_NonExistent_Board_Fails()
        {
            Console.WriteLine("Running Test_Join_NonExistent_Board_Fails...");

            string userEmail = "Joinfail@example.com";
            string password = "Password1";
            serviceFactory.UserService.Register(userEmail, password);
            int nonExistentBoardId = 500;
            string result = serviceFactory.BoardService.JoinBoard(userEmail, nonExistentBoardId);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Join_NonExistent_Board_Fails\n");
            }
            else
            {
                Console.WriteLine("Test_Join_NonExistent_Board_Fails failed : should not allow to join non-exist board. \n");
            }
        }

        /// <summary>
        /// Requirement 13: Board owner can transfer ownership to another board member (a user who joined).
        /// </summary>
        public void Test_Transfer_Board_Ownership_Successful()
        {
            Console.WriteLine("Running Test_Transfer_Board_Ownership_Successful...");
            string ownerEmail = "BoardTransfer@gmail.com";
            string memberEmail = "NewOwnerBoard@gmail.com";
            string password = "Password1";
            string boardName = "TransformBoard";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.UserService.Register(memberEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, boardName);
            serviceFactory.BoardService.JoinBoard(memberEmail, 15);
            string transferRes = serviceFactory.BoardService.TransferOwnership(ownerEmail, memberEmail, boardName);
            Response transferResp = JsonSerializer.Deserialize<Response>(transferRes);
            if (transferResp.ErrorMessage == null)
            {
                Console.WriteLine("Passed Test_Transfer_Board_Ownership_Successful\n");
            }
            else
            {
                Console.WriteLine("Test_Transfer_Board_Ownership_Successful failed: " + transferResp.ErrorMessage + "\n");
            }
        }

        /// <summary>
        /// Requirement 13: Fail to transfer board ownership to a user who is not a board member.
        /// </summary>
        public void Test_Transfer_Board_Ownership_To_NonMember_Fails()
        {
            Console.WriteLine("Running Test_Transfer_Board_Ownership_To_NonMember_Fails...");
            string ownerEmail = "actualOwner@example.com";
            string nonMemberEmail = "notMember1@example.com";
            string password = "Password1";
            string boardName = "NonMemberTransferBoard";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.UserService.Register(nonMemberEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, boardName);
            string transferRes = serviceFactory.BoardService.TransferOwnership(ownerEmail, nonMemberEmail, boardName);
            Response transferResp = JsonSerializer.Deserialize<Response>(transferRes);
            if (transferResp.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Transfer_Board_Ownership_To_NonMember_Fails\n");
            }
            else
            {
                Console.WriteLine("FAILED: Test_Transfer_Board_Ownership_To_NonMember_Fails — ownership should not be transferred to a non-member.\n");
            }
        }

        /// <summary>
        /// Requirement 13: Fail to transfer board ownership when the user is not the owner.
        /// </summary>
        public void Test_Transfer_Board_Ownership_By_NonOwner_Fails()
        {
            Console.WriteLine("Running Test_Transfer_Board_Ownership_By_NonOwner_Fails...");
            string ownerEmail = "trueowner@example.com";
            string memberEmail = "member@example.com";
            string password = "Password1";
            string boardName = "OwnerOnlyTransferBoard";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.UserService.Register(memberEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, boardName);
            string joinRes = serviceFactory.BoardService.JoinBoard(memberEmail, 17);
            string transferRes = serviceFactory.BoardService.TransferOwnership(memberEmail, ownerEmail, boardName);
            Response transferResp = JsonSerializer.Deserialize<Response>(transferRes);
            if (transferResp.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Transfer_Board_Ownership_By_NonOwner_Fails\n");
            }
            else
            {
                Console.WriteLine("FAILED: Test_Transfer_Board_Ownership_By_NonOwner_Fails — non-owner should not be able to transfer ownership.\n");
            }
        }

        /// <summary>
        /// Requirement 12: User successfully leaves a board they joined.
        /// </summary>
        public void Test_leave_Board_Successful()
        {
            Console.WriteLine("Running Test_leave_Board_Successful...");
            string email = "LeaveBoardOwner@gmail.com";
            string password = "Password1";
            string boardName = "leaveBoard";
            serviceFactory.UserService.Register(email, password);
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.UserService.Register("LeaveTestMember@gmail.com", "Password1");
            serviceFactory.BoardService.JoinBoard("LeaveTestMember@gmail.com", 18);
            string res = serviceFactory.BoardService.LeaveBoard("LeaveTestMember@gmail.com", 18);
            Response o = JsonSerializer.Deserialize<Response>(res);
            if (o.ErrorMessage == null)
            {
                Console.WriteLine("Passed Test_leave_Board_Successful" + "\n");
            }
            else
            {
                Console.WriteLine("Test_leave_Board_Successful failed: " + o.ErrorMessage.ToString() + "\n");
            }
        }

        /// <summary>
        /// Requirement 12: User fails to leave a board they are not a member of.
        /// </summary>
        public void Test_Leave_Unjoined_Board_Fails()
        {
            Console.WriteLine("Running Test_Leave_Unjoined_Board_Fails...");
            string ownerEmail = "owner@example.com";
            string password = "Password1";
            serviceFactory.UserService.Register(ownerEmail, password);
            serviceFactory.BoardService.CreateBoard(ownerEmail, "FailCaseBoard");
            string notMemEmail = "memberUserFail@example.com";
            serviceFactory.UserService.Register(notMemEmail, password);
            string leaveRes = serviceFactory.BoardService.LeaveBoard(notMemEmail, 19);
            Response leaveResponse = JsonSerializer.Deserialize<Response>(leaveRes);
            if (leaveResponse.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Leave_Unjoined_Board_Fails\n");
            }
            else
            {
                Console.WriteLine("Test_Leave_Unjoined_Board_Fails failed: user who never joined should not be able to leave the board.\n");
            }
        }


        /// <summary>
        /// Requirement 14: A board owner cannot leave their own board.
        /// </summary>
        public void Test_Leave_Own_Board_Fails()
        {
            Console.WriteLine("Running Test_Leave_Own_Board_Fails...");

            string userEmail = "leavefail@example.com";
            string password = "Password1";
            serviceFactory.UserService.Register(userEmail, password);
            serviceFactory.BoardService.CreateBoard(userEmail, "LeaveFail");
            string result = serviceFactory.BoardService.LeaveBoard(userEmail, 20);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response.ErrorMessage != null)
            {
                Console.WriteLine("Passed Test_Leave_Own_Board_Fails\n");
            }
            else
            {
                Console.WriteLine("Test_Leave_Own_Board_Fails failed : owner should not be allowed to leave their board. \n");
            }
        }

        public void TestGetUserBoards()
        {
            string email = "alice@example.com";
            string password = "Password1";
            (serviceFactory.UserService).Register(email, password);
            (serviceFactory.BoardService).CreateBoard(email, "ProjectAlpha");
            string result = (serviceFactory.BoardService).GetUserBoards(email);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response.ErrorMessage == null && response.ReturnValue != null)
            {
                Console.WriteLine("TestGetUserBoards SUCCESS: Returned board list correctly.");
            }
            else
            {
                Console.WriteLine("TestGetUserBoards FAILED: Did not return expected board list.");
            }

        }

        public void TestGetBoardName()
        {
            string email = "getName@example.com";
            string password = "Password1";
            (serviceFactory.UserService).Register(email, password);
            (serviceFactory.BoardService).CreateBoard(email, "toTestProject");
            string result = (serviceFactory.BoardService).GetBoardName(22);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response.ErrorMessage == null && response.ReturnValue != null && response.ReturnValue.ToString() == "toTestProject")
            {
                Console.WriteLine("TestGetBoardName SUCCESS: Returned board name correctly.");
            }
            else
            {
                Console.WriteLine("TestGetBoardName FAILED: Did not return expected board name.");
            }

        }
        public void TestCounter() {
            serviceFactory.printData();
        }
    }
}

