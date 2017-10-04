using Dapper.GraphQL.Test.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.EntityMappers
{
    public class PersonEntityMapper :
        IEntityMapper<Person>
    {
        public Func<Person, Person> ResolveEntity { get; set; }

        public Person Map(IEnumerable<object> objs)
        {
            Person person = null;
            foreach (var obj in objs)
            {
                if (obj is Person p)
                {
                    person = ResolveEntity(p);
                    continue;
                }
                if (obj is Email email)
                {
                    person.Emails.Add(email);
                    continue;
                }
                if (obj is Phone phone)
                {
                    person.Phones.Add(phone);
                    continue;
                }
            }

            return person;
        }
    }
}