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

        [Fact(DisplayName = "INSERT person succeeds")]
        public void InsertPerson()
        {
            var person = new Person
            {
                FirstName = "Steven",
                LastName = "Rollman",
            };

            // Ensure inserting a person works and we get the person's Id back
            var personId = SqlBuilder
                .Insert(person)
                .ExecuteWithSqliteIdentity(fixture.DbConnection);

            Assert.True(personId > 0);

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

            // Add email and phone number to the person
            var insertedCount = SqlBuilder
                .Insert(email)
                .Insert(phone)
                .Execute(fixture.DbConnection);

            // Ensure both were inserted properly
            Assert.Equal(2, insertedCount);

            // Build an identity mapper for person
            var personMapper = fixture.BuildMapper<Person>(p => p.Id);

            // Query the person from the database
            var query = SqlBuilder
                .From<Person>("person")
                .LeftJoin("Email email on person.Id = email.PersonId")
                .LeftJoin("Phone phone on person.Id = phone.PersonId")
                .Select("person.*, email.*, phone.*")
                .SplitOn<Person>("Id")
                .SplitOn<Email>("Id")
                .SplitOn<Phone>("Id")
                .Where("person.Id = @id", new { id = personId });

            person = query
                .Execute(fixture.DbConnection, personMapper)
                .FirstOrDefault();

            // Ensure all inserted data is present
            Assert.NotNull(person);
            Assert.Equal(personId, person.Id);
            Assert.Equal("Steven", person.FirstName);
            Assert.Equal("Rollman", person.LastName);
            Assert.Equal(1, person.Emails.Count);
            Assert.Equal("srollman@landmarkhw.com", person.Emails[0].Address);
            Assert.Equal(personId, person.Emails[0].PersonId);
            Assert.Equal(1, person.Phones.Count);
            Assert.Equal("8011115555", person.Phones[0].Number);
            Assert.Equal(personId, person.Phones[0].PersonId);
        }
    }
}