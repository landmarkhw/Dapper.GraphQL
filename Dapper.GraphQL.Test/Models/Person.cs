using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.Models
{
    public class Person
    {
        public IList<Email> Emails { get; set; }
        public string FirstName { get; set; }
        public int Id { get; set; }
        public string LastName { get; set; }
        public IList<Phone> Phones { get; set; }

        public Person()
        {
            Emails = new List<Email>();
            Phones = new List<Phone>();
        }
    }
}