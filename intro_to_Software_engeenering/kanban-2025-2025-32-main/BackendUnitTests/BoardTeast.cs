using NUnit.Framework;
using System;
using System.Collections.Generic;
using IntroSE.Kanban.Backend.BusinessLayer;
using Task = IntroSE.Kanban.Backend.BusinessLayer.Task;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace BackendUnitTests
{
    [TestFixture]
    public class BoardTests
    {
        private Board board;
        private ServiceFactory serviceFactory;

        [SetUp]
        public void Setup()
        {
            DBInitializer.Initialize();           
            serviceFactory = new ServiceFactory(); 
            serviceFactory.DeleteData();           
            board = new Board("TestBoard", 1, "owner@email.com"); 
        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void AddTask_ValidTask_Success()
        {
            // Arrange
            string email = "owner@email.com";
            string title = "Task 1";
            string description = "Description";
            DateTime creationTime = DateTime.Now;
            DateTime dueDate = creationTime.AddDays(3);

            // Act
            Task task = board.AddTask(email, title, description, dueDate);

            // Assert
            Assert.IsNotNull(task);
            Assert.AreEqual(title, task.Title);
        }

        [Test]
        public void AddTask_BacklogLimitZero_fail()
        {
            // Arrange
            string email = "owner@email.com";
            string title = "Task 1";
            string description = "Description";
            DateTime dueDate = DateTime.Now.AddDays(3);

            board.SetLimit(ColumnCategory.Backlog, 0);

            // Act + Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                board.AddTask(email, title, description, dueDate));

            Assert.IsNotNull(ex);
        }

        [TestCase(3)]
        [TestCase(20)]
        [TestCase(0)]
        public void SetLimit_ValidLimit_Success(int limit)
        {
            // Arrange + Act
            board.SetLimit(ColumnCategory.Backlog, limit);

            // Assert
            Assert.AreEqual(limit, board.GetLimit(ColumnCategory.Backlog));
        }

        [TestCase(-2)]
        [TestCase(-5)]
        [TestCase(int.MinValue)]
        public void SetLimit_InvalidNegativeValue_fail(int invalidLimit)
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                board.SetLimit(ColumnCategory.Backlog, invalidLimit));
        }


        [Test]
        public void MoveTaskToProgress_NotAssignee_fail()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "user2@email.com");

            // Act + Assert
            Assert.Throws<ArgumentException>(() => board.MoveTaskToProgress(task.Id, "owner@email.com"));
        }

        [Test]
        public void MoveTaskToProgress_Success()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "owner@email.com");

            // Act
            bool result = board.MoveTaskToProgress(task.Id, "owner@email.com");

            // Assert
            Assert.True(result);
        }

        [Test]
        public void MoveTaskToProgress_InProgressFull_fail()
        {
            // Arrange
            board.SetLimit(ColumnCategory.InProgress, 0); 
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "owner@email.com");

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                board.MoveTaskToProgress(task.Id, "owner@email.com"));
        }


        [Test]
        public void MoveTaskToDone_NotAssignee_fail()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "user@email.com");
            board.MoveTaskToProgress(task.Id, "user@email.com");

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                board.MoveTaskToDone(task.Id, "owner@email.com"));
        }

        [Test]
        public void MoveTaskToDone_Success()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "owner@email.com");
            board.MoveTaskToProgress(task.Id, "owner@email.com");

            // Act
            bool result = board.MoveTaskToDone(task.Id, "owner@email.com");

            // Assert
            Assert.True(result);
        }

        [Test]
        public void MoveTaskToDone_DoneFull_fail()
        {
            // Arrange
            board.SetLimit(ColumnCategory.Done, 0); 
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "owner@email.com");
            board.MoveTaskToProgress(task.Id, "owner@email.com");

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                board.MoveTaskToDone(task.Id, "owner@email.com"));
        }

        [Test]
        public void AssignTask_NotAssignee_fail()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "user1@email.com");

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                board.AssignTask("user2@email.com", ColumnCategory.Backlog, task.Id, "user3@email.com"));
        }

        [Test]
        public void AssignTask_AssignByOwner_Success()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));

            // Act
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task.Id, "user@email.com");

            // Assert
            Assert.AreEqual("user@email.com", task.Assignee);
        }


        [Test]
        public void ContainTask_TaskNotFound_fail()
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() => board.ContainTask(-999));
        }

        [Test]
        public void ContainTask_TaskInBacklog_ReturnsBacklog()
        {
            // Arrange
            Task task = board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));

            // Act
            ColumnCategory result = board.ContainTask(task.Id);

            // Assert
            Assert.AreEqual(ColumnCategory.Backlog, result);
        }

        [Test]
        public void GetAllInProgress_EmptyInitially_Success()
        {
            // Act
            var tasks = board.GetAllInProgress();

            // Assert
            Assert.IsEmpty(tasks);
        }

        [Test]
        public void IsOwner_ValidEmail_Success()
        {
            // Arrange
            board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));

            // Act
            bool result = board.IsOwner("owner@email.com");

            // Assert
            Assert.True(result);
        }

        [Test]
        public void IsOwner_InvalidEmail_fail()
        {
            // Arrange
            board.AddTask("owner@email.com", "title", "desc", DateTime.Now.AddDays(1));

            // Act
            bool result = board.IsOwner("notowner@email.com");

            // Assert
            Assert.False(result);
        }


        [Test]
        public void IsMember_Success()
        {
            // Arrange
            string memberEmail = "user@email.com";
            board.AddMember(memberEmail);

            // Act
            bool result = board.IsMember(memberEmail);

            // Assert
            Assert.True(result);
        }

        [Test]
        public void IsMember_NotMember_fail()
        {
            // Arrange
            string nonMemberEmail = "notmember@email.com";

            // Act
            bool result = board.IsMember(nonMemberEmail);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void GetAllTasksOfAssign_NoTasksAssigned_success()
        {
            // Arrange
            string assignee = "user@email.com";

            // Act
            List<Task> tasks = board.GetAllTasksOfAssign(assignee);

            // Assert
            Assert.IsEmpty(tasks);
        }
        [Test]
        public void GetAllTasksOfAssign_AssignedTasksInProgress_success()
        {
            // Arrange
            string assignee = "user@email.com";

            Task task1 = board.AddTask("owner@email.com", "Task1", "desc", DateTime.Now.AddDays(1));
            Task task2 = board.AddTask("owner@email.com", "Task2", "desc", DateTime.Now.AddDays(1));

            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task1.Id, assignee);
            board.AssignTask("owner@email.com", ColumnCategory.Backlog, task2.Id, assignee);

            board.MoveTaskToProgress(task1.Id, assignee);
            board.MoveTaskToProgress(task2.Id, assignee);

            // Act
            List<Task> tasks = board.GetAllTasksOfAssign(assignee);

            // Assert
            Assert.AreEqual(2, tasks.Count);
            Assert.Contains(task1, tasks);
            Assert.Contains(task2, tasks);
        }

        [Test]
        public void IsFull_ColumnNotFull_False()
        {
            // Act
            bool result = board.IsFull(ColumnCategory.Backlog);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void IsFull_ColumnLimitZero_ReturnsTrue()
        {
            // Arrange
            board.SetLimit(ColumnCategory.Backlog, 0);

            // Act
            bool result = board.IsFull(ColumnCategory.Backlog);

            // Assert
            Assert.True(result);
        }


    }
}
