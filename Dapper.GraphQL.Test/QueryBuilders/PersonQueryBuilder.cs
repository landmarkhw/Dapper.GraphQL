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
            if (fields.ContainsKey("companies"))
            {
                var personCompanies = $"{alias}PersonCompany";
                var companyAlias = $"{alias}Company";
                query
                    .LeftJoin($"PersonCompany {personCompanies} ON {alias}.Id = {personCompanies}.PersonId")
                    .LeftJoin($"Company {companyAlias} ON {personCompanies}.CompanyId = {companyAlias}.Id");
                query = emailQueryBuilder.Build(query, fields["companies"], companyAlias);
            }
            if (fields.ContainsKey("emails"))
            {
                var personEmailAlias = $"{alias}PersonEmail";
                var emailAlias = $"{alias}Email";
                query
                    .LeftJoin($"PersonEmail {personEmailAlias} ON {alias}.Id = {personEmailAlias}.PersonId")
                    .LeftJoin($"Email {emailAlias} ON {personEmailAlias}.EmailId = {emailAlias}.Id");
                query = emailQueryBuilder.Build(query, fields["emails"], emailAlias);
            }
            if (fields.ContainsKey("phones"))
            {
                var personPhoneAlias = $"{alias}PersonPhone";
                var phoneAlias = $"{alias}Phone";
                query
                    .LeftJoin($"PersonPhone {personPhoneAlias} ON {alias}.Id = {personPhoneAlias}.PersonId")
                    .LeftJoin($"Phone {phoneAlias} ON {personPhoneAlias}.PhoneId = {phoneAlias}.Id");
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