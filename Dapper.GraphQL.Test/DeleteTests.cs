using Dapper.GraphQL.Test.Models;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class DeleteTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public DeleteTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "DELETE query uses custom table name")]
        public void DeleteWithCustomTableName()
        {
            // Check generic Delete uses custom table name for Contact as configured in TestFixture
            var contact = new Contact();

            var query = SqlBuilder.Delete<Contact>(contact);
            Assert.Equal("DELETE FROM Contacts WHERE ", query.ToString());
        }
    }
}
