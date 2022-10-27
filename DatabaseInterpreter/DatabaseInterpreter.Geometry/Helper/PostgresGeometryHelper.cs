using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.NetworkInformation;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class PostgresGeometryHelper
    {
        #region Common Method
        public static bool CanConvertToGeography(PgGeom.Geometry geometry)
        {
            return !geometry.Coordinates.Any(item => Math.Abs(item.X) > 90);
        }

        public static void ReverseCoordinates(PgGeom.Geometry geometry)
        {
            var coordinates = geometry.Coordinates;

            foreach (var coordinate in coordinates)
            {
                double temp = coordinate.X;
                coordinate.X = coordinate.Y;
                coordinate.Y = temp;
            }
        }
        #endregion

        #region SqlServer
        public static SqlGeography ToSqlGeography(PgGeom.Geometry geometry)
        {
            string geometryType = geometry.GeometryType;

            int srid = geometry.SRID < 0 ? 0 : geometry.SRID;

            if (geometryType == nameof(Point))
            {
                PgGeom.Point point = (PgGeom.Point)geometry;
                return SqlGeography.Point(point.X, point.Y, srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.LineString))
            {
                PgGeom.LineString lineString = (PgGeom.LineString)geometry;
                return SqlGeography.STLineFromText(new SqlChars(new SqlString(lineString.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.Polygon))
            {
                PgGeom.Polygon polygon = (PgGeom.Polygon)geometry;
                return SqlGeography.STPolyFromText(new SqlChars(new SqlString(polygon.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiPoint))
            {
                PgGeom.MultiPoint multiPoint = (PgGeom.MultiPoint)geometry;
                return SqlGeography.STMPointFromText(new SqlChars(new SqlString(multiPoint.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiLineString))
            {
                PgGeom.MultiLineString multiLineString = (PgGeom.MultiLineString)geometry;
                return SqlGeography.STMLineFromText(new SqlChars(new SqlString(multiLineString.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiPolygon))
            {
                PgGeom.MultiPolygon multiPolygon = (PgGeom.MultiPolygon)geometry;
                return SqlGeography.STMPolyFromText(new SqlChars(new SqlString(multiPolygon.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.GeometryCollection))
            {
                PgGeom.GeometryCollection geometryCollection = (PgGeom.GeometryCollection)geometry;
                return SqlGeography.STGeomCollFromText(new SqlChars(new SqlString(geometryCollection.AsText())), srid);
            }

            return null;
        }

        public static SqlGeometry ToSqlGeometry(PgGeom.Geometry geometry)
        {
            string geometryType = geometry.GeometryType;

            int srid = geometry.SRID < 0 ? 0 : geometry.SRID;

            if (geometryType == nameof(OpenGisGeometryType.Point))
            {
                PgGeom.Point point = (PgGeom.Point)geometry;
                return SqlGeometry.STPointFromText(new SqlChars(new SqlString(point.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.LineString))
            {
                PgGeom.LineString lineString = (PgGeom.LineString)geometry;
                return SqlGeometry.STLineFromText(new SqlChars(new SqlString(lineString.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.Polygon))
            {
                PgGeom.Polygon polygon = (PgGeom.Polygon)geometry;
                return SqlGeometry.STPolyFromText(new SqlChars(new SqlString(polygon.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiPoint))
            {
                PgGeom.MultiPoint multiPoint = (PgGeom.MultiPoint)geometry;
                return SqlGeometry.STMPointFromText(new SqlChars(new SqlString(multiPoint.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiLineString))
            {
                PgGeom.MultiLineString multiLineString = (PgGeom.MultiLineString)geometry;
                return SqlGeometry.STMLineFromText(new SqlChars(new SqlString(multiLineString.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.MultiPolygon))
            {
                PgGeom.MultiPolygon multiPolygon = (PgGeom.MultiPolygon)geometry;
                return SqlGeometry.STMPolyFromText(new SqlChars(new SqlString(multiPolygon.AsText())), srid);
            }
            else if (geometryType == nameof(OpenGisGeometryType.GeometryCollection))
            {
                PgGeom.GeometryCollection geometryCollection = (PgGeom.GeometryCollection)geometry;
                return SqlGeometry.STGeomCollFromText(new SqlChars(new SqlString(geometryCollection.AsText())), srid);
            }

            return null;
        }
        #endregion

        #region MySql
        public static MySqlGeometry ToMySqlGeometry(PgGeom.Geometry geometry)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(geometry);

            return SqlGeometryHelper.ToMySqlGeometry(sqlGeometry);
        }
        #endregion

        #region Oracle
        public static SdoGeometry ToOracleSdoGeometry(PgGeom.Geometry geometry)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(geometry);

            return SqlGeometryHelper.ToOracleSdoGeometry(sqlGeometry);
        }

        public static StGeometry ToOracleStGeometry(PgGeom.Geometry geometry)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(geometry);

            return SqlGeometryHelper.ToOracleStGeometry(sqlGeometry);
        }
        #endregion
    }
}
