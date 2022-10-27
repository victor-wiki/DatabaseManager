using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class SqlGeographyHelper
    {
        #region Common Method
        public static OpenGisGeometryType GetGeometryType(SqlGeography geography)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geography.STGeometryType().Value);
            return geometryType;
        }
        #endregion

        #region Postgres
        public static PgGeom.Geometry ToPostgresGeography(SqlGeography geography)
        {
            PgGeom.Geometry geom = ToPostgresGeometry(geography);

            if (geom != null)
            {
                PostgresGeometryHelper.ReverseCoordinates(geom);               
            }

            return geom;
        }

        public static PgGeom.Geometry ToPostgresGeometry(SqlGeography geography)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geography.STGeometryType().Value);

            PgGeom.Geometry geom = null;

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    var coordinate = new Coordinate(geography.Lat.Value, geography.Long.Value);
                    geom = new Point(coordinate);
                    break;
                case OpenGisGeometryType.LineString:
                    geom = new LineString(ToPostgresPoints(geography));
                    break;
                case OpenGisGeometryType.Polygon:
                    geom = ToPostgresPolygon(geography);
                    break;
                case OpenGisGeometryType.MultiPoint:
                    geom = new MultiPoint(ToPostgresMultiPoints(geography));
                    break;
                case OpenGisGeometryType.MultiLineString:
                    geom = ToPostgresMultiLineString(geography);
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    geom = ToPostgresMultiPolygon(geography);
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    geom = ToPostgresGeometryCollection(geography);
                    break;
            }

            if (geom != null)
            {
                geom.SRID = geography.STSrid.Value;
            }

            return geom;
        }

        public static Coordinate[] ToPostgresPoints(SqlGeography geography)
        {
            Coordinate[] points = new Coordinate[geography.STNumPoints().Value];

            for (int i = 1; i <= points.Length; i++)
            {
                SqlGeography geom = geography.STPointN(i);

                var coordinate = new Coordinate(geom.Lat.Value, geom.Long.Value);

                points[i - 1] = coordinate;
            }

            return points;
        }

        private static Point[] ToPostgresMultiPoints(SqlGeography geography)
        {
            Point[] points = new Point[geography.STNumGeometries().Value];

            for (int i = 1; i <= points.Length; i++)
            {
                points[i - 1] = new Point(new Coordinate(geography.Lat.Value, geography.Long.Value));
            }

            return points;
        }

        private static MultiLineString ToPostgresMultiLineString(SqlGeography geography)
        {
            LineString[] lineStrings = new LineString[geography.STNumGeometries().Value];

            for (int i = 1; i <= lineStrings.Length; i++)
            {
                lineStrings[i - 1] = new LineString(ToPostgresPoints(geography));
            }

            return new MultiLineString(lineStrings);
        }

        private static Polygon ToPostgresPolygon(SqlGeography geography)
        {
            var ringsNum = geography.NumRings().Value;

            LinearRing exterior = new LinearRing(ToPostgresPoints(geography.RingN(1)));

            LinearRing[] interior = null;

            if (ringsNum > 1)
            {
                interior = new LinearRing[ringsNum - 1];

                for (int i = 1; i <= interior.Length; i++)
                {
                    interior[i - 1] = new LinearRing(ToPostgresPoints(geography.RingN(i + 1)));
                }
            }

            return new Polygon(exterior, interior);
        }

        private static MultiPolygon ToPostgresMultiPolygon(SqlGeography geography)
        {
            Polygon[] polygons = new Polygon[geography.STNumGeometries().Value];

            for (var i = 1; i <= polygons.Length; i++)
            {
                polygons[i - 1] = ToPostgresPolygon(geography.STGeometryN(i));
            }

            return new MultiPolygon(polygons);
        }

        private static GeometryCollection ToPostgresGeometryCollection(SqlGeography geography)
        {
            PgGeom.Geometry[] geoms = new PgGeom.Geometry[geography.STNumGeometries().Value];

            for (int i = 1; i <= geoms.Length; i++)
            {
                geoms[i - 1] = ToPostgresGeometry(geography.STGeometryN(i));
            }

            return new GeometryCollection(geoms);
        }
        #endregion

        #region MySql      
        public static MySqlGeometry ToMySqlGeometry(SqlGeography geography)
        {
            PgGeom.Geometry geom = ToPostgresGeography(geography);

            SqlGeometry sqlGeometry = SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(geom.AsText()), geom.SRID);

            return MySqlGeometry.FromMySql(SqlGeometryHelper.ToMySqlBytes(sqlGeometry));
        }
        #endregion

        #region Oracle
        public static StGeometry ToOracleStGeometry(SqlGeography geography)
        {
            if (!geography.IsNull)
            {
                SdoGeometry sg = ToOracleSdoGeometry(geography);

                if (sg != null)
                {
                    return new StGeometry() { Geom = sg };
                }
            }

            return null;
        }

        public static SdoGeometry ToOracleSdoGeometry(SqlGeography geography)
        {
            OpenGisGeometryType geographyType = GetGeometryType(geography);

            switch (geographyType)
            {
                case OpenGisGeometryType.Point:
                    int type = geography.HasZ ? 3001 : 2001;
                    return new SdoGeometry() { Type = type, Srid = geography.STSrid.Value, Point = new SdoPoint() { X = (decimal)geography.Long.Value, Y = (decimal)geography.Lat.Value, Z = (decimal)geography.Z.Value } };
                case OpenGisGeometryType.LineString:
                    return ToOracleLineString(geography);
                case OpenGisGeometryType.Polygon:
                    return ToOraclePolygon(geography);
                case OpenGisGeometryType.MultiPoint:
                    return ToOracleMultiPoints(geography);
                case OpenGisGeometryType.MultiLineString:
                    return ToOracleMultiLineString(geography);
                case OpenGisGeometryType.MultiPolygon:
                    return ToOracleMultiPolygon(geography);
                case OpenGisGeometryType.GeometryCollection:
                    return ToOracleGeometryCollection(geography);
            }

            return null;
        }

        private static decimal[] GetOracleSdoOrdinates(SqlGeography geography)
        {
            int pointNum = geography.STNumPoints().Value;

            List<double> list = new List<double>();

            for (int i = 1; i <= pointNum; i++)
            {
                var point = geography.STPointN(i);

                list.Add(point.Long.Value);
                list.Add(point.Lat.Value);                

                if (geography.HasZ)
                {
                    list.Add(point.Z.Value);
                }
            }

            return list.Select(item => (decimal)item).ToArray();
        }

        private static SdoGeometry ToOracleMultiPoints(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3005 : 2005;
            sg.Srid = geography.STSrid.Value;
            sg.ElemInfo = GetOracleSdoElemInfo(1, 1, 2);
            sg.Ordinates = GetOracleSdoOrdinates(geography);

            return sg;
        }

        private static SdoGeometry ToOracleLineString(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3006 : 2006;
            sg.Srid = geography.STSrid.Value;
            sg.ElemInfo = GetOracleSdoElemInfo(1, 2);
            sg.Ordinates = GetOracleSdoOrdinates(geography);

            return sg;
        }

        private static SdoGeometry ToOracleMultiLineString(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3002 : 2002;
            sg.Srid = geography.STSrid.Value;

            int numOfEachPoint = geography.HasZ ? 3 : 2;
            int geomNum = geography.STNumGeometries().Value;

            List<decimal[]> list = new List<decimal[]>();

            int offset = 1;

            for (int i = 1; i <= geomNum; i++)
            {
                list.Add(GetOracleSdoElemInfo(offset, 2));

                offset += geography.STGeometryN(i).STNumPoints().Value * numOfEachPoint;
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geography);

            return sg;
        }

        private static SdoGeometry ToOraclePolygon(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3003 : 2003;
            sg.Srid = geography.STSrid.Value;

            int numOfEachPoint = geography.HasZ ? 3 : 2;

            List<decimal[]> list = new List<decimal[]>();

            list.Add(GetOracleSdoElemInfo(1, 1003));

            int ringNum = geography.NumRings().Value;
            var exterior = geography.RingN(1);

            int exteriorLength = exterior.STNumPoints().Value * numOfEachPoint;

            var interiorNum = ringNum - 1;

            int offset = 1 + exteriorLength;

            for (int i = 1; i <= interiorNum; i++)
            {
                list.Add(GetOracleSdoElemInfo(offset, 2003));

                offset += geography.RingN(i + 1).STNumPoints().Value * numOfEachPoint;
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geography);

            return sg;
        }

        private static SdoGeometry ToOracleMultiPolygon(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3007 : 2007;
            sg.Srid = geography.STSrid.Value;

            int numOfEachPoint = geography.HasZ ? 3 : 2;

            List<decimal[]> list = new List<decimal[]>();

            int geomNum = geography.STNumGeometries().Value;

            int offset = 1;

            for (int i = 1; i <= geomNum; i++)
            {
                var geom = geography.STGeometryN(i);

                list.Add(GetOracleSdoElemInfo(offset, 1003));

                int ringNum = geom.NumRings().Value;

                var exterior = geom.RingN(1);

                int exteriorLength = exterior.STNumPoints().Value * numOfEachPoint;

                var interiorNum = ringNum - 1;

                offset += exteriorLength;

                for (int j = 1; j <= interiorNum; j++)
                {
                    list.Add(GetOracleSdoElemInfo(offset, 2003));

                    offset += geom.RingN(j + 1).STNumPoints().Value * numOfEachPoint;
                }
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geography);

            return sg;
        }

        private static SdoGeometry ToOracleGeometryCollection(SqlGeography geography)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geography.HasZ ? 3004 : 2004;
            sg.Srid = geography.STSrid.Value;

            List<decimal[]> elemInfos = new List<decimal[]>();
            List<decimal[]> ordinates = new List<decimal[]>();

            int geomNum = geography.STNumGeometries().Value;

            int count = 0;

            for (int i = 1; i <= geomNum; i++)
            {
                var geom = ToOracleSdoGeometry(geography.STGeometryN(i));

                geom.ElemInfo[0] = count + 1;

                count += geom.Ordinates.Length;

                elemInfos.Add(geom.ElemInfo);
                ordinates.Add(geom.Ordinates);
            }

            sg.ElemInfo = elemInfos.SelectMany(item => item).ToArray();
            sg.Ordinates = ordinates.SelectMany(item => item).ToArray();

            return sg;
        }

        private static decimal[] GetOracleSdoElemInfo(int offset, int type, int interpretation = 1)
        {
            return SqlGeometryHelper.GetOracleSdoElemInfo(offset, type, interpretation);
        }

        #endregion
    }
}
