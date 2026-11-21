using System;
using BackendTests.ServiceLayerTests;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace BackendTests
{
    internal class MainTests
    {
        public static void Main(string[] args)
        {
            DBInitializer.Initialize();

            // Load all users and boards
            /*var t = new ServiceFactory();
            t.LoadData();
            (t).printData();*/



            ///Create instances of test classes
            var userServiceTest = new UserServiceTest();
            var boardServiceTest = new BoardServiceTest();
            var taskServiceTest = new TaskServiceTest();

            // Run all test methods from TaskServiceTest
            taskServiceTest.TestAddTaskSuccess();
            taskServiceTest.TestEditTaskTitleSuccess();
            taskServiceTest.TestEditTaskDueTimeSuccess();
            taskServiceTest.TestEditTaskDescriptionSuccess();
            taskServiceTest.TestAdvanceTaskSuccessful();
            taskServiceTest.TestAdvanceTaskFailed();
            taskServiceTest.TestGetAllInProgressSuccessful();
            taskServiceTest.TestGetAllInProgressFailed();
            taskServiceTest.TestAddTaskFailed();
            taskServiceTest.TestEditTaskTitleFailed();
            taskServiceTest.TestEditTaskDueTimeFailed();
            taskServiceTest.TestEditTaskDescriptionFailed();
            taskServiceTest.Test_AssignTask_Successful();
            taskServiceTest.Test_AssignTask_Failed();
            taskServiceTest.TestEditTaskTitleByNonAssigneeFailed();
            taskServiceTest.TestEditTaskDueTimeByNonAssigneeFailed();
            taskServiceTest.TestEditTaskDescriptionByNonAssigneeFailed();
            taskServiceTest.CleanTests();
            // Run all test methods from userServiceTest
            userServiceTest.TestRegisterUserSuccess();
            userServiceTest.TestRegisterUserInvalidPassword();
            userServiceTest.TestRegisterUserDuplicate();
            userServiceTest.LogoutSuccess();
            userServiceTest.TestLoginFailure();
            userServiceTest.TestRegisterUserInvalidEmail();

            // Run all test methods from BoardServiceTest
            boardServiceTest.Test_CreateBoard_Successful();
            boardServiceTest.Test_CreateBoard_Failed_InvalidUser();
            boardServiceTest.Test_DeleteBoard_Successful();
            boardServiceTest.Test_DeleteBoard_Failed_InvalidBoard();
            boardServiceTest.Test_DuplicateBoardName_SameUser();
            boardServiceTest.Test_SameBoardName_DifferentUsers();
            boardServiceTest.Test_SetColumnLimit_Successful();
            boardServiceTest.Test_SetColumnLimit_Failed_InvalidColumn();
            boardServiceTest.Test_GetColumnLimit_Successful();
            boardServiceTest.Test_GetColumnLimit_Failed_InvalidColumn();
            boardServiceTest.Test_Move_Backlog_To_InProgress_Successful();
            boardServiceTest.Test_Move_Backlog_To_InProgress_Failed();
            boardServiceTest.Test_Move_InProgress_To_Done_Successful();
            boardServiceTest.Test_Move_InProgress_To_Done_Failed();
            boardServiceTest.Test_Join_Board_Successful();
            boardServiceTest.Test_Join_NonExistent_Board_Fails();
            boardServiceTest.Test_Join_Board_Already_Member();
            boardServiceTest.Test_Transfer_Board_Ownership_Successful();
            boardServiceTest.Test_Transfer_Board_Ownership_To_NonMember_Fails();
            boardServiceTest.Test_Transfer_Board_Ownership_By_NonOwner_Fails();
            boardServiceTest.Test_leave_Board_Successful();
            boardServiceTest.Test_Leave_Unjoined_Board_Fails();
            boardServiceTest.Test_Leave_Own_Board_Fails();
            boardServiceTest.TestGetUserBoards();
            boardServiceTest.TestGetBoardName();
            boardServiceTest.CleanTests();
        }
    }
}