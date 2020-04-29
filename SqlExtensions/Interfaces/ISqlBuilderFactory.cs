using Dapper.Framework.SqlExtensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Dapper.Framework
{
    public interface ISqlBuilder<T> where T: IEntity,new()
    {
        Query<T> CreateQueryService();
        Renewal<T> CreateRenewalService();
        IExecuteBatch<T> CreateDapper();
    }
    public interface ISqlBuilderFactory<T> where T : IEntity,new()
    {
        ISqlBuilder<T> CreateBuilder();
    }
    public class SqlSererBuilder<T> : ISqlBuilder<T> where T:IEntity,new()
    {
        private IConnectionCreate Connection;
        public SqlSererBuilder(IConnectionCreate conn)
        {
            Connection = conn;
        }
        public IExecuteBatch<T> CreateDapper()
        {
            return new DapperExecuteBatch<T>(Connection);
        }

        public Query<T> CreateQueryService()
        {
            return new SqlServerQuery<T>();
        }

        public Renewal<T> CreateRenewalService()
        {
            return new SqlServerRenewal<T>();
        }
    }

    public interface IConnectionCreate
    {
        IDbConnection CreateConnection();
    }

    public class MysqlBuilder<T> : ISqlBuilder<T> where T : IEntity, new()
    {
        private IConnectionCreate Connection;
        public MysqlBuilder(IConnectionCreate conn)
        {
            Connection = conn;
        }
        public IExecuteBatch<T> CreateDapper()
        {
            return new DapperExecuteBatch<T>(Connection);
        }

        public Query<T> CreateQueryService()
        {
            return new MysqlQuery<T>();
        }

        public Renewal<T> CreateRenewalService()
        {
            return new MysqlRenewal<T>();
        }
    }
    public class SqlBuilderFactory<T> : ISqlBuilderFactory<T> where T : IEntity, new()
    {
        private IConnectionCreate m_connection;
        private static String m_type;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlEnum">mysql种类</param>
        /// <param name="connString">字符串连接</param>
        public SqlBuilderFactory(IConnectionCreate connection)
        {
            m_connection = connection;
            if(m_type == null)
            {
                using (var i = connection.CreateConnection())
                {
                    m_type = i.GetType().Name;
                }
            }
        }
        /// <summary>
        /// 创建SqlBuilder
        /// </summary>
        /// <returns></returns>
        public ISqlBuilder<T> CreateBuilder()
        {
            switch (m_type)
            {
                case "SqlConnection":
                    return new SqlSererBuilder<T>(m_connection);
                case "MySqlConnection":
                    return new MysqlBuilder<T>(m_connection);
                default: return new SqlSererBuilder<T>(m_connection);
            }
        }
    }
}
