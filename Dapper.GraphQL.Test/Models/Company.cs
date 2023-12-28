using System.Collections.Generic;

namespace Dapper.GraphQL.Test.Models
{
    public class Company
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Email> Emails { get; set; }

        public IList<Phone> Phones { get; set; }

        public Company()
        {
            Emails = new List<Email>();
            Phones = new List<Phone>();
        }
    }
}
