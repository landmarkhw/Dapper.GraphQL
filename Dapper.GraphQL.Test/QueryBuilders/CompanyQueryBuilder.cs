using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Language.AST;
using Dapper.GraphQL.Test.Models;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class CompanyQueryBuilder :
        IQueryBuilder<Company>
    {
        private readonly IQueryBuilder<Email> emailQueryBuilder;
        private readonly IQueryBuilder<Phone> phoneQueryBuilder;

        public CompanyQueryBuilder(
            IQueryBuilder<Email> emailQueryBuilder,
            IQueryBuilder<Phone> phoneQueryBuilder)
        {
            this.emailQueryBuilder = emailQueryBuilder;
            this.phoneQueryBuilder = phoneQueryBuilder;
        }

        public SqlQueryContext Build(SqlQueryContext query, IHaveSelectionSet context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Company>("Id");

            var fields = context.GetSelectedFields();

            if (fields.ContainsKey("name"))
            {
                query.Select($"{alias}.Name");
            }
            if (fields.ContainsKey("emails"))
            {
                var companyEmailAlias = $"{alias}CompanyEmail";
                var emailAlias = $"{alias}Email";
                query
                    .LeftJoin($"CompanyEmail {companyEmailAlias} ON {alias}.Id = {companyEmailAlias}.PersonId")
                    .LeftJoin($"Email {emailAlias} ON {companyEmailAlias}.EmailId = {emailAlias}.Id");
                query = emailQueryBuilder.Build(query, fields["emails"], emailAlias);
            }
            if (fields.ContainsKey("phones"))
            {
                var companyPhoneAlias = $"{alias}CompanyPhone";
                var phoneAlias = $"{alias}Phone";
                query
                    .LeftJoin($"CompanyPhone {companyPhoneAlias} ON {alias}.Id = {companyPhoneAlias}.PersonId")
                    .LeftJoin($"Phone {phoneAlias} ON {companyPhoneAlias}.PhoneId = {phoneAlias}.Id");
                query = phoneQueryBuilder.Build(query, fields["phones"], phoneAlias);
            }

            return query;
        }
    }
}