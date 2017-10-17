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

        [Fact]
        public void InsertPerson()
        {
            var person = new Person
            {
                FirstName = "Steven",
                LastName = "Rollman",
            };

            var statement = new SqlBuilder();
            var personId = statement
                .Insert(person)
                .Execute(dbConnection);

            var email = new Email
            {
                Address = "srollman@landmarkhw.com",
                PersonId = personId,
            };

            var phone = new Phone
            {
                Number = "8011115555",
                Type = PhoneType.Mobile,
                PersonId = personId,
            };

            new SqlBuilder()
                .Insert(email)
                .Insert(phone)
                .Execute(dbConnection);
        }

        [Fact]
        public void UpdatePerson()
        {
            var person = new Person
            {
                FirstName = "Douglas"
            };

            var statement = new SqlBuilder();
            statement
                .Update(person)
                .Where("Id = @id")
                .WithParameter("id", 1)
                .Execute(dbConnection);
        }
    }
}