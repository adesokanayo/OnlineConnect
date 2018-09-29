using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineAD.Api.Domain
{
    public class ADModel
    {
        [Required]
        public string  username { get; set; }
        [Required]

        public string password { get; set; }
        [Required]

        public string key { get; set; }
    }

    public class ADResponse
    {
        public string Status { get; set; }
        public bool UserExist { get; set; }
        public string  ErrorMessage { get; set; }
        public string  UserGroup { get; set; }
    }

    public class StatusType
    {
        public static string Success { get; set; } = "00";
        public static  string Failed { get; set; } = "01";
    }
}

