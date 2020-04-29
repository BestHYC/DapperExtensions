using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Framework
{

    public interface IExecuteBatch<T> where T : IEntity, new()
    {
        IConnectionCreate Create { get; }
        Int32 Execute(IBatch batch, Boolean transaction = false);
        Int32 Execute(IDbConnection connection, IBatch batch, IDbTransaction transaction);
        Task<Int32> ExecuteAsync(IBatch batch, Boolean transaction = false);
        T Query(IBatch batch);
        T Query(String sql, DynamicParameters parameters);
        K Query<K>(IBatch batch);
        K Query<K>(String sql, DynamicParameters parameters);
        IEnumerable<T> QueryList(IBatch batch);
        IEnumerable<T> ExecuteReader(IBatch batch);
        IEnumerable<Tuple<T, K>> ExecuteReader<K>(IBatch batch, List<ColumnRelevanceMapper> mapper) where K : IEntity, new();
        IEnumerable<Tuple<T, K, P>> ExecuteReader<K, P>(IBatch batch, List<ColumnRelevanceMapper> mapper) where K : IEntity, new() where P : IEntity, new();
    }

    public class DapperExecuteBatch<T> : IExecuteBatch<T> where T : IEntity, new()
    {
        public IConnectionCreate Create { get; }
        public DapperExecuteBatch(IConnectionCreate create)
        {
            Create = create;
        }
        public Int32 Execute(String sql, DynamicParameters parameters, Boolean transaction=false)
        {
            Int32 result = 0;
            using (var Connection = Create.Create())
            {
                IDbTransaction dbTransaction = null;
                if (transaction)
                {
                    Connection.Open();
                    dbTransaction = Connection.BeginTransaction();
                }
                try
                {
                    result = Connection.Execute(sql, parameters, dbTransaction, commandTimeout: 3600);
                    if (transaction)
                    {
                        dbTransaction.Commit();
                    }
                }
                catch (Exception)
                {
                    if (transaction)
                    {
                        dbTransaction.Rollback();
                    }
                    return result;
                }
                finally
                {
                    if (transaction)
                    {
                        Connection.Close();
                    }
                }
            }
            return result;
        }
        public Int32 Execute(IBatch batch, Boolean transaction = false)
        {
            Int32 result = 0;
            using (var Connection = Create.Create())
            {
                IDbTransaction dbTransaction = null;
                if (transaction)
                {
                    Connection.Open();
                    dbTransaction = Connection.BeginTransaction();
                }
                try
                {
                    result = Connection.Execute(batch.SqlBuilder, batch.DynamicParameters, dbTransaction, commandTimeout: 3600);
                    if (transaction)
                    {
                        dbTransaction.Commit();
                    }
                }
                catch (Exception)
                {
                    if (transaction)
                    {
                        dbTransaction.Rollback();
                    }
                    return result;
                }
                finally
                {
                    if (transaction)
                    {
                        Connection.Close();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 这个方法不对connction进行开启或者释放,注意,使用在外部使用using语句,若使用transation请手动open
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="batch"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Int32 Execute(IDbConnection connection, IBatch batch, IDbTransaction transaction)
        {
            return connection.Execute(batch.SqlBuilder, batch.DynamicParameters, transaction, commandTimeout:3600);
        }
        public async Task<Int32> ExecuteAsync(IBatch batch, Boolean transaction = false)
        {
            Int32 result = 0;
            using (var Connection = Create.Create())
            {
                IDbTransaction dbTransaction = null;
                if (transaction)
                {
                    Connection.Open();
                    dbTransaction = Connection.BeginTransaction();
                }
                try
                {
                    result = await Connection.ExecuteAsync(batch.SqlBuilder, batch.DynamicParameters, dbTransaction, commandTimeout: 3600);
                    if (transaction)
                    {
                        dbTransaction.Commit();
                    }
                }
                catch (Exception)
                {
                    if (transaction)
                    {
                        dbTransaction.Rollback();
                    }
                    return result;
                }
                finally
                {
                    if (transaction)
                    {
                        Connection.Close();
                    }
                }
            }
            return result;
        }
        public T Query(IBatch batch)
        {
            using (var Connection = Create.Create())
            {
                IEnumerable<T> list = Connection.Query<T>(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
                if(list != null)
                {
                    return list.FirstOrDefault();
                }
                return default(T);
            }
        }
        public K Query<K>(IBatch batch)
        {
            using (var Connection = Create.Create())
            {
                IEnumerable<K> list = Connection.Query<K>(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
                if (list != null)
                {
                    return list.FirstOrDefault();
                }
                return default(K);
            }
        }
        public K Query<K>(String sql, DynamicParameters parameters)
        {
            using (var Connection = Create.Create())
            {
                IEnumerable<K> list = Connection.Query<K>(sql, parameters, commandTimeout: 3600);
                if (list != null)
                {
                    return list.FirstOrDefault();
                }
                return default(K);
            }
        }
        public T Query(String sql, DynamicParameters parameters)
        {
            using (var Connection = Create.Create())
            {
                IEnumerable<T> list = Connection.Query<T>(sql, parameters, commandTimeout: 3600);
                if (list != null)
                {
                    return list.FirstOrDefault();
                }
                return default(T);
            }
        }
        public IEnumerable<T> QueryList(IBatch batch)
        {
            using (var Connection = Create.Create())
            {
                return Connection.Query<T>(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
            }
        }
        public IEnumerable<T> ExecuteReader(IBatch batch)
        {
            List<T> entitybuffer = new List<T>(1024);
            using (var Connection = Create.Create())
            {
                var reader = Connection.ExecuteReader(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
                var entityParser = reader.GetRowParser<T>();
                while (reader.Read())
                {
                    var entity = entityParser.Invoke(reader);
                    if (entity == null) break;
                    entitybuffer.Add(entity);
                }
            }
            return entitybuffer;
        }
        public IEnumerable<Tuple<T, K>> ExecuteReader<K>(IBatch batch, List<ColumnRelevanceMapper> mapper) where K : IEntity, new()
        {
            List<Tuple<T, K>> entitybuffer = new List<Tuple<T, K>>(128);
            using (var Connection = Create.Create())
            {
                var reader = Connection.ExecuteReader(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
                while (reader.Read())
                {
                    T t = new T();
                    Type t1 = typeof(T);
                    K k = new K();
                    Type k1 = typeof(K);
                    for (Int32 i = 0; i < reader.FieldCount; i++)
                    {
                        Object obj = reader.GetValue(i);
                        if (obj != null)
                        {
                            ColumnRelevanceMapper column = mapper[i];
                            if (column.TableName == t1)
                            {
                                PropertyValueExpression<T>.SetValue(t, column.ColumnName, obj);
                            }
                            else
                            {
                                PropertyValueExpression<K>.SetValue(k, column.ColumnName, obj);
                            }
                        }
                    }
                    Tuple<T, K> tuple = new Tuple<T, K>(t, k);
                    entitybuffer.Add(tuple);
                }
            }
            return entitybuffer;
        }

        public IEnumerable<Tuple<T, K, P>> ExecuteReader<K, P>(IBatch batch, List<ColumnRelevanceMapper> mapper) where K : IEntity, new() where P : IEntity, new()
        {
            List<Tuple<T, K, P>> entitybuffer = new List<Tuple<T, K, P>>(128);
            using (var Connection = Create.Create())
            {
                var reader = Connection.ExecuteReader(batch.SqlBuilder, batch.DynamicParameters, commandTimeout: 3600);
                while (reader.Read())
                {
                    T t = new T();
                    Type t1 = typeof(T);
                    K k = new K();
                    Type k1 = typeof(K);
                    P p = new P();
                    Type p1 = typeof(P);
                    for (Int32 i = 0; i < reader.FieldCount; i++)
                    {
                        Object obj = reader.GetValue(i);
                        if (obj != null)
                        {
                            ColumnRelevanceMapper column = mapper[i];
                            if (column.TableName == t1)
                            {
                                PropertyValueExpression<T>.SetValue(t, column.ColumnName, obj);
                            }
                            else if (column.TableName == k1)
                            {
                                PropertyValueExpression<K>.SetValue(k, column.ColumnName, obj);
                            }
                            else
                            {
                                PropertyValueExpression<P>.SetValue(p, column.ColumnName, obj);
                            }
                        }
                    }
                    Tuple<T, K, P> tuple = new Tuple<T, K, P>(t, k, p);
                    entitybuffer.Add(tuple);
                }
            }
            return entitybuffer;
        }
    }
}
