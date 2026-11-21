using System;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class BoardDTO
    {
        public const string IdColumnName = "Id";
        public const string NameColumnName = "Name";
        public const string OwnerColumnName = "Owner";
        public const string CounterColumnName = "TaskCounter";

        public int Id { get; private set; }
        private string _name;
        private string _owner;
        private int _counter;

        private readonly BoardController _controller;
        private readonly BoardMemberController _memberController;

        public bool IsPersisted { get;  set; } = false;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _controller.Update(Id, NameColumnName, value);
                _name = value;
            }
        }

        public string Owner
        {
            get => _owner;
            set
            {
                if (_owner == value)
                    return;

                _controller.Update(Id, OwnerColumnName, value);
                _owner = value;
            }
        }

        public int TaskCounter
        {
            get => _counter;
            set
            {
                if (_counter == value)
                    return;

                _controller.Update(Id, CounterColumnName, value);
                _counter = value;
            }
        }

        public BoardDTO(int id, string name, string owner, int counter = 0)
        {
            Id = id;
            _name = name;
            _owner = owner;
            _counter = counter;
            _controller = new BoardController();
            _memberController = new BoardMemberController();
        }

        /// <summary>
        /// Adds a user to the board's members table.
        /// </summary>
        public void JoinBoard(string userEmail)
        {
            var member = new BoardMemberDTO(Id, userEmail);
            member.Save();
        }

        /// <summary>
        /// Removes a user from the board's members table.
        /// </summary>
        public void LeaveBoard(string userEmail)
        {
            _memberController.DeleteMember(Id, userEmail);
        }

        /// <summary>
        /// Deletes the board and all its members from the database.
        /// </summary>
        public void Delete()
        {
            if (!IsPersisted)
                throw new InvalidOperationException("Board does not exist in the database.");
            new TaskController().DeleteTasksByBoard(Id);
            new ColumnController().DeleteColumnsByBoard(Id);
            _controller.DeleteBoard(Id);
            IsPersisted = false;
        }

        /// <summary>
        /// Persists the board to the database.
        /// </summary>
        public void Save()
        {
            if (IsPersisted)
                throw new InvalidOperationException("Board already exists in the database.");

            _controller.Insert(this);
            IsPersisted = true;
        }
        public void AddColumn(ColumnDTO column)
        {
           column.Save(Id);
        }
    }
}

