using Dapper.GraphQL;
using Dapper.GraphQL.Test.Models;
using System.Linq;

namespace Dapper.GraphQL.Test.EntityMappers
{
    public class PersonEntityMapper :
        EntityMapper<Person>
    {
        private readonly CompanyEntityMapper companyEntityMapper;
        private readonly DeduplicatingEntityMapper<Person> personEntityMapper;

        public PersonEntityMapper()
        {
            companyEntityMapper = new CompanyEntityMapper();

            // FIXME: DeduplicatingEntityMapper is broken here, why?
            personEntityMapper = new DeduplicatingEntityMapper<Person>
            {
                Mapper = this,
                PrimaryKey = p => p.Id,
                ReturnsNullWithDuplicates = false,
            };
        }

        public override Person Map(EntityMapContext context)
        {
            // NOTE: Order is very important here.  We must map the objects in
            // the same order they were queried in the QueryBuilder.
            var person = context.Start<Person>();
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

                person.Supervisor = supervisor;
                person.CareerCounselor = careerCounselor;
            }

            return person;
        }
    }
}