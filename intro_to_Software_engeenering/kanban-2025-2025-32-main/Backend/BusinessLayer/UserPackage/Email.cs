using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using log4net;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Represents and validates an email address format.
    /// </summary>
    internal class Email
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _value;

        /// <summary>
        /// Gets or sets the email value.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (!IsValidEmail(value))
                {
                    log.Warn($"Invalid email format attempted: {value}");
                    throw new ArgumentException("Invalid email address.");
                }
                _value = value;
            }
        }

        /// <summary>
        /// Constructs a new Email instance with the given string.
        /// </summary>
        /// <param name="email">The email address string.</param>
        public Email(string email)
        {
            Value = email;
        }

        /// <summary>
        /// Validates the email format.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email); // assuming you include using System.Net.Mail
                return addr.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        
    }
}
