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
        companies {
            id
            name
        }
        supervisor {
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
        careerCounselor {
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
    }
}");

            var expectedJson = @"
{
    ""data"": {
        ""people"": [{
                ""id"": 1,
                ""firstName"": ""Hyrum"",
                ""lastName"": ""Clyde"",
                ""emails"": [{
                    ""id"": 1,
                    ""address"": ""hclyde@landmarkhw.com""
                }],
                ""phones"": [],
                ""companies"": [{
                    ""id"": 1,
                    ""name"": ""Landmark Home Warranty, LLC""
                }],
                ""supervisor"": null,
                ""careerCounselor"": null
            },
            {
                ""id"": 2,
                ""firstName"": ""Doug"",
                ""lastName"": ""Day"",
                ""emails"": [{
                        ""id"": 2,
                        ""address"": ""dday@landmarkhw.com""
                    },
                    {
                        ""id"": 3,
                        ""address"": ""dougrday@gmail.com""
                    }
                ],
                ""phones"": [{
                    ""id"": 1,
                    ""number"": ""8011234567"",
                    ""type"": 3
                }],
                ""companies"": [{
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
                    },
                    {
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
                    }
                ],
                ""supervisor"": null,
                ""careerCounselor"": {
                    ""id"": 1,
                    ""firstName"": ""Hyrum"",
                    ""lastName"": ""Clyde"",
                    ""emails"": [{
                        ""id"": 1,
                        ""address"": ""hclyde@landmarkhw.com""
                    }],
                    ""phones"": []
                }
            },
            {
                ""id"": 3,
                ""firstName"": ""Kevin"",
                ""lastName"": ""Russon"",
                ""emails"": [{
                    ""id"": 4,
                    ""address"": ""krusson@landmarkhw.com""
                }],
                ""phones"": [{
                        ""id"": 2,
                        ""number"": ""8019876543"",
                        ""type"": 3
                    },
                    {
                        ""id"": 3,
                        ""number"": ""8011111111"",
                        ""type"": 1
                    }
                ],
                ""companies"": [{
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
                    },
                    {
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
                    }
                ],
                ""supervisor"": {
                    ""id"": 1,
                    ""firstName"": ""Hyrum"",
                    ""lastName"": ""Clyde"",
                    ""emails"": [{
                        ""id"": 1,
                        ""address"": ""hclyde@landmarkhw.com""
                    }],
                    ""phones"": []
                },
                ""careerCounselor"": {
                    ""id"": 2,
                    ""firstName"": ""Doug"",
                    ""lastName"": ""Day"",
                    ""emails"": [{
                            ""id"": 2,
                            ""address"": ""dday@landmarkhw.com""
                        },
                        {
                            ""id"": 3,
                            ""address"": ""dougrday@gmail.com""
                        }
                    ],
                    ""phones"": [{
                        ""id"": 1,
                        ""number"": ""8011234567"",
                        ""type"": 3
                    }]
                }
            }
        ]
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Async query should succeed")]
        public async Task PeopleAsyncQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    peopleAsync {
        id
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
  ""data"": {
    ""peopleAsync"": [
      {
        ""id"": 1,
        ""firstName"": ""Hyrum"",
        ""lastName"": ""Clyde""
      },
      {
        ""id"": 2,
        ""firstName"": ""Doug"",
        ""lastName"": ""Day""
      },
      {
        ""id"": 3,
        ""firstName"": ""Kevin"",
        ""lastName"": ""Russon""
      }
    ]
  }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Person query should succeed")]
        public async Task PersonQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    person (id: 2) {
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
            id: 2,
            firstName: 'Doug',
            lastName: 'Day',
            emails: [{
                id: 2,
                address: 'dday@landmarkhw.com'
            }, {
                id: 3,
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
    people: [
      {
        firstName: 'Hyrum',
        lastName: 'Clyde'
      },
      {
        firstName: 'Doug',
        lastName: 'Day'
      },
      {
        firstName: 'Kevin',
        lastName: 'Russon'
      }
    ]
  }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Simple person query should succeed")]
        public async Task SimplePersonQuery()
        {
            var json = await fixture.QueryGraphQLAsync(@"
query {
    person (id: 2) {
        id
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
    data: {
        person: {
            id: 2,
            firstName: 'Douglas',
            lastName: 'Day'
        }
    }
}";

            Assert.True(fixture.JsonEquals(expectedJson, json));
        }
    }
}