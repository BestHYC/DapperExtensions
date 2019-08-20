using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Dapper.Framework.SqlExtensions
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
    public enum SqlVarietyEnum
    {
        /// <summary>
        /// 
        /// </summary>
        SqlServer =0, 
        Mysql = 1,
    }
    public class SqlSererBuilder<T> : ISqlBuilder<T> where T:IEntity,new()
    {
        private String Connection;
        public SqlSererBuilder(String conn)
        {
            Connection = conn;
        }
        public IExecuteBatch<T> CreateDapper()
        {
            return new DapperExecuteBatch<T>(new SqlConnectionCreate(Connection));
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
        IDbConnection Create();
    }
    public class SqlConnectionCreate : IConnectionCreate
    {
        private String _connection;
        public SqlConnectionCreate(String conn)
        {
            _connection = conn;
        }
        public IDbConnection Create()
        {
            return new SqlConnection(_connection);
        }
    }
    public class MysqlConnectionCreate : IConnectionCreate
    {
        private String _connection;
        public MysqlConnectionCreate(String conn)
        {
            _connection = conn;
        }
        public IDbConnection Create()
        {
            return new MySqlConnection(_connection);
        }
    }

    public class MysqlBuilder<T> : ISqlBuilder<T> where T : IEntity, new()
    {
        private String Connection;
        public MysqlBuilder(String conn)
        {
            Connection = conn;
        }
        public IExecuteBatch<T> CreateDapper()
        {
            return new DapperExecuteBatch<T>(new MysqlConnectionCreate(Connection));
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
        private SqlVarietyEnum SqlVarietyEnum;
        private String Connection;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlEnum">mysql种类</param>
        /// <param name="connString">字符串连接</param>
        public SqlBuilderFactory(SqlVarietyEnum sqlEnum, String connString)
        {
            SqlVarietyEnum = sqlEnum;
            Connection = connString;
        }
        /// <summary>
        /// 创建SqlBuilder
        /// </summary>
        /// <returns></returns>
        public ISqlBuilder<T> CreateBuilder()
        {
            switch (SqlVarietyEnum)
            {
                case SqlVarietyEnum.Mysql:
                    return new MysqlBuilder<T>(Connection);
                case SqlVarietyEnum.SqlServer:
                    return new SqlSererBuilder<T>(Connection);
                default: return new SqlSererBuilder<T>(Connection);
            }
        }
    }
}
