using System;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class BoardMemberDTO
    {
        public const string BoardIdColumn = "Board_id";
        public const string UserColumn = "User";

        public int BoardId { get; private set; }
        public string UserEmail { get; private set; }

        private BoardMemberController _controller;
        public bool IsPersisted { get;  set; } = false;

        public BoardMemberDTO(int boardId, string userEmail)
        {
            BoardId = boardId;
            UserEmail = userEmail;
            _controller = new BoardMemberController();
        }

        public void Save()
        {
            if (IsPersisted)
                throw new InvalidOperationException("BoardMember already exists in the database.");

            _controller.Insert(this);
            IsPersisted = true;
        }
        public void Delete()
        {
            if (!IsPersisted)
                throw new InvalidOperationException("Cannot delete a board member that is not persisted.");

            _controller.DeleteMember(BoardId, UserEmail);
            IsPersisted = false;
        }

    }
}
