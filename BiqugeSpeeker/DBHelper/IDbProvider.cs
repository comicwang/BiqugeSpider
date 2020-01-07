using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoearth.Solution.MultiPointTile.DBHelper
{
    public interface IDbProvider
    {
        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        int ExecuteNonQuery(string commandText);

        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <typeparam name="T">返回对象类型</typeparam>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        T ExecuteScalar<T>(string commandText);

        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        DataSet ExecuteDataSet(string commandText);

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        DataSet PageQuery(string tableName, int pageIndex, int pageSize);
    }
}
