using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.Controllers
{
    internal class BoardController
    {
        private readonly BoardService boardService;

        public BoardController(BoardService boardService)
        {
            this.boardService = boardService;
        }

        public ObservableCollection<BoardModel> GetALLBoards(string email)
        {
            string result = boardService.GetAllBoards(email);
            Response response = JsonSerializer.Deserialize<Response>(result);

            if (response?.ErrorOccured == true)
            {
                throw new ArgumentException(response.ErrorMessage);
            }

            // Proper deserialization of the ReturnValue
            if (response?.ReturnValue is JsonElement jsonElement)
            {
                var boardsSL = JsonSerializer.Deserialize<List<BoardSL>>(jsonElement.GetRawText());
                return new ObservableCollection<BoardModel>(
                    boardsSL?.Select(board => new BoardModel(board)) ?? Enumerable.Empty<BoardModel>()
                );
            }

            // Fallback if ReturnValue is already the correct type
            if (response?.ReturnValue is List<BoardSL> directList)
            {
                return new ObservableCollection<BoardModel>(
                    directList.Select(board => new BoardModel(board))
                );
            }

            // Return empty collection instead of null
            return new ObservableCollection<BoardModel>();
        }

        public void CreateBoard(string email, string name)
        {
            string result = boardService.CreateBoard(email, name);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorOccured)
            {
                throw new ArgumentException(response.ErrorMessage);
            }

        }

        public void DeleteBoard(string owner, string name)
        {
            string result = boardService.DeleteBoard(owner, name);
            Response response = JsonSerializer.Deserialize<Response>(result);
            if (response.ErrorOccured)
            {
                throw new ArgumentException(response.ErrorMessage);
            }
        }

        public ColumnModel GetColumn(string email, string boardName, int columnOrdinal)
        {
            string json = boardService.GetColumn(email, boardName, columnOrdinal);
            Response res = JsonSerializer.Deserialize<Response>(json);

            if (res.ErrorOccured)
                throw new ArgumentException(res.ErrorMessage);
            var colSl = JsonSerializer.Deserialize<ColumnSL>(((JsonElement)res.ReturnValue).GetRawText());

            return new ColumnModel(colSl);
        }

        public IList<TaskModel> FetchColumnTasks(string email, string boardName, int ordinal)
        {
            string json = boardService.GetColumn(email, boardName, ordinal);
            Response res = JsonSerializer.Deserialize<Response>(json);

            if (res.ErrorOccured)
                throw new ArgumentException(res.ErrorMessage);
            var tasksSl = JsonSerializer.Deserialize<List<TaskSL>>(
                              ((JsonElement)res.ReturnValue).GetRawText());

            return tasksSl.Select(t => new TaskModel(t)).ToList();
        }
        public IList<string> GetMembers(string ownerEmail, string boardName)
        {
            return new List<string> { ownerEmail };
        }

    }
}
