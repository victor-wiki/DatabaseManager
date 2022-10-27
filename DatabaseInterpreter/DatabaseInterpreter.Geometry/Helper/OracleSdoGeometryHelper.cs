using Microsoft.SqlServer.Types;
using MySqlConnector;
using System.Data.SqlTypes;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class OracleSdoGeometryHelper
    {
        #region SqlServer
        public static SqlGeography ToSqlGeography(SdoGeometry geometry)
        {
            string text = geometry.GetText();

            PgGeom.Geometry geom = OracleSdoGeometryHelper.ToPostgresGeography(geometry);

            if (geom != null)
            {
                return SqlGeography.STGeomFromText(new SqlChars(geom.AsText()), geometry.Srid);
            }

            return null;
        }

        public static SqlGeometry ToSqlGeometry(SdoGeometry geometry)
        {
            string text = geometry.GetText();

            return SqlGeometry.STGeomFromText(new SqlChars(text), geometry.Srid);
        }
        #endregion

        #region MySql
        public static MySqlGeometry ToMySqlGeometry(SdoGeometry geometry)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(geometry);

            return SqlGeometryHelper.ToMySqlGeometry(sqlGeometry);
        }
        #endregion

        #region Postgres
        public static PgGeom.Geometry ToPostgresGeometry(SdoGeometry geometry)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(geometry);

            return SqlGeometryHelper.ToPostgresGeometry(sqlGeometry);
        }

        public static PgGeom.Geometry ToPostgresGeography(SdoGeometry geometry)
        {
            PgGeom.Geometry geom = ToPostgresGeometry(geometry);

            if (geom != null)
            {
                PostgresGeometryHelper.ReverseCoordinates(geom);
            }

            return geom;
        }
        #endregion
    }
}
