using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.Models
{
    public class Person
    {
        public Person CareerCounselor { get; set; }
        public IList<Company> Companies { get; set; }
        public IList<Email> Emails { get; set; }
        public string FirstName { get; set; }
        public int Id { get; set; }
        public string LastName { get; set; }
        public IList<Phone> Phones { get; set; }
        public Person Supervisor { get; set; }

        public Person()
        {
            Companies = new List<Company>();
            Emails = new List<Email>();
            Phones = new List<Phone>();
        }
    }
}