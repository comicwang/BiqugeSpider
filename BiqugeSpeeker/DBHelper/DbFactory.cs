using Infoearth.Solution.MultiPointTile.DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Infoearth.Solution.MultiPointTile
{
    public class DbFactory
    {
        public static IDbProvider CreateDbProvider()
        {
            string DbType = ConfigurationManager.AppSettings["DBType"];
            if (DbType.ToLower() == "mysql")
                return new MySqlHelper();
            else if (DbType.ToLower() == "oracle")
                return new OracleHelper();
            else
                return new SqlHelper();
        }

        public static DBType GetDBType()
        {
            string DbType = ConfigurationManager.AppSettings["DBType"];
            if (DbType.ToLower() == "mysql")
                return DBType.MySql;
            else if (DbType.ToLower() == "oracle")
                return DBType.Oracle;
            else
                return DBType.SqlServer;
        }
    }

    public enum DBType
    {
        MySql = 0,
        Oracle = 1,
        SqlServer = 2
    }
}
