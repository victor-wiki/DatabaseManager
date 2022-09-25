using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using Microsoft.SqlServer.Types;
using NpgsqlTypes;
using System.Data.SqlTypes;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Linq;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using System.Runtime.CompilerServices;
using System.IO;

namespace DatabaseInterpreter.Core
{
    public class GeometryHelper
    {
        #region Common Method
        public static OpenGisGeometryType GetGeometryType(SqlGeography geography)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geography.STGeometryType().Value);
            return geometryType;
        }

        public static OpenGisGeometryType GetGeometryType(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geometry.STGeometryType().Value);
            return geometryType;
        }
        #endregion

        #region SqlSever -> Postgres
        #region SqlGeography
        public static Geometry SqlGeographyToPostgresGeography(SqlGeography geography)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geography.STGeometryType().Value);

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    var coordinate = new Coordinate(geography.Long.Value, geography.Lat.Value);
                    return new Point(coordinate);
                case OpenGisGeometryType.LineString:
                    return new LineString(SqlGeographyToPostgresPoints(geography));
                case OpenGisGeometryType.Polygon:
                    return SqlGeometryToPostgresPolygon(geography);
                case OpenGisGeometryType.MultiPoint:
                    return new MultiPoint(SqlGeographyToPostgresMultiPoints(geography));
                case OpenGisGeometryType.MultiLineString:
                    return SqlGeographyToPostgresMultiLineString(geography);
                case OpenGisGeometryType.MultiPolygon:
                    return SqlGeographyToPostgresMultiPolygon(geography);
                case OpenGisGeometryType.GeometryCollection:
                    return SqlGeographyToGeometryGeometryCollection(geography);
            }

            return null;
        }

        public static Coordinate[] SqlGeographyToPostgresPoints(SqlGeography geography)
        {
            Coordinate[] pts = new Coordinate[geography.STNumPoints().Value];
            for (int i = 1; i <= pts.Length; i++)
            {
                SqlGeography ptGeometry = geography.STPointN(i);
                var coordinate = new Coordinate(ptGeometry.Long.Value, ptGeometry.Lat.Value);
                pts[i - 1] = coordinate;
            }
            return pts;
        }

        private static Point[] SqlGeographyToPostgresMultiPoints(SqlGeography geography)
        {
            Point[] points = new Point[geography.STNumGeometries().Value];
            for (int i = 1; i <= points.Length; i++)
                points[i - 1] = new Point(new Coordinate(geography.Lat.Value, geography.Long.Value));

            return points;
        }

        public static MultiLineString SqlGeographyToPostgresMultiLineString(SqlGeography geography)
        {
            LineString[] lineStrings = new LineString[geography.STNumGeometries().Value];
            for (int i = 1; i <= lineStrings.Length; i++)
                lineStrings[i - 1] = new LineString(SqlGeographyToPostgresPoints(geography));

            return new MultiLineString(lineStrings);
        }

        public static Polygon SqlGeometryToPostgresPolygon(SqlGeography geography)
        {
            LinearRing exterior = new LinearRing(SqlGeographyToPostgresPoints(geography));

            LinearRing[] interior = null;
            if (geography.NumRings() > 0)
            {
                interior = new LinearRing[geography.NumRings().Value];
                for (int i = 1; i <= interior.Length; i++)
                    interior[i - 1] = new LinearRing(SqlGeographyToPostgresPoints(geography.RingN(i)));
            }

            return new Polygon(exterior, interior);
        }

        public static MultiPolygon SqlGeographyToPostgresMultiPolygon(SqlGeography geography)
        {
            Polygon[] polygons = new Polygon[geography.STNumGeometries().Value];
            for (var i = 1; i <= polygons.Length; i++)
                polygons[i - 1] = SqlGeometryToPostgresPolygon(geography);

            return new MultiPolygon(polygons);
        }

        public static GeometryCollection SqlGeographyToGeometryGeometryCollection(SqlGeography geography)
        {
            Geometry[] geoms = new Geometry[geography.STNumGeometries().Value];
            for (int i = 1; i <= geoms.Length; i++)
                geoms[i - 1] = SqlGeographyToPostgresGeography(geography);

            return new GeometryCollection(geoms);
        }
        #endregion
        #region SqlGeometry       
        public static Geometry SqlGeometryToPostgresGeometry(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = GetGeometryType(geometry);

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    return new Point(new Coordinate(geometry.STX.Value, geometry.STY.Value));
                case OpenGisGeometryType.LineString:
                    return new LineString(SqlGeometryToPostgresPoints(geometry));
                case OpenGisGeometryType.Polygon:
                    return SqlGeometryToPostgresPolygon(geometry);
                case OpenGisGeometryType.MultiPoint:
                    return new MultiPoint(SqlGeometryToPostgresMultiPoints(geometry));
                case OpenGisGeometryType.MultiLineString:
                    return SqlGeometryToPostgresMultiLineString(geometry);
                case OpenGisGeometryType.MultiPolygon:
                    return SqlGeometryToPostgresMultiPolygon(geometry);
                case OpenGisGeometryType.GeometryCollection:
                    return SqlGeometryToPostgresGeometryCollection(geometry);
            }

            return null;
        }

        public static Coordinate[] SqlGeometryToPostgresPoints(SqlGeometry geometry)
        {
            Coordinate[] pts = new Coordinate[geometry.STNumPoints().Value];
            for (int i = 1; i <= pts.Length; i++)
            {
                SqlGeometry ptGeometry = geometry.STPointN(i);
                pts[i - 1] = new Coordinate(ptGeometry.STX.Value, ptGeometry.STY.Value);
            }
            return pts;
        }

        public static Point[] SqlGeometryToPostgresMultiPoints(SqlGeometry geometry)
        {
            Point[] points = new Point[geometry.STNumGeometries().Value];
            for (int i = 1; i <= points.Length; i++)
                points[i - 1] = new Point(new Coordinate(geometry.STX.Value, geometry.STY.Value));

            return points;
        }

        public static Polygon SqlGeometryToPostgresPolygon(SqlGeometry geometry)
        {
            LinearRing exterior = new LinearRing(SqlGeometryToPostgresPoints(geometry.STExteriorRing()));

            LinearRing[] interior = null;
            if (geometry.STNumInteriorRing() > 0)
            {
                interior = new LinearRing[geometry.STNumInteriorRing().Value];
                for (int i = 1; i <= interior.Length; i++)
                    interior[i - 1] = new LinearRing(SqlGeometryToPostgresPoints(geometry.STInteriorRingN(i)));
            }

            return new Polygon(exterior, interior);
        }

        public static MultiPolygon SqlGeometryToPostgresMultiPolygon(SqlGeometry geometry)
        {
            Polygon[] polygons = new Polygon[geometry.STNumGeometries().Value];
            for (var i = 1; i <= polygons.Length; i++)
                polygons[i - 1] = SqlGeometryToPostgresPolygon(geometry);

            return new MultiPolygon(polygons);
        }

        public static MultiLineString SqlGeometryToPostgresMultiLineString(SqlGeometry geometry)
        {
            LineString[] lineStrings = new LineString[geometry.STNumGeometries().Value];
            for (int i = 1; i <= lineStrings.Length; i++)
                lineStrings[i - 1] = new LineString(SqlGeometryToPostgresPoints(geometry));

            return new MultiLineString(lineStrings);
        }

        public static GeometryCollection SqlGeometryToPostgresGeometryCollection(SqlGeometry geometry)
        {
            Geometry[] geoms = new Geometry[geometry.STNumGeometries().Value];
            for (int i = 1; i <= geoms.Length; i++)
                geoms[i - 1] = SqlGeometryToPostgresGeometry(geometry.STGeometryN(i));

            return new GeometryCollection(geoms);
        }
        #endregion
        #endregion

        #region Postgres -> SqlSever
        public static SqlGeography PostgresGeographyToSqlGeography(Geometry geometry)
        {
            string geometryType = geometry.GeometryType;

            if (geometryType == "Point")
            {
                NetTopologySuite.Geometries.Point point = geometry as NetTopologySuite.Geometries.Point;
                return SqlGeography.Point(point.X, point.Y, point.SRID);
            }
            else if (geometryType == "LineString")
            {
                NetTopologySuite.Geometries.LineString lineString = geometry as NetTopologySuite.Geometries.LineString;
                return SqlGeography.STLineFromText(new SqlChars(new SqlString(lineString.AsText())), lineString.SRID < 0 ? 0 : lineString.SRID);
            }
            else if (geometryType == "Polygon")
            {
                NetTopologySuite.Geometries.Polygon polygon = geometry as NetTopologySuite.Geometries.Polygon;
                return SqlGeography.STPolyFromText(new SqlChars(new SqlString(polygon.AsText())), polygon.SRID < 0 ? 0 : polygon.SRID);
            }
            else if (geometryType == "MultiPoint")
            {
                NetTopologySuite.Geometries.MultiPoint multiPoint = geometry as NetTopologySuite.Geometries.MultiPoint;
                return SqlGeography.STMPointFromText(new SqlChars(new SqlString(multiPoint.AsText())), multiPoint.SRID < 0 ? 0 : multiPoint.SRID);
            }
            else if (geometryType == "MultiLineString")
            {
                NetTopologySuite.Geometries.MultiLineString multiLineString = geometry as NetTopologySuite.Geometries.MultiLineString;
                return SqlGeography.STMLineFromText(new SqlChars(new SqlString(multiLineString.AsText())), multiLineString.SRID < 0 ? 0 : multiLineString.SRID);
            }
            else if (geometryType == "MultiLineString")
            {
                NetTopologySuite.Geometries.MultiPolygon multiPolygon = geometry as NetTopologySuite.Geometries.MultiPolygon;
                return SqlGeography.STMPolyFromText(new SqlChars(new SqlString(multiPolygon.AsText())), multiPolygon.SRID < 0 ? 0 : multiPolygon.SRID);
            }
            else if (geometryType == "GeometryCollection")
            {
                NetTopologySuite.Geometries.GeometryCollection geometryCollection = geometry as NetTopologySuite.Geometries.GeometryCollection;
                return SqlGeography.STGeomCollFromText(new SqlChars(new SqlString(geometryCollection.AsText())), geometryCollection.SRID < 0 ? 0 : geometryCollection.SRID);
            }

            return null;
        }
        public static SqlGeometry PostgresGeometryToSqlGeometry(Geometry geometry)
        {
            string geometryType = geometry.GeometryType;

            if (geometryType == "Point")
            {
                NetTopologySuite.Geometries.Point point = geometry as NetTopologySuite.Geometries.Point;
                return SqlGeometry.STMPointFromText(new SqlChars(new SqlString(point.AsText())), point.SRID < 0 ? 0 : point.SRID);
            }
            else if (geometryType == "LineString")
            {
                NetTopologySuite.Geometries.LineString lineString = geometry as NetTopologySuite.Geometries.LineString;
                return SqlGeometry.STLineFromText(new SqlChars(new SqlString(lineString.AsText())), lineString.SRID < 0 ? 0 : lineString.SRID);
            }
            else if (geometryType == "Polygon")
            {
                NetTopologySuite.Geometries.Polygon polygon = geometry as NetTopologySuite.Geometries.Polygon;
                return SqlGeometry.STPolyFromText(new SqlChars(new SqlString(polygon.AsText())), polygon.SRID < 0 ? 0 : polygon.SRID);
            }
            else if (geometryType == "MultiPoint")
            {
                NetTopologySuite.Geometries.MultiPoint multiPoint = geometry as NetTopologySuite.Geometries.MultiPoint;
                return SqlGeometry.STMPointFromText(new SqlChars(new SqlString(multiPoint.AsText())), multiPoint.SRID < 0 ? 0 : multiPoint.SRID);
            }
            else if (geometryType == "MultiLineString")
            {
                NetTopologySuite.Geometries.MultiLineString multiLineString = geometry as NetTopologySuite.Geometries.MultiLineString;
                return SqlGeometry.STMLineFromText(new SqlChars(new SqlString(multiLineString.AsText())), multiLineString.SRID < 0 ? 0 : multiLineString.SRID);
            }
            else if (geometryType == "MultiLineString")
            {
                NetTopologySuite.Geometries.MultiPolygon multiPolygon = geometry as NetTopologySuite.Geometries.MultiPolygon;
                return SqlGeometry.STMPolyFromText(new SqlChars(new SqlString(multiPolygon.AsText())), multiPolygon.SRID < 0 ? 0 : multiPolygon.SRID);
            }
            else if (geometryType == "GeometryCollection")
            {
                NetTopologySuite.Geometries.GeometryCollection geometryCollection = geometry as NetTopologySuite.Geometries.GeometryCollection;
                return SqlGeometry.STGeomCollFromText(new SqlChars(new SqlString(geometryCollection.AsText())), geometryCollection.SRID < 0 ? 0 : geometryCollection.SRID);
            }

            return null;
        }
        #endregion

        #region SqlSever -> MySql
        public static MySqlGeometry SqlGeographyToMySqlGeometry(SqlGeography geography)
        {
            SqlGeometry geometry = SqlGeometry.STGeomFromText(geography.STAsText(), geography.STSrid.Value);

            return MySqlGeometry.FromMySql(SqlGeometryToMySqlBytes(geometry));
        }

        public static MySqlGeometry SqlGeometryToMySqlGeometry(SqlGeometry geometry)
        {
            return MySqlGeometry.FromMySql(SqlGeometryToMySqlBytes(geometry));
        }

        public static byte[] SqlGeometryToMySqlBytes(SqlGeometry geometry, bool isBeginning = true)
        {
            OpenGisGeometryType geometryType = GetGeometryType(geometry);
            List<byte> bytes = new List<byte>();

            if (isBeginning)
            {
                byte[] sridBytes = BitConverter.GetBytes(geometry.STSrid.Value); //4 bytes   
                bytes.AddRange(sridBytes);
            }

            bytes.Add(1); //1 byte for order (1 = little-endian)    

            Func<OpenGisGeometryType, int, int> ComparePointNum = (geoType, pointNum) =>
            {
                return (pointNum > (int)geoType ? pointNum : (int)geoType);
            };

            Action<byte, int> AppendToByteList = (value, size) =>
            {
                bytes.Add(value);
                bytes.AddRange(new byte[size - 1]);
            };

            Action<int, SqlGeometry> AppendPointsToByteList = (pointNum, sqlGeom) =>
            {
                for (int k = 1; k <= pointNum; k++)
                {
                    SqlGeometry geom = sqlGeom.STPointN(k);

                    long num = BitConverter.DoubleToInt64Bits(geom.STX.Value);
                    long num2 = BitConverter.DoubleToInt64Bits(geom.STY.Value);

                    for (int m = 0; m < 8; m++) //8 bytes for double-precision X coordinate
                    {
                        bytes.Add((byte)(num & 0xFF));
                        num >>= 8;
                    }

                    for (int n = 0; n < 8; n++) //8 bytes for double-precision Y coordinate 
                    {
                        bytes.Add((byte)(num2 & 0xFF));
                        num2 >>= 8;
                    }
                }
            };

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    AppendToByteList((byte)geometryType, 4);
                    AppendPointsToByteList(geometry.STNumPoints().Value, geometry);
                    break;
                case OpenGisGeometryType.LineString:
                    AppendToByteList((byte)geometryType, 4);
                    AppendToByteList((byte)ComparePointNum(geometryType, geometry.STNumPoints().Value), 4);
                    AppendPointsToByteList(geometry.STNumPoints().Value, geometry);
                    break;
                case OpenGisGeometryType.Polygon:
                    int interiorRingNum = geometry.STNumInteriorRing().Value;

                    AppendToByteList((byte)geometryType, 4);
                    AppendToByteList((byte)(geometry.STNumGeometries().Value + interiorRingNum), 4);

                    int exteriorPointNum = geometry.STExteriorRing().STNumPoints().Value;
                    AppendToByteList((byte)ComparePointNum(geometryType, exteriorPointNum), 4);
                    AppendPointsToByteList((byte)ComparePointNum(geometryType, exteriorPointNum), geometry.STExteriorRing());

                    for (int i = 1; i <= interiorRingNum; i++)
                    {
                        var geom = geometry.STInteriorRingN(i);
                        AppendToByteList((byte)ComparePointNum(geometryType, geom.STNumPoints().Value), 4);
                        AppendPointsToByteList((byte)ComparePointNum(geometryType, geom.STNumPoints().Value), geom);
                    }
                    break;
                case OpenGisGeometryType.MultiPoint:
                    int pointNum = geometry.STNumGeometries().Value;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)pointNum, 4); //point num

                    for (int i = 1; i <= pointNum; i++)
                    {
                        SqlGeometry geom = geometry.STPointN(i);

                        bytes.AddRange(SqlGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.MultiLineString:
                    int linestringNum = geometry.STNumGeometries().Value;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)linestringNum, 4); //linestring num

                    for (int i = 1; i <= linestringNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(SqlGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    int polygonNum = geometry.STNumGeometries().Value;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)polygonNum, 4); //polygon num

                    for (int i = 1; i <= polygonNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(SqlGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    int geometryNum = geometry.STNumGeometries().Value;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)geometryNum, 4); //geometry num

                    for (int i = 1; i <= geometryNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(SqlGeometryToMySqlBytes(geom, false));
                    }
                    break;
            }

            return bytes.ToArray();
        }

        #endregion

        #region MySql -> SqlServer
        public static SqlGeometry MySqlGeometryBytesToSqlGeometry(byte[] bytes)
        {
            Geometry geometry = MySqlGeometryToPostgresGeometry(bytes);
            string text = geometry.ToText();

            return SqlGeometry.STGeomFromText(new SqlChars(text), geometry.SRID);
        }
        #endregion

        #region MySql -> Postgres
        public static Geometry MySqlGeometryToPostgresGeometry(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryReader r = new BinaryReader(ms);
                var srid = r.ReadInt32();
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                Geometry geometry = MySqlGeometryToPostgresGeometry(type, r, srid);

                return geometry;
            }
        }

        private static Geometry MySqlGeometryToPostgresGeometry(int type, BinaryReader r, int srid)
        {
            switch ((OpenGisGeometryType)type)
            {
                case OpenGisGeometryType.Point:
                    return MySqlPointToPostgresGeometry(r, srid);
                case OpenGisGeometryType.LineString:
                    return MySqlLineStringToPostgresGeometry(r, srid);
                case OpenGisGeometryType.Polygon:
                    return MySqlPolygonToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiPoint:
                    return MySqlMultiPointToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiLineString:
                    return MySqlMultiLineStringToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiPolygon:
                    return MySqlMultiPolygonToPostgresGeometry(r, srid);
                case OpenGisGeometryType.GeometryCollection:
                    return MySqlGeometryCollectionToPostgresGeometry(r, srid);
            }

            return null;
        }
        private static Geometry MySqlPointToPostgresGeometry(BinaryReader r, int srid)
        {
            double x = r.ReadDouble();
            double y = r.ReadDouble();
            return new Point(x, y) { SRID = srid };
        }

        private static Geometry MySqlLineStringToPostgresGeometry(BinaryReader r, int srid)
        {
            return GetLineString(r, srid);
        }

        private static LineString GetLineString(BinaryReader r, int srid)
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            int pointNum = r.ReadInt32();

            for (int i = 1; i <= pointNum; i++)
            {
                coordinates.Add(ReadCoordinate(r));
            }

            LineString lineString = new LineString(coordinates.ToArray());

            return lineString;
        }

        private static Coordinate ReadCoordinate(BinaryReader r)
        {
            double x = r.ReadDouble();
            double y = r.ReadDouble();

            return new Coordinate(x, y);
        }

        private static Polygon GetPolgon(BinaryReader r, int srid)
        {
            int total = r.ReadInt32();
            int pointNum = r.ReadInt32();

            LinearRing exterior = new LinearRing(GetCoordinates(r, pointNum));

            LinearRing[] interior = null;

            if (total > 1)
            {
                pointNum = r.ReadInt32();

                interior = new LinearRing[total - 1];

                for (int i = 1; i <= interior.Length; i++)
                {
                    interior[i - 1] = new LinearRing(GetCoordinates(r, pointNum));
                }
            }

            Polygon polygon = new Polygon(exterior, interior);

            return polygon;
        }

        private static Geometry MySqlPolygonToPostgresGeometry(BinaryReader r, int srid)
        {
            return GetPolgon(r, srid);
        }

        private static Coordinate[] GetCoordinates(BinaryReader r, int pointNum)
        {
            Coordinate[] coordinates = new Coordinate[pointNum];

            for (int i = 0; i < pointNum; i++)
            {
                coordinates[i] = ReadCoordinate(r);
            }

            return coordinates;
        }

        private static Geometry MySqlMultiPointToPostgresGeometry(BinaryReader r, int srid)
        {
            List<Point> points = new List<Point>();

            int pointNum = r.ReadInt32();

            for (int i = 1; i <= pointNum; i++)
            {
                points.Add(new Point(ReadCoordinate(r)));
            }

            MultiPoint multiPoint = new MultiPoint(points.ToArray());

            return multiPoint;
        }

        private static Geometry MySqlMultiLineStringToPostgresGeometry(BinaryReader r, int srid)
        {
            int linestringNum = r.ReadInt32();

            LineString[] lineStrings = new LineString[linestringNum];

            for (int i = 0; i < linestringNum; i++)
            {
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                lineStrings[i] = GetLineString(r, srid);
            }

            MultiLineString multiLineString = new MultiLineString(lineStrings);

            return multiLineString;
        }

        private static Geometry MySqlMultiPolygonToPostgresGeometry(BinaryReader r, int srid)
        {
            int polgonNum = r.ReadInt32();

            Polygon[] polygons = new Polygon[polgonNum];

            for (int i = 0; i < polgonNum; i++)
            {
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                polygons[i] = GetPolgon(r, srid);
            }

            MultiPolygon multiPolygon = new MultiPolygon(polygons);

            return multiPolygon;
        }

        private static Geometry MySqlGeometryCollectionToPostgresGeometry(BinaryReader r, int srid)
        {
            int geometryNum = r.ReadInt32();

            Geometry[] geoms = new Geometry[geometryNum];

            for (int i = 1; i <= geometryNum; i++)
            {
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                geoms[i - 1] = MySqlGeometryToPostgresGeometry(type, r, srid);
            }

            return new GeometryCollection(geoms);
        }
        #endregion

        #region Postgres -> MySql
        public static MySqlGeometry PostgresGeometryToMySqlGeometry(Geometry geometry)
        {
            return MySqlGeometry.FromMySql(PostgresGeometryToMySqlBytes(geometry));
        }

        public static byte[] PostgresGeometryToMySqlBytes(Geometry geometry, bool isBeginning = true)
        {
            OgcGeometryType geometryType = geometry.OgcGeometryType;

            List<byte> bytes = new List<byte>();

            if (isBeginning)
            {
                byte[] sridBytes = BitConverter.GetBytes(geometry.SRID); //4 bytes   
                bytes.AddRange(sridBytes);
            }

            bytes.Add(1); //1 byte for order (1 = little-endian)    

            Func<OgcGeometryType, int, int> ComparePointNum = (geoType, pointNum) =>
            {
                return (pointNum > (int)geoType ? pointNum : (int)geoType);
            };

            Action<byte, int> AppendToByteList = (value, size) =>
            {
                bytes.Add(value);
                bytes.AddRange(new byte[size - 1]);
            };

            Action<int, Geometry> AppendPointsToByteList = (pointNum, geom) =>
            {
                for (int k = 0; k < pointNum; k++)
                {
                    Point g = null;

                    if (geom is LineString lineString)
                    {
                        g = lineString.GetPointN(k);
                    }
                    else if (geom is Point point)
                    {
                        g = point;
                    }

                    long num = BitConverter.DoubleToInt64Bits(g.X);
                    long num2 = BitConverter.DoubleToInt64Bits(g.Y);

                    for (int m = 0; m < 8; m++) //8 bytes for double-precision X coordinate
                    {
                        bytes.Add((byte)(num & 0xFF));
                        num >>= 8;
                    }

                    for (int n = 0; n < 8; n++) //8 bytes for double-precision Y coordinate 
                    {
                        bytes.Add((byte)(num2 & 0xFF));
                        num2 >>= 8;
                    }
                }
            };

            switch (geometryType)
            {
                case OgcGeometryType.Point:
                    AppendToByteList((byte)geometryType, 4);
                    AppendPointsToByteList(geometry.NumPoints, geometry);
                    break;
                case OgcGeometryType.LineString:
                    AppendToByteList((byte)geometryType, 4);
                    AppendToByteList((byte)ComparePointNum(geometryType, geometry.NumPoints), 4);
                    AppendPointsToByteList(geometry.NumPoints, geometry);
                    break;
                case OgcGeometryType.Polygon:
                    Polygon polygon = geometry as Polygon;

                    int interiorRingNum = polygon.NumInteriorRings;

                    AppendToByteList((byte)geometryType, 4);
                    AppendToByteList((byte)(polygon.NumGeometries + interiorRingNum), 4);

                    int exteriorPointNum = polygon.ExteriorRing.NumPoints;
                    AppendToByteList((byte)ComparePointNum(geometryType, exteriorPointNum), 4);
                    AppendPointsToByteList((byte)ComparePointNum(geometryType, exteriorPointNum), polygon.ExteriorRing);

                    for (int i = 0; i < interiorRingNum; i++)
                    {
                        LineString geom = polygon.GetInteriorRingN(i);
                        AppendToByteList((byte)ComparePointNum(geometryType, geom.NumPoints), 4);
                        AppendPointsToByteList((byte)ComparePointNum(geometryType, geom.NumPoints), geom);
                    }
                    break;
                case OgcGeometryType.MultiPoint:
                    MultiPoint multiPoint = geometry as MultiPoint;

                    int pointNum = multiPoint.NumPoints;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)pointNum, 4); //point num

                    for (int i = 0; i < pointNum; i++)
                    {
                        Geometry geom = multiPoint.GetGeometryN(i);

                        bytes.AddRange(PostgresGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OgcGeometryType.MultiLineString:
                    MultiLineString multiLineString = geometry as MultiLineString;

                    int linestringNum = multiLineString.NumGeometries;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)linestringNum, 4); //linestring num

                    for (int i = 0; i < linestringNum; i++)
                    {
                        Geometry geom = multiLineString.GetGeometryN(i);

                        bytes.AddRange(PostgresGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OgcGeometryType.MultiPolygon:
                    MultiPolygon multiPolygon = geometry as MultiPolygon;

                    int polygonNum = multiPolygon.NumGeometries;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)polygonNum, 4); //polygon num

                    for (int i = 0; i < polygonNum; i++)
                    {
                        Geometry geom = multiPolygon.GetGeometryN(i);

                        bytes.AddRange(PostgresGeometryToMySqlBytes(geom, false));
                    }
                    break;
                case OgcGeometryType.GeometryCollection:
                    GeometryCollection geometryCollection = geometry as GeometryCollection;

                    int geometryNum = geometryCollection.NumGeometries;
                    AppendToByteList((byte)geometryType, 4); //type
                    AppendToByteList((byte)geometryNum, 4); //geometry num

                    for (int i = 0; i < geometryNum; i++)
                    {
                        Geometry geom = geometryCollection.GetGeometryN(i);

                        bytes.AddRange(PostgresGeometryToMySqlBytes(geom, false));
                    }
                    break;
            }

            return bytes.ToArray();
        }
        #endregion
    }
}
