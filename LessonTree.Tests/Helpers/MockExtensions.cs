using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LessonTree.Tests.Helpers
{
    /// <summary>
    /// Extension methods for creating mock DbSet objects for testing
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Creates a mock DbSet from an IQueryable for Entity Framework testing
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="source">Source queryable data</param>
        /// <returns>Mock DbSet that behaves like the source data</returns>
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
            
            // Support for async operations
            if (source is IAsyncEnumerable<T>)
            {
                mockSet.As<IAsyncEnumerable<T>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
                
                mockSet.As<IQueryable<T>>()
                    .Setup(m => m.Provider)
                    .Returns(new TestAsyncQueryProvider<T>(source.Provider));
            }
            
            return mockSet;
        }
        
        /// <summary>
        /// Creates a mock DbSet from a list for Entity Framework testing
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="source">Source list data</param>
        /// <returns>Mock DbSet that behaves like the source data</returns>
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IList<T> source) where T : class
        {
            return source.AsQueryable().BuildMockDbSet();
        }
    }

    /// <summary>
    /// Test async enumerator for mocking async Entity Framework operations
    /// </summary>
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }

    /// <summary>
    /// Test async query provider for mocking async Entity Framework operations
    /// </summary>
    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public ValueTask<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Execute<TResult>(expression));
        }

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
        {
            return Execute<TResult>(expression);
        }
    }

    /// <summary>
    /// Test async enumerable for mocking async Entity Framework operations
    /// </summary>
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }
}