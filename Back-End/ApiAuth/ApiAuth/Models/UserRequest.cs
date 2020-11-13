using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuth.Models
{
    public class UserRequest : LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
