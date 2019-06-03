using Oracle.ManagedDataAccess.Client;
using OracleSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleQuartz
{
    public interface IDbClient : IDisposable
    {
        void BeginTran();
        DataTable GetDataTable(string sqlStr, object _params = null);
        int ExecuteCommand(string sqlStr, object _params = null);
        void CommitTran();
        void RollbackTran();
        DateTime GetDbTime();
        string GetString(string sqlStr, object _params = null);
        int GetInt(string sqlStr, object _params = null);
        decimal GetDecimal(string sqlStr, object _params = null);




        void AddDisableInsertColumns(params string[] columns);
        void AddDisableUpdateColumns(params string[] columns);
        void AddMappingColumn(KeyValue mappingColumn);
        void AddMappingTable(KeyValue mappingTable);
        bool Delete<T, FiledType>(Expression<Func<T, object>> expression, params FiledType[] whereIn);
        bool Delete<T, FiledType>(params FiledType[] whereIn);
        bool Delete<T>(string SqlWhereString, object whereObj = null);
        bool Delete<T>(Expression<Func<T, bool>> expression);
        bool Delete<T>(List<T> deleteObjList);
        bool Delete<T>(T deleteObj);
        bool Delete<T, FiledType>(Expression<Func<T, object>> expression, List<FiledType> whereIn);
        bool FalseDelete<T, FiledType>(string field, params FiledType[] whereIn);
        bool FalseDelete<T>(string field, Expression<Func<T, bool>> expression);
        object Insert<T>(T entity, bool isIdentity = true) where T : class;
        object InsertOrUpdate<T>(T operationObj) where T : class;
        List<object> InsertRange<T>(List<T> entities, bool isIdentity = true) where T : class;
        Queryable<T> Queryable<T>() where T : new();
        Queryable<T> Queryable<T>(string tableName) where T : new();
        void RemoveAllCache<T>();
        void SetFilterItems(Dictionary<string, Func<KeyValueObj>> filterRows);
        void SetFilterItems(Dictionary<string, List<string>> filterColumns);
        void SetMappingColumns(List<KeyValue> mappingColumns);
        void SetMappingTables(List<KeyValue> mappingTables);
        void SetSerialNumber(List<PubModel.SerialNumber> serNum);
        bool SqlBulkCopy<T>(List<T> entities) where T : class;
        bool SqlBulkReplace<T>(List<T> entities) where T : class;
        List<T> SqlQuery<T>(string sql, object whereObj = null);
        List<T> SqlQuery<T>(string sql, List<OracleParameter> pars);
        List<T> SqlQuery<T>(string sql, OracleParameter[] pars);
        string SqlQueryJson(string sql, object whereObj = null);
        bool Update<T>(T rowObj) where T : class;
        bool Update<T>(object rowObj, Expression<Func<T, bool>> expression) where T : class;
        bool Update<T, FiledType>(object rowObj, params FiledType[] whereIn) where T : class;
        bool Update<T>(string setValues, Expression<Func<T, bool>> expression, object whereObj = null) where T : class;
        List<bool> UpdateRange<T>(List<T> rowObjList) where T : class;
    }
}
