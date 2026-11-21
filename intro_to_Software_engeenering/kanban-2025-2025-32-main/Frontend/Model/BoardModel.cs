using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Frontend.Model
{
    internal class BoardModel
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public List<string> Members { get; set; }
        public List<ColumnModel> Columns { get; set; }
        public BoardModel(BoardSL board)
        {
            Name = board.Name;
            Owner = board.Owner;
            Members = board.Members;
            Columns = board.Columns
                             .Select(column => new ColumnModel(column))
                             .ToList(); ;

        }
    }
}
