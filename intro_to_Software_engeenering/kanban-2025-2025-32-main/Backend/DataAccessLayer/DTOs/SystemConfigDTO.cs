using System;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class SystemConfigDTO
    {
        public const string KeyColumn = "Key";
        public const string ValueColumn = "Value";

        public string Key { get; private set; }

        private string _value;
        private readonly SystemConfigController _controller;

        public string Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _controller.Update(Key, ValueColumn, value);
                _value = value;
            }
        }

        public bool IsPersisted { get; private set; } = false;

        public SystemConfigDTO(string key, string value)
        {
            Key = key;
            _value = value;
            _controller = new SystemConfigController();
        }

        public void Save()
        {
            if (IsPersisted)
                throw new InvalidOperationException("Config key already exists in the database.");

            _controller.Insert(this);
            IsPersisted = true;
        }

        public void Delete()
        {
            if (!IsPersisted)
                throw new InvalidOperationException("Cannot delete a non-persisted config.");

            _controller.Delete(Key);
            IsPersisted = false;
        }

    }
}
