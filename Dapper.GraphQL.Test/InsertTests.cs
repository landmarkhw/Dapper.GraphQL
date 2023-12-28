using System.Linq;
using System.Threading.Tasks;
using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class InsertTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InsertTests(TestFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact(DisplayName = "INSERT person succeeds")]
        public void InsertPerson()
        {
            Person person = null;

            // Ensure inserting a person works and we get IDs back
            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    // Get the next identity aggressively, as we need to assign
                    // it to both Id/MergedToPersonId
                    personId = PostgreSql.NextIdentity(db, (Person p) => p.Id);
                    Assert.True(personId > 0);

                    person = new Person
                    {
                        Id = personId,
                        FirstName = "Steven",
                        LastName = "Rollman",
                        MergedToPersonId = personId,
                    };

                    int insertedCount;
                    insertedCount = SqlBuilder
                        .Insert(person)
                        .Execute(db);
                    Assert.Equal(1, insertedCount);

                    emailId = PostgreSql.NextIdentity(db, (Email e) => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = "srollman@landmarkhw.com",
                    };

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = PostgreSql.NextIdentity(db, (Phone p) => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = "8011115555",
                        Type = PhoneType.Mobile,
                    };

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    // Add email and phone number to the person
                    insertedCount = SqlBuilder
                        .Insert(email)
                        .Insert(phone)
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .Execute(db);

                    // Ensure all were inserted properly
                    Assert.Equal(4, insertedCount);

                    // Build an entity mapper for person
                    var personMapper = new PersonEntityMapper();

                    // Query the person from the database
                    var query = SqlBuilder
                        .From<Person>("person")
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = @id", new { id = personId });

                    var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";
                    var selection = _fixture.BuildGraphQlSelection(graphql);
                    person = query
                        .Execute(db, selection, personMapper)
                        .Single();
                }

                // Ensure all inserted data is present
                Assert.NotNull(person);
                Assert.Equal(personId, person.Id);
                Assert.Equal("Steven", person.FirstName);
                Assert.Equal("Rollman", person.LastName);
                Assert.Single(person.Emails);
                Assert.Equal("srollman@landmarkhw.com", person.Emails[0].Address);
                Assert.Single(person.Phones);
                Assert.Equal("8011115555", person.Phones[0].Number);
            }
            finally
            {
                // Ensure the changes here don't affect other unit tests
                using (var db = _fixture.GetDbConnection())
                {
                    if (emailId != default(int))
                    {
                        SqlBuilder
                            .Delete("PersonEmail", new { EmailId = emailId })
                            .Delete("Email", new { Id = emailId })
                            .Execute(db);
                    }

                    if (phoneId != default(int))
                    {
                        SqlBuilder
                            .Delete("PersonPhone", new { PhoneId = phoneId })
                            .Delete("Phone", new { Id = phoneId })
                            .Execute(db);
                    }

                    if (personId != default(int))
                    {
                        SqlBuilder
                            .Delete<Person>(new { Id = personId })
                            .Execute(db);
                    }
                }
            }
        }

        [Fact(DisplayName = "INSERT person asynchronously succeeds")]
        public async Task InsertPersonAsync()
        {
            Person person = null;

            // Ensure inserting a person works and we get IDs back
            var emailId = -1;
            var personId = -1;
            var phoneId = -1;

            try
            {
                using (var db = _fixture.GetDbConnection())
                {
                    db.Open();

                    // Get the next identity aggressively, as we need to assign
                    // it to both Id/MergedToPersonId
                    personId = PostgreSql.NextIdentity(db, (Person p) => p.Id);
                    Assert.True(personId > 0);

                    person = new Person
                    {
                        Id = personId,
                        FirstName = "Steven",
                        LastName = "Rollman",
                        MergedToPersonId = personId,
                    };

                    int insertedCount;
                    insertedCount = await SqlBuilder
                        .Insert(person)
                        .ExecuteAsync(db);
                    Assert.Equal(1, insertedCount);

                    emailId = PostgreSql.NextIdentity(db, (Email e) => e.Id);
                    var email = new Email
                    {
                        Id = emailId,
                        Address = "srollman@landmarkhw.com",
                    };

                    var personEmail = new
                    {
                        PersonId = personId,
                        EmailId = emailId,
                    };

                    phoneId = PostgreSql.NextIdentity(db, (Phone p) => p.Id);
                    var phone = new Phone
                    {
                        Id = phoneId,
                        Number = "8011115555",
                        Type = PhoneType.Mobile,
                    };

                    var personPhone = new
                    {
                        PersonId = personId,
                        PhoneId = phoneId,
                    };

                    // Add email and phone number to the person
                    insertedCount = await SqlBuilder
                        .Insert(email)
                        .Insert(phone)
                        .Insert("PersonEmail", personEmail)
                        .Insert("PersonPhone", personPhone)
                        .ExecuteAsync(db);

                    // Ensure all were inserted properly
                    Assert.Equal(4, insertedCount);

                    // Build an entity mapper for person
                    var personMapper = new PersonEntityMapper();

                    // Query the person from the database
                    var query = SqlBuilder
                        .From<Person>("person")
                        .LeftJoin("PersonEmail personEmail on person.Id = personEmail.Id")
                        .LeftJoin("Email email on personEmail.EmailId = email.Id")
                        .LeftJoin("PersonPhone personPhone on person.Id = personPhone.PersonId")
                        .LeftJoin("Phone phone on personPhone.PhoneId = phone.Id")
                        .Select("person.*, email.*, phone.*")
                        .SplitOn<Person>("Id")
                        .SplitOn<Email>("Id")
                        .SplitOn<Phone>("Id")
                        .Where("person.Id = @id", new { id = personId });

                    var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";
                    var selection = _fixture.BuildGraphQlSelection(graphql);

                    var people = await query.ExecuteAsync(db, selection, personMapper);
                    person = people
                        .FirstOrDefault();
                }

                // Ensure all inserted data is present
                Assert.NotNull(person);
                Assert.Equal(personId, person.Id);
                Assert.Equal("Steven", person.FirstName);
                Assert.Equal("Rollman", person.LastName);
                Assert.Single(person.Emails);
                Assert.Equal("srollman@landmarkhw.com", person.Emails[0].Address);
                Assert.Single(person.Phones);
                Assert.Equal("8011115555", person.Phones[0].Number);
            }
            finally
            {
                // Ensure the changes here don't affect other unit tests
                using (var db = _fixture.GetDbConnection())
                {
                    if (emailId != default(int))
                    {
                        await SqlBuilder
                            .Delete("PersonEmail", new { EmailId = emailId })
                            .Delete("Email", new { Id = emailId })
                            .ExecuteAsync(db);
                    }

                    if (phoneId != default(int))
                    {
                        await SqlBuilder
                            .Delete("PersonPhone", new { PhoneId = phoneId })
                            .Delete("Phone", new { Id = phoneId })
                            .ExecuteAsync(db);
                    }

                    if (personId != default(int))
                    {
                        await SqlBuilder
                            .Delete<Person>(new { Id = personId })
                            .ExecuteAsync(db);
                    }
                }
            }
        }
    }
}
