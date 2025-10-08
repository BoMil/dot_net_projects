using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{

    public class LocalMailService : IMailService
    {
        private string _mailTo = String.Empty;
        private string _mailFrom = String.Empty;

        public LocalMailService(IConfiguration configuration)
        {
            // This will get this values from the appsettings.json file
            _mailFrom = configuration.GetValue<string>("mailSettings:from") ?? "";
            _mailTo = configuration.GetValue<string>("mailSettings:to") ?? "";
        }

        public void SendMail(string subject, string message)
        {
            Console.WriteLine($"Sending mail to {_mailTo} from {_mailFrom}");
            Console.WriteLine($"Subject {subject}");
            Console.WriteLine($"Message {message}");

        }

    }
}