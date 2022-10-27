using Microsoft.SqlServer.Types;
using MySqlConnector;
using System.Data.SqlTypes;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class OracleStGeometryHelper
    {
        #region SqlServer   

        public static SqlGeography ToSqlGeography(StGeometry geometry)
        {
            return OracleSdoGeometryHelper.ToSqlGeography(geometry.Geom);
        }

        public static SqlGeometry ToSqlGeometry(StGeometry geometry)
        {
            return OracleSdoGeometryHelper.ToSqlGeometry(geometry.Geom);
        }

        #endregion

        #region MySql      

        public static MySqlGeometry ToMySqlGeometry(StGeometry geometry)
        {
            return OracleSdoGeometryHelper.ToMySqlGeometry(geometry.Geom);
        }
        #endregion      

        #region Postgres
        public static PgGeom.Geometry ToPostgresGeometry(StGeometry geometry)
        {
            return OracleSdoGeometryHelper.ToPostgresGeometry(geometry.Geom);
        }

        public static PgGeom.Geometry ToPostgresGeography(StGeometry geometry)
        {
            return OracleSdoGeometryHelper.ToPostgresGeography(geometry.Geom);
        }
        #endregion
    }
}
