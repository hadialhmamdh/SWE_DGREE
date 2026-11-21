using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Frontend.Model
{
    internal class TaskModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DueTime { get; set; }
        public string Assignee { get; set; }
        public string CreationTime { get; set; }
        public TaskModel(TaskSL taskSL)
        {
            Title = taskSL.Title;
            Description = taskSL.Description;
            Assignee = taskSL.Assignee;
            DueTime = taskSL.DateTime.ToString("dd-MM-yyyy");
            CreationTime = taskSL.CreationTime.ToString("dd-MM-yyyy");
            
            
        }
    }
}
