using Dapper.GraphQL;
using Dapper.GraphQL.Test.Models;
using System.Linq;

namespace Dapper.GraphQL.Test.EntityMappers
{
    public class PersonEntityMapper :
        DeduplicatingEntityMapper<Person>
    {
        private CompanyEntityMapper companyEntityMapper;
        private PersonEntityMapper personEntityMapper;

        public PersonEntityMapper()
        {
            Mapper = new EntityMapper<Person>();
            PrimaryKey = p => p.Id;
            ReturnsNullWithDuplicates = true;
        }

        public override Person Map(EntityMapContext context)
        {
            // Avoid creating the mappers until they're used
            // NOTE: this avoids an infinite loop (had these been created in the ctor)
            if (companyEntityMapper == null)
            {
                companyEntityMapper = new CompanyEntityMapper();
            }
            if (personEntityMapper == null)
            {
                personEntityMapper = new PersonEntityMapper
                {
                    ReturnsNullWithDuplicates = false
                };
            }

            // NOTE: Order is very important here.  We must map the objects in
            // the same order they were queried in the QueryBuilder.

            // Start with the person, and deduplicate
            var person = Deduplicate(context.Start<Person>());
            var company = context.Next<Company>("companies", companyEntityMapper);
            var email = context.Next<Email>("emails");
            var phone = context.Next<Phone>("phones");
            var supervisor = context.Next<Person>("supervisor", personEntityMapper);
            var careerCounselor = context.Next<Person>("careerCounselor", personEntityMapper);

            if (person != null)
            {
                if (email != null &&
                    // Eliminate duplicates
                    !person.Emails.Any(e => e.Address == email.Address))
                {
                    person.Emails.Add(email);
                }

                if (phone != null &&
                    // Eliminate duplicates
                    !person.Phones.Any(p => p.Number == phone.Number))
                {
                    person.Phones.Add(phone);
                }

                person.Supervisor = person.Supervisor ?? supervisor;
                person.CareerCounselor = person.CareerCounselor ?? careerCounselor;
            }

            return person;
        }
    }
}