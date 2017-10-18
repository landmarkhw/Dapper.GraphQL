using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class GraphQLTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public GraphQLTests(
            TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "Full people query should succeed")]
        public async Task FullPeopleQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    people {
        id
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}");

            var expectedJson = @"
{
    data: {
        people: [{
            id: 1,
            firstName: 'Doug',
            lastName: 'Day',
            emails: [{
                id: 1,
                address: 'dday@landmarkhw.com'
            }, {
                id: 2,
                address: 'dougrday@gmail.com'
            }],
            phones: [{
                id: 1,
                number: '8011234567',
                type: 3
            }]
        }, {
            id: 2,
            firstName: 'Kevin',
            lastName: 'Russon',
            emails: [{
                id: 3,
                address: 'krusson@landmarkhw.com'
            }],
            phones: [{
                id: 3,
                number: '8011111111',
                type: 1
            }, {
                id: 2,
                number: '8019876543',
                type: 3
            }]
        }]
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Full person query should succeed")]
        public async Task FullPersonQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    person (id: 1) {
        id
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}");

            var expectedJson = @"
{
    data: {
        person: {
            id: 1,
            firstName: 'Doug',
            lastName: 'Day',
            emails: [{
                id: 1,
                address: 'dday@landmarkhw.com'
            }, {
                id: 2,
                address: 'dougrday@gmail.com'
            }],
            phones: [{
                id: 1,
                number: '8011234567',
                type: 3
            }]
        }
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Simple people query should succeed")]
        public async Task SimplePeopleQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    people {
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
    data: {
        people: [{
            firstName: 'Doug',
            lastName: 'Day'
        }, {
            firstName: 'Kevin',
            lastName: 'Russon'
        }]
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Simple person query should succeed")]
        public async Task SimplePersonQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    person (id: 1) {
        id
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
    data: {
        person: {
            id: 1,
            firstName: 'Doug',
            lastName: 'Day'
        }
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }
    }
}