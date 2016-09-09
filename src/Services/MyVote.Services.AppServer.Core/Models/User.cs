using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace MyVote.Services.AppServer.Models
{
    public sealed class User : IdentityUser
    {
        public string ProfileID { get; set; }
        public int? UserID { get; set; }
        public override string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PostalCode { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}