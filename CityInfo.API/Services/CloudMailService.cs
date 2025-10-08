using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{

    public class ClouldMailService : IMailService
    {
        private string _mailTo = "boki@boki.cz";
        private string _mailFrom = "info@cityinfo.cz";

        public void SendMail(string subject, string message)
        {
            Console.WriteLine($"Sending mail to {_mailTo} from {_mailFrom}");
            Console.WriteLine($"Subject {subject}");
            Console.WriteLine($"Message {message}");

        }

    }
}