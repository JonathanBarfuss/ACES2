using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Models;

namespace ACES.Services
{
    interface IEmailService
    {
        Task SendEmailAsync(Email mailRequest);
    }
}
