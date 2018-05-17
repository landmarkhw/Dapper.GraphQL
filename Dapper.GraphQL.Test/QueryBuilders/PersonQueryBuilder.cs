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

            if (fields.ContainsKey("firstName"))
            {
                query.Select($"{alias}.FirstName");
            }
            if (fields.ContainsKey("lastName"))
            {
                query.Select($"{alias}.LastName");
            }
            if (fields.ContainsKey("emails"))
            {
                var emailAlias = $"{alias}Email";
                query.LeftJoin($"Email {emailAlias} ON {alias}.Id = {emailAlias}.PersonId");
                query = emailQueryBuilder.Build(query, fields["emails"], emailAlias);
            }
            if (fields.ContainsKey("phones"))
            {
                var phoneAlias = $"{alias}Phone";
                query.LeftJoin($"Phone {phoneAlias} ON {alias}.Id = {phoneAlias}.PersonId");
                query = phoneQueryBuilder.Build(query, fields["phones"], phoneAlias);
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