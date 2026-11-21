using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class TaskSL
    {
        public string Title {  get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Assignee { get; set; }
        public DateTime CreationTime { get; set; }
        internal TaskSL(Task task)
        {
            this.Title = task.Title;
            this.Description = task.Description;
            this.Assignee = task.Assignee;
            this.DateTime=task.DueDate;
            this.CreationTime=task.CreationDate;
        }
        public TaskSL() { }
    }
}
