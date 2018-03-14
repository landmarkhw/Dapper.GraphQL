using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Language.AST;
using Dapper.GraphQL.Test.Models;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class PersonQueryBuilder :
        IQueryBuilder<Person>
    {
        private readonly IQueryBuilder<Email> emailQueryBuilder;
        private readonly IQueryBuilder<Phone> phoneQueryBuilder;

        public PersonQueryBuilder(
            IQueryBuilder<Email> emailQueryBuilder,
            IQueryBuilder<Phone> phoneQueryBuilder)
        {
            this.emailQueryBuilder = emailQueryBuilder;
            this.phoneQueryBuilder = phoneQueryBuilder;
        }

        public SqlQueryContext Build(SqlQueryContext query, IHaveSelectionSet context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Person>("Id");

            var fields = context.GetSelectedFields();

            foreach (var kvp in fields)
            {
                switch (kvp.Key)
                {
                    case "firstName": query.Select($"{alias}.FirstName"); break;
                    case "lastName": query.Select($"{alias}.LastName"); break;

                    case "emails":
                        {
                            var emailAlias = $"{alias}Email";
                            query.LeftJoin($"Email {emailAlias} ON {alias}.Id = {emailAlias}.PersonId");
                            query = emailQueryBuilder.Build(query, kvp.Value, emailAlias);
                        }
                        break;

                    case "phones":
                        {
                            var phoneAlias = $"{alias}Phone";
                            query.LeftJoin($"Phone {phoneAlias} ON {alias}.Id = {phoneAlias}.PersonId");
                            query = phoneQueryBuilder.Build(query, kvp.Value, phoneAlias);
                        }
                        break;
                }
            }

            if (fields.ContainsKey("supervisor"))
            {
                var supervisorAlias = $"{alias}Supervisor";
                query.LeftJoin($"Person {supervisorAlias} ON {alias}.SupervisorId = {supervisorAlias}.Id");
                query = Build(query, fields["supervisor"], supervisorAlias);
            }
            if (fields.ContainsKey("careerCounselor"))
            {
                var careerCounselorAlias = $"{alias}CareerCounselor";
                query.LeftJoin($"Person {careerCounselorAlias} ON {alias}.CareerCounselorId = {careerCounselorAlias}.Id");
                query = Build(query, fields["careerCounselor"], careerCounselorAlias);
            }

            return query;
        }
    }
}