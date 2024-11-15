using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ensek.Model
{
    public class AccountRecord
    {
        public int AccountId { get; set; }

        public string? FristName { get; set; }

        public string? LastName { get; set; }

    }
}
