using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Frontend.Model
{
    internal class ColumnModel
    {
        public string Name { get; set; }
        public ObservableCollection<TaskModel> Tasks { get; set; }
        public int Limit { get; set; }
        public ColumnModel(ColumnSL columnSL) 
        {
            Name = columnSL.Name;
            Tasks =new ObservableCollection<TaskModel>( columnSL.Tasks
                             .Select(task => new TaskModel(task))
                             .ToList());
            Limit = columnSL.Limit;
        }
    }
}
