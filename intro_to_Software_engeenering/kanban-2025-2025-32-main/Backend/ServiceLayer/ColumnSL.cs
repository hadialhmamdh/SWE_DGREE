using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class ColumnSL
    {
        public string Name { get; set; }
        public List<TaskSL> Tasks { get; set; }
        public int Limit { get; set; }
        internal ColumnSL(Column column)
        {
            this.Name = (column.Name).ToString();
            this.Tasks = column.GetAllTasks()
                             .Select(task => new TaskSL(task))
                             .ToList();
            this.Limit = column.Limit;
        }
        public ColumnSL()
        {
            Tasks = new List<TaskSL>();
        }
    }
}
