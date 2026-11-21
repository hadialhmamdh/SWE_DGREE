using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class ColumnDTO
    {
        public const string BoardIdColumnName = "BoardID";
        public const string LimitColumnName = "Limit";   
        public const string ColumnColumnName = "Column"; 

        public string ColumnName { get; set; }
        public int  BoardId { get; set; }
        public bool IsPersisted { get; set; }=false;
        private int limit;
        private ColumnController _controller;

        public int Limit
        {
            get => limit;
            set
            {
                if (IsPersisted)
                {
                    _controller.UpdateLimit(LimitColumnName,ColumnName,BoardId,value);
                }
                limit = value;
            }
        }

        

        public ColumnDTO(string columnname)
        {
           this.ColumnName = columnname;
            this._controller = new ColumnController();
        }

        internal void Save()
        {

            if (IsPersisted)
            {
                throw new ArgumentException("Cannot save persisted Object");
            }
        }
        public void AddTask(TaskDTO task)
        {
            if (task.IsPersisted)
            {
                task.ColumnName = ColumnName;
            }
            else
            {
                task.Save(this.BoardId, this.ColumnName);
            }
           
        }

        public void Save(int id)
        {
            if (IsPersisted)
            {
                throw new ArgumentException("Cannot save persisted Object");
            }
            this.BoardId = id;
            _controller.Insert(this);
            IsPersisted = true;
        }
    }
}
