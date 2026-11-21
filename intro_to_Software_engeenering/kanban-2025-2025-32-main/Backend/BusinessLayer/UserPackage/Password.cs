using System;
using System.Text.RegularExpressions;
using log4net;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Represents and validates a password according to defined security rules.
    /// </summary>
    internal class Password
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MIN = 6;
        private const int MAX = 20;
        private string _value;

        /// <summary>
        /// Gets or sets the password value.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (!IsLegal(value))
                {
                    log.Warn("Attempted to set an invalid password.");
                    throw new ArgumentException("Password must be 6-20 characters long and contain at least one uppercase letter, one lowercase letter, and one digit.");
                }
                _value = value;
            }
        }

        /// <summary>
        /// Constructs a new Password instance with the given password string.
        /// </summary>
        /// <param name="pass">The password string.</param>
        public Password(string pass)
        {
            Value = pass;
        }

        /// <summary>
        /// Checks if the password is valid according to defined rules.
        /// </summary>
        /// <param name="pass">The password to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        private bool IsLegal(string pass)
        {
            if (string.IsNullOrWhiteSpace(pass))
                return false;

            if (pass.Length < MIN || pass.Length > MAX)
                return false;

            bool hasUpper = false, hasLower = false, hasDigit = false;

            foreach (char c in pass)
            {
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsLower(c)) hasLower = true;
                if (char.IsDigit(c)) hasDigit = true;

                if (hasUpper && hasLower && hasDigit)
                    return true;
            }

            return false;
        }
    }
}
