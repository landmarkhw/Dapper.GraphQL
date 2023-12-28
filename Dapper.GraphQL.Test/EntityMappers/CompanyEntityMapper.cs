using System.Linq;
using Dapper.GraphQL.Test.Models;

namespace Dapper.GraphQL.Test.EntityMappers
{
    public class CompanyEntityMapper :
        DeduplicatingEntityMapper<Company>
    {
        public CompanyEntityMapper()
        {
            PrimaryKey = c => c.Id;
        }

        public override Company Map(EntityMapContext context)
        {
            // NOTE: Order is very important here.  We must map the objects in
            // the same order they were queried in the QueryBuilder.
            var company = Deduplicate(context.Start<Company>());
            var email = context.Next<Email>("emails");
            var phone = context.Next<Phone>("phones");

            if (company != null)
            {
                if (email != null &&

                    // Eliminate duplicates
                    !company.Emails.Any(e => e.Address == email.Address))
                {
                    company.Emails.Add(email);
                }

                if (phone != null &&

                    // Eliminate duplicates
                    !company.Phones.Any(p => p.Number == phone.Number))
                {
                    company.Phones.Add(phone);
                }
            }

            return company;
        }
    }
}
