using System.Threading.Tasks;
using Dapper.GraphQL.Test.GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class GraphQLInsertTests
        : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GraphQLInsertTests(
            TestFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact(DisplayName = "Simple person insert should succeed")]
        public async Task SimplePersonInsert()
        {
            var graphQuery = new GraphQlQuery();
            graphQuery.OperationName = "addPerson";
            graphQuery.Variables = JObject.Parse(@"{""person"":{""firstName"":""Joe"",""lastName"":""Doe""}}");

            graphQuery.Query = @"
mutation ($person: PersonInput!) {
  addPerson(person: $person) {
    firstName
    lastName
  }
}";

            var json = await _fixture.QueryGraphQlAsync(graphQuery);

            var expectedJson = @"
            {
                data: {
                    addPerson: {
                        firstName: 'Joe',
                        lastName: 'Doe'
                    }
                }
            }";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }
    }
}
