//using System.Data;
//using DatabaseGpt.Abstractions;
//using DatabaseGpt.DataAccessLayer.TypeHandlers;

//namespace DatabaseGpt.DataAccessLayer;

//internal class SqlContext : ISqlContext
//{
//    private bool disposedValue;

//    private IDbConnection connection;
//    private readonly IDatabaseGptProvider provider;

//    private IDbConnection Connection
//    {
//        get
//        {
//            if (connection.State == ConnectionState.Closed)
//            {
//                connection.Open();
//            }

//            return connection;
//        }
//    }

//    static SqlContext()
//    {
//    }

//    public SqlContext(IDatabaseGptProvider provider, SqlContextOptions options)
//    {
//        this.provider = provider;
//        connection = provider.CreateConnection();
//    }

//    public Task<IDataReader> GetDataReaderAsync(string sql, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null)
//    {
//        ThrowIfDisposed();

//        return Connection.ExecuteReaderAsync(sql, param, transaction, commandType: commandType);
//    }

//    public Task<IEnumerable<T>> GetDataAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null)
//        where T : class
//    {
//        ThrowIfDisposed();

//        return Connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
//    }

//    public Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        return Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
//    }

//    public Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TThrid : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        return Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
//    }

//    public Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TThrid : class
//        where TFourth : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        return Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType);
//    }

//    public Task<T?> GetObjectAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null)
//        where T : class
//    {
//        ThrowIfDisposed();

//        return Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandType: commandType);
//    }

//    public async Task<TReturn?> GetObjectAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        var result = await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
//        return result.FirstOrDefault();
//    }

//    public async Task<TReturn?> GetObjectAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TThird : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        var result = await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
//        return result.FirstOrDefault();
//    }

//    public async Task<TReturn?> GetObjectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null, string splitOn = "Id")
//        where TFirst : class
//        where TSecond : class
//        where TThird : class
//        where TFourth : class
//        where TReturn : class
//    {
//        ThrowIfDisposed();

//        var result = await Connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
//        return result.FirstOrDefault();
//    }

//    public Task<T?> GetSingleValueAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null)
//    {
//        ThrowIfDisposed();

//        return Connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
//    }

//    public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CommandType? commandType = null)
//    {
//        ThrowIfDisposed();

//        return Connection.ExecuteAsync(sql, param, transaction, commandType: commandType);
//    }

//    public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
//    {
//        ThrowIfDisposed();

//        return Connection.BeginTransaction(isolationLevel);
//    }

//    protected virtual void Dispose(bool disposing)
//    {
//        if (!disposedValue)
//        {
//            if (disposing)
//            {
//                if (connection?.State == ConnectionState.Open)
//                {
//                    connection.Close();
//                }

//                connection?.Dispose();
//                connection = null!;
//            }

//            disposedValue = true;
//        }
//    }

//    public void Dispose()
//    {
//        Dispose(disposing: true);
//        GC.SuppressFinalize(this);
//    }

//    private void ThrowIfDisposed()
//    {
//        if (disposedValue)
//        {
//            throw new ObjectDisposedException(GetType().FullName);
//        }
//    }
//}
