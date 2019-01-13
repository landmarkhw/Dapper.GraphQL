using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Builders;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.EntityMappers;
using Microsoft.Extensions.DependencyInjection;


namespace Dapper.GraphQL.Test.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IQueryBuilder<Person> _personQueryBuilder;
        private PersonEntityMapper personMapper = new PersonEntityMapper();
        private string pAlias = "Person";
        private IServiceProvider _serviceProvider;

        public PersonRepository(IQueryBuilder<Person> personQueryBuilder, IServiceProvider serviceProvider)
        {
            _personQueryBuilder = personQueryBuilder;
            _serviceProvider = serviceProvider;
        }

        public ResolveConnectionContext<object> Context { get; set; }

        public Task<int> GetTotalCount(CancellationToken cancellationToken)
        {
            var query = this.GetQuery(Context, _personQueryBuilder);

            try
            {
                using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
                {
                    var results = query
                        .Execute(connection, Context.FieldAst, personMapper)
                        .Distinct()
                        .Count();

                    return Task.FromResult(results);
                }
            }
            catch (Exception ex)
            {
                // log
            }

            return Task.FromResult(0);
        }

        public Task<List<Person>> GetPeople(int? first,
                                                    DateTime? createdAfter,
                                                    CancellationToken cancellationToken)
        {
            string sWhere = createdAfter != null ? ($"{pAlias}.CreateDate > '{createdAfter}'") : "";
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            try
            {
                using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
                {
                    List<Person> results = query
                        .Execute(connection, Context.FieldAst, personMapper)
                        .Distinct()
                        .If(first.HasValue, x => x.Take(first.Value))
                        .ToList();

                    return Task.FromResult(results);
                }
            }
            catch (Exception ex)
            {
                // log
            }

            return Task.FromResult<List<Person>>(null);
        }

        public Task<List<Person>> GetPeopleReversed(int? last,
                                                    DateTime? createdBefore,
                                                    CancellationToken cancellationToken)
        {
            string sWhere = createdBefore != null ? ($"{pAlias}.CreateDate < @{createdBefore}") : "";
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            try
            {
                using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
                {
                    List<Person> results = query
                        .Execute(connection, Context.FieldAst, personMapper)
                        .Distinct()
                        .If(last.HasValue, x => x.TakeLast(last.Value))
                        .ToList();

                    return Task.FromResult(results);
                }
            }
            catch (Exception ex)
            {
                // log
            }

            return Task.FromResult<List<Person>>(null);
        }


        public Task<bool> GetHasNextPage(int? first,
                                        DateTime? createdAfter,
                                        CancellationToken cancellationToken)
        {
            string sWhere = createdAfter != null ? ($"{pAlias}.CreateDate > '{createdAfter}'") : "";
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            try
            {
                using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
                {
                    return Task.FromResult(query
                        .Execute(connection, Context.FieldAst, personMapper)
                        .Distinct()
                        .Skip(first.Value)
                        .Any());
                }
            }
            catch (Exception ex)
            {
                // log
            }

            return Task.FromResult(false);
        }

        public Task<bool> GetHasPreviousPage(int? last,
                                        DateTime? createdBefore,
                                        CancellationToken cancellationToken)
        {
            string sWhere = createdBefore != null ? ($"{pAlias}.CreateDate < '{createdBefore}'") : "";
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            try
            {
                using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
                {
                    return Task.FromResult(query
                        .Execute(connection, Context.FieldAst, personMapper)
                        .Distinct()
                        .SkipLast(last.Value)
                        .Any());
                }
            }
            catch (Exception ex)
            {
                // log
            }

            return Task.FromResult(false);
        }
    }

    public interface IPersonRepository
    {
        ResolveConnectionContext<object> Context { get; set; }
        Task<int> GetTotalCount(CancellationToken cancellationToken);
        Task<List<Person>> GetPeople(int? first, DateTime? createdAfter, CancellationToken cancellationToken);
        Task<List<Person>> GetPeopleReversed(int? last, DateTime? createdBefore, CancellationToken cancellationToken);
        Task<bool> GetHasNextPage(int? first, DateTime? createdAfter, CancellationToken cancellationToken);
        Task<bool> GetHasPreviousPage(int? last, DateTime? createdBefore, CancellationToken cancellationToken);
    }

    public static class IPersonRepositoryExtensions
    {
        public static SqlQueryContext GetQuery(this IPersonRepository personRepository,
                                        ResolveConnectionContext<object> context,
                                        IQueryBuilder<Person> personQueryBuilder,
                                        string sWhere = "")
        {
            var alias = "Person";

            var query = SqlBuilder
                       .From<Person>(alias)
                       .OrderBy($"{alias}.CreateDate");

            query = !string.IsNullOrEmpty(sWhere) ? query.Where(sWhere) : query;

            return personQueryBuilder.Build(query, context.FieldAst, alias);
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> If<T>(this IEnumerable<T> enumerable, bool condition, Func<IEnumerable<T>, IEnumerable<T>> action)
        {
            if (condition)
            {
                return action(enumerable);
            }

            return enumerable;
        }
    }
}
