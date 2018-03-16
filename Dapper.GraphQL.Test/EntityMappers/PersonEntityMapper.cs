using Dapper.GraphQL.Test.Models;
using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.GraphQL.Test.EntityMappers
{
    public class PersonEntityMapper :
        EntityMapper<Person>
    {
        public override Person Map(
            object[] objs,
            IHaveSelectionSet selectionSet = null,
            List<Type> splitOn = null)
        {
            Person person = null;

            foreach (var obj in objs)
            {
                if (person == null &&
                    obj is Person p)
                {
                    person = ResolveEntity(p);
                    continue;
                }
                if (obj is Email email &&
                    // Eliminate duplicates
                    !person.Emails.Any(e => e.Address == email.Address))
                {
                    person.Emails.Add(email);
                    continue;
                }
                if (obj is Phone phone &&
                    // Eliminate duplicates
                    !person.Phones.Any(ph => ph.Number == phone.Number))
                {
                    person.Phones.Add(phone);
                    continue;
                }
            }

            if (selectionSet != null)
            {
                var fields = selectionSet.GetSelectedFields();
                int supervisorIndex = -1;
                // Start at 1 to skip the "first" person object in the list,
                // which is the person we just mapped.
                int startingIndex = 1;

                // NOTE: order matters here, if both supervisor
                // and careerCounselor exist, then supervisor must appear
                // first in the list.  This order is guaranteed in PersonQueryBuilder.
                if (fields.ContainsKey("supervisor"))
                {
                    supervisorIndex = splitOn.IndexOf(typeof(Person), startingIndex);
                    if (supervisorIndex >= 0)
                    {
                        person.Supervisor = objs[supervisorIndex] as Person;
                    }
                }
                if (fields.ContainsKey("careerCounselor"))
                {
                    var careerCounselorIndex = splitOn.IndexOf(typeof(Person), Math.Max(startingIndex, supervisorIndex + 1));
                    if (careerCounselorIndex >= 0)
                    {
                        person.CareerCounselor = objs[careerCounselorIndex] as Person;
                    }
                }
            }

            return person;
        }
    }
}