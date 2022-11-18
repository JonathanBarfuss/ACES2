using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models
{
    public class CombinedUsers
    {
        public CombinedUsers(int id, string email, string userType)
        {
            Id = id;
            Email = email;
            UserType = userType;
        }
        public CombinedUsers() { }

        public int Id { get; set; }

        public string Email { get; set; }

        public string UserType { get; set; }

    }
}