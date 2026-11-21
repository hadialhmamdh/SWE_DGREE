using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class TaskDTO
    {
        public bool IsPersisted { get; set; }=false;
        public const string TitleColumnName = "Title";
        public const string DescriptionColumnName = "Description";
        public const string DueDateColumnName = "DueDate";
        public const string BoardIdColumnName = "BoardID";
        public const string AssignColumnName = "Assignee";
        public const string ColumnColumnName = "Column";
        public const string IDColumnName = "Id";
        public const string  CreationTimeColumnName="CreationTime";
        private string title;
        private string description;
        private DateTime dueDate;
        private string columnName;
        private string assigne;
        public int Id { get;  set; }
        public int BoardId { get;  set; }
        public DateTime CreationDate { get; }

        private readonly TaskController _controller;

        public TaskDTO(int id, string title, string description, DateTime creationDate, DateTime dueDate, string assign)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.CreationDate = creationDate;
            this.DueDate = dueDate; 
            this.Assign = assign;
            this._controller = new TaskController();
        }

        public string Title 
        {
            get => title;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateTaskTitle(TitleColumnName,BoardId, Id, ColumnName, value);
                }
                title = value;
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateTaskDescription(DescriptionColumnName,BoardId, Id, ColumnName, value);
                }
                description = value;
            }
        }

        public DateTime DueDate
        {
            get => dueDate;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateTaskDueDate(DueDateColumnName,BoardId, Id, ColumnName, value.ToString("o"));
                }
                dueDate = value;
            }
        }

        public string ColumnName
        {
            get => columnName;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateColumnName(ColumnColumnName,BoardId, Id, value);
                }
                columnName = value;
            }
        }
        public string Assign
        {
            get => assigne;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateTaskAssign(AssignColumnName,BoardId, Id,ColumnName ,value);
                }
                assigne = value;
            }
        }

        

        public void Save()
        {
            if (IsPersisted)
            { 
                throw new ArgumentException("Cannot save persisted Object");
            }
        }
        public void Save(int boardId,string column)
        {
            if (IsPersisted)
            {
                throw new ArgumentException("Cannot save persisted Object");
            }
            this.BoardId = boardId;
            this.ColumnName = column;
            _controller.Insert(this);
            IsPersisted = true; 
        }

    }
}
