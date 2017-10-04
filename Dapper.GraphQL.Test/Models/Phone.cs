using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.Models
{
    public enum PhoneType
    {
        Home,
        Work,
        Mobile,
        Other
    }

    public class Phone
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public PhoneType Type { get; set; }
    }
}