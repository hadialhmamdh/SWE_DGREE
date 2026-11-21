using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class BoardSL
    {
        public string Name { get; set; }
        public string Owner {  get; set; }
        public List<string> Members { get; set; }
        public List<ColumnSL> Columns { get; set; }
        internal BoardSL (Board board)
        { 
            this.Name = board.Name;
            this.Owner = board.Owner;
            this.Members = board.Members;
            this.Columns = board.GetAllColumns()
            .Select(col => new ColumnSL(col))
            .ToList();
        }
        // Add this parameterless constructor for JSON deserialization
        public BoardSL()
        {
            Members = new List<string>();
            Columns = new List<ColumnSL>();
        }
    }
}
