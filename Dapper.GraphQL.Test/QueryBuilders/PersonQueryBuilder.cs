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
        private readonly IQueryBuilder<Company> companyQueryBuilder;
        private readonly IQueryBuilder<Email> emailQueryBuilder;
        private readonly IQueryBuilder<Phone> phoneQueryBuilder;

        public PersonQueryBuilder(
            IQueryBuilder<Company> companyQueryBuilder,
            IQueryBuilder<Email> emailQueryBuilder,
            IQueryBuilder<Phone> phoneQueryBuilder)
        {
            this.companyQueryBuilder = companyQueryBuilder;
            this.emailQueryBuilder = emailQueryBuilder;
            this.phoneQueryBuilder = phoneQueryBuilder;
        }

        public SqlQueryContext Build(SqlQueryContext query, IHaveSelectionSet context, string alias)
        {
            var mergedAlias = $"{alias}Merged";

            // Deduplicate the person
            query.LeftJoin($"Person {mergedAlias} ON {alias}.MergedToPersonId = {mergedAlias}.MergedToPersonId");
            query.Select($"{alias}.Id", $"{alias}.MergedToPersonId");
            query.SplitOn<Person>("Id");

            var fields = context.GetSelectedFields();

            if (fields.ContainsKey("firstName"))
            {
                query.Select($"{mergedAlias}.FirstName");
            }
            if (fields.ContainsKey("lastName"))
            {
                query.Select($"{mergedAlias}.LastName");
            }
            if (fields.ContainsKey("companies"))
            {
                var personCompanyAlias = $"{alias}PersonCompany";
                var companyAlias = $"{alias}Company";
                query
                    .LeftJoin($"PersonCompany {personCompanyAlias} ON {mergedAlias}.Id = {personCompanyAlias}.PersonId")
                    .LeftJoin($"Company {companyAlias} ON {personCompanyAlias}.CompanyId = {companyAlias}.Id");
                query = companyQueryBuilder.Build(query, fields["companies"], companyAlias);
            }
            if (fields.ContainsKey("emails"))
            {
                var personEmailAlias = $"{alias}PersonEmail";
                var emailAlias = $"{alias}Email";
                query
                    .LeftJoin($"PersonEmail {personEmailAlias} ON {mergedAlias}.Id = {personEmailAlias}.PersonId")
                    .LeftJoin($"Email {emailAlias} ON {personEmailAlias}.EmailId = {emailAlias}.Id");
                query = emailQueryBuilder.Build(query, fields["emails"], emailAlias);
            }
            if (fields.ContainsKey("phones"))
            {
                var personPhoneAlias = $"{alias}PersonPhone";
                var phoneAlias = $"{alias}Phone";
                query
                    .LeftJoin($"PersonPhone {personPhoneAlias} ON {mergedAlias}.Id = {personPhoneAlias}.PersonId")
                    .LeftJoin($"Phone {phoneAlias} ON {personPhoneAlias}.PhoneId = {phoneAlias}.Id");
                query = phoneQueryBuilder.Build(query, fields["phones"], phoneAlias);
            }
            if (fields.ContainsKey("supervisor"))
            {
                var supervisorAlias = $"{alias}Supervisor";
                query.LeftJoin($"Person {supervisorAlias} ON {mergedAlias}.SupervisorId = {supervisorAlias}.Id");
                query = Build(query, fields["supervisor"], supervisorAlias);
            }
            if (fields.ContainsKey("careerCounselor"))
            {
                var careerCounselorAlias = $"{alias}CareerCounselor";
                query.LeftJoin($"Person {careerCounselorAlias} ON {mergedAlias}.CareerCounselorId = {careerCounselorAlias}.Id");
                query = Build(query, fields["careerCounselor"], careerCounselorAlias);
            }

            return query;
        }
    }
}