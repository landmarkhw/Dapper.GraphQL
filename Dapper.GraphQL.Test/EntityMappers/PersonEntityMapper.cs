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
            IHaveSelectionSet selectionSet,
            List<Type> splitOn)
        {
            var person = objs[0] as Person;
            if (person != null)
            {
                var selectedFields = selectionSet.GetSelectedFields();

                if (selectedFields.ContainsKey("emails"))
                {
                    var index = splitOn.IndexOf(typeof(Email));
                    if (index >= 0 &&
                        index < objs.Length &&
                        objs[index] is Email email &&
                        // Eliminate duplicates
                        !person.Emails.Any(e => e.Address == email.Address))
                    {
                        person.Emails.Add(email);
                    }
                }
                if (selectedFields.ContainsKey("phones"))
                {
                    var index = splitOn.IndexOf(typeof(Phone));
                    if (index >= 0 &&
                        index < objs.Length &&
                        objs[index] is Phone phone &&
                        // Eliminate duplicates
                        !person.Phones.Any(ph => ph.Number == phone.Number))
                    {
                        person.Phones.Add(phone);
                    }
                }

                // Start at 1 to skip the "first" person object in the list,
                // which is the person we just mapped.
                int startingIndex = 1;

                // NOTE: order matters here, if both supervisor
                // and careerCounselor exist, then supervisor must appear
                // first in the list.  This order is guaranteed in PersonQueryBuilder.
                if (selectedFields.ContainsKey("supervisor"))
                {
                    if (startingIndex < splitOn.Count)
                    {
                        var index = splitOn.IndexOf(typeof(Person), startingIndex);
                        if (index >= 0 &&
                            index < objs.Length)
                        {
                            startingIndex = index + 1;
                            person.Supervisor = objs[index] as Person;
                        }
                    }
                }
                if (selectedFields.ContainsKey("careerCounselor"))
                {
                    if (startingIndex < splitOn.Count)
                    {
                        var index = splitOn.IndexOf(typeof(Person), startingIndex);
                        if (index >= 0 &&
                            index < objs.Length)
                        {
                            person.CareerCounselor = objs[index] as Person;
                        }
                    }
                }
            }
            
            return person;
        }
    }
}