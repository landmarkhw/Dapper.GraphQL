using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class InsertTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public InsertTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "INSERT person succeeds", Skip = "Not implemented yet")]
        public void InsertPerson()
        {
            //var person = new Person
            //{
            //    FirstName = "Steven",
            //    LastName = "Rollman",
            //};

            //var personId = SqlBuilder
            //    .Insert(person)
            //    .Execute(dbConnection);

            //var email = new Email
            //{
            //    Address = "srollman@landmarkhw.com",
            //    PersonId = personId,
            //};

            //var phone = new Phone
            //{
            //    Number = "8011115555",
            //    Type = PhoneType.Mobile,
            //    PersonId = personId,
            //};

            //SqlBuilder
            //    .Insert(email)
            //    .Insert(phone)
            //    .Execute(dbConnection);
        }

        [Fact(DisplayName = "UPDATE person succeeds", Skip = "Not implemented yet")]
        public void UpdatePerson()
        {
            //var person = new Person
            //{
            //    FirstName = "Douglas"
            //};

            //SqlBuilder
            //    .Update(person)
            //    .Where("Id = @id")
            //    .WithParameter("id", 1)
            //    .Execute(dbConnection);
        }
    }
}