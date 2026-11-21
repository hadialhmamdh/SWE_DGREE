using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net.Repository.Hierarchy;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
   

    internal class Task
    {
        /// <summary>
        /// The maximum allowed length for the task title.
        /// </summary>
        const int Max_Title = 50;

        /// <summary>
        /// The maximum allowed length for the task description.
        /// </summary>
        const int Max_Description = 300;

        /// <summary>
        /// Gets the unique identifier of the task within the board.
        /// This property is immutable.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the timestamp of when the task was created.
        /// This property is immutable.
        /// </summary>
        public DateTime CreationDate { get; }

        private string title;
        private string description;
        private DateTime dueDate;
        public TaskDTO TaskDto { get; set; }
        private string assignee;


        /// <summary>
        /// Gets the current status of the task.
        /// </summary>
        //public ColumnCategory Status { get; private set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class with the specified parameters.
        /// </summary>
        /// <param name="id">The unique task identifier.</param>
        /// <param name="title">The title of the task (non-null, non-empty, max 50 characters).</param>
        /// <param name="description">The description of the task (optional, max 300 characters).</param>
        /// <param name="creationDate">The creation timestamp of the task.</param>
        /// <param name="dueDate">The due date of the task (must be after the creation time).</param>
        /// <exception cref="ArgumentNullException">Thrown when title is null.</exception>
        /// <exception cref="ArgumentException">Thrown when title is empty or too long, or due date is invalid, or description is too long.</exception>
        public Task(int id, string title, string description, DateTime creationDate, DateTime dueDate)
        {
            TaskDto = new TaskDTO(id,title,description,creationDate,dueDate,null);
            this.Id = id;
            this.CreationDate = creationDate;
            this.Title = title;
            this.Description = description;
            this.DueDate = dueDate;
            this.Assignee = null;
            TaskDto.Save();
        }
        public Task(TaskDTO taskdto)
        {
            this.TaskDto = taskdto;
            this.Id = taskdto.Id;
            this.title = taskdto.Title;
            this.description = taskdto.Description;
            this.CreationDate = taskdto.CreationDate;
            this.dueDate = taskdto.DueDate;
            this.assignee = taskdto.Assign;
           

        }
        /// <summary>
        /// Gets or sets the task title.
        /// Cannot be null, empty, exceed 50 characters, or be modified after the task is marked Done.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when title is null.</exception>
        /// <exception cref="ArgumentException">Thrown when title is empty, too long, or task is Done.</exception>
        public string Title
        {
            get => title;
            set
            {
                if (value == null)
                {
                    log.Error($"Task {Id}: Attempted to set Title to null.Title cannot be null.");
                    throw new ArgumentNullException(nameof(Title), "Title cannot be null.");   

                }
                if (value == "")
                {
                    log.Error($"Task {Id}: Attempted to set Title to empty string.Title cannot be empty.");
                    throw new ArgumentException("Title cannot be empty.");
                }
                if (value.Length > Max_Title)
                {
                    log.Error($"Task {Id}: Invalid Title - Title cannot exceed {Max_Title} characters.");
                    throw new ArgumentException($"Title cannot exceed {Max_Title} characters.");
                }
                TaskDto.Title = value;
                log.Info($"Task {Id}: Title updated from '{title}' to '{value}'.");
                title = value;
            }
        }

        /// <summary>
        /// Gets or sets the task description.
        /// Can be null. Must not exceed 300 characters or be modified after the task is marked Done.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when description is too long or task is Done.</exception>
        public string Description
        {
            get => description;
            set
            {
                if (value == null)
                {
                    log.Error($"Task {Id}: Attempted to set Description to null.Descrption cannot be null.");
                    throw new ArgumentNullException(nameof(Description), "Description cannot be null.");


                }
                if ((value != null) && (value.Length > Max_Description))
                {
                    log.Error($"Task {Id}: Invalid Description - Description cannot exceed {Max_Description} characters.");
                    throw new ArgumentException($"Description cannot exceed {Max_Description} characters.");
                }
                TaskDto.Description = value;
                log.Info($"Task {Id}: Description updated from '{description}' to '{value}'.");
                description = value;
            }
        }

        /// <summary>
        /// Gets or sets the due date of the task.
        /// Must be later than the creation time and cannot be modified after the task is marked Done.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when due date is invalid or task is Done.</exception>
        public DateTime DueDate
        {
            get => dueDate;
            set
            {
                if (value < CreationDate)
                {
                    log.Error($"Task {Id}: Invalid DueTime — attempted to set due date to '{value}', which is before the creation time '{CreationDate}'.");
                    throw new ArgumentException($"DueTime must be after CreationTime. Given: '{value}', but creation time is '{CreationDate}'.");
                }
                TaskDto.DueDate = value;
                log.Info($"Task {Id}: DueTime updated from '{dueDate}' to '{value}'.");
                dueDate = value;
            }
        }

        public string Assignee 
        {
            get => assignee;
            set
            {
                TaskDto.Assign = value;
                log.Info($"Task {Id}: Assigne updated from '{assignee}' to '{value}'.");
                assignee = value;   
            }

        }

        public override string ToString()
        {
            return $"Board [Id={Id}, Title={Title}, Description={Description}, CreationTime={CreationDate}, DueDate={DueDate}, Assignee={Assignee}]";
        }

    }
}
