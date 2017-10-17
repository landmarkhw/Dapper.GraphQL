using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.Models
{
    public enum PhoneType
    {
        Unknown = 0,
        Home = 1,
        Work = 2,
        Mobile = 3,
        Other = 4
    }

    public class Phone
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int PersonId { get; set; }
        public PhoneType Type { get; set; }
    }
}