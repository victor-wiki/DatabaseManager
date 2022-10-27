using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class MySqlGeometryHelper
    {
        #region SqlServer
        public static SqlGeometry ToSqlGeometry(byte[] bytes)
        {
            PgGeom.Geometry geometry = ToPostgresGeometry(bytes);
            string text = geometry.AsText();

            return SqlGeometry.STGeomFromText(new SqlChars(text), geometry.SRID);
        }

        public static SqlGeography ToSqlGeography(byte[] bytes)
        {
            PgGeom.Geometry geometry = ToPostgresGeography(bytes);

            return SqlGeography.STGeomFromText(new SqlChars(geometry.AsText()), geometry.SRID);
        }
        #endregion

        #region Postgres
        public static PgGeom.Geometry ToPostgresGeography(byte[] bytes)
        {
            PgGeom.Geometry geom = ToPostgresGeometry(bytes);

            if (geom != null)
            {
                PostgresGeometryHelper.ReverseCoordinates(geom);
            }

            return geom;
        }

        public static PgGeom.Geometry ToPostgresGeometry(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryReader r = new BinaryReader(ms);
                var srid = r.ReadInt32();
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                PgGeom.Geometry geometry = ToPostgresGeometry(type, r, srid);

                return geometry;
            }
        }

        private static PgGeom.Geometry ToPostgresGeometry(int type, BinaryReader r, int srid)
        {
            switch ((OpenGisGeometryType)type)
            {
                case OpenGisGeometryType.Point:
                    return PointToPostgresGeometry(r, srid);
                case OpenGisGeometryType.LineString:
                    return LineStringToPostgresGeometry(r, srid);
                case OpenGisGeometryType.Polygon:
                    return PolygonToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiPoint:
                    return MultiPointToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiLineString:
                    return MultiLineStringToPostgresGeometry(r, srid);
                case OpenGisGeometryType.MultiPolygon:
                    return MultiPolygonToPostgresGeometry(r, srid);
                case OpenGisGeometryType.GeometryCollection:
                    return GeometryCollectionToPostgresGeometry(r, srid);
            }

            return null;
        }

        private static PgGeom.Geometry PointToPostgresGeometry(BinaryReader r, int srid)
        {
            double x = r.ReadDouble();
            double y = r.ReadDouble();
            return new Point(x, y) { SRID = srid };
        }

        private static PgGeom.Geometry LineStringToPostgresGeometry(BinaryReader r, int srid)
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
            int total = r.ReadInt32(); //total rings
            int pointNum = r.ReadInt32(); // points num of exterior ring

            LinearRing exterior = new LinearRing(GetCoordinates(r, pointNum));

            LinearRing[] interior = null;

            if (total > 1)
            {
                interior = new LinearRing[total - 1];

                for (int i = 1; i <= interior.Length; i++)
                {
                    pointNum = r.ReadInt32();

                    interior[i - 1] = new LinearRing(GetCoordinates(r, pointNum));
                }
            }

            Polygon polygon = new Polygon(exterior, interior);

            return polygon;
        }

        private static PgGeom.Geometry PolygonToPostgresGeometry(BinaryReader r, int srid)
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

        private static PgGeom.Geometry MultiPointToPostgresGeometry(BinaryReader r, int srid)
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

        private static PgGeom.Geometry MultiLineStringToPostgresGeometry(BinaryReader r, int srid)
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

        private static PgGeom.Geometry MultiPolygonToPostgresGeometry(BinaryReader r, int srid)
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

        private static PgGeom.Geometry GeometryCollectionToPostgresGeometry(BinaryReader r, int srid)
        {
            int geometryNum = r.ReadInt32();

            PgGeom.Geometry[] geoms = new PgGeom.Geometry[geometryNum];

            for (int i = 1; i <= geometryNum; i++)
            {
                byte order = r.ReadByte();
                int type = r.ReadInt32();

                geoms[i - 1] = ToPostgresGeometry(type, r, srid);
            }

            return new GeometryCollection(geoms);
        }
        #endregion

        #region Oracle
        public static SdoGeometry ToOracleSdoGeometry(byte[] bytes)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(bytes);

            return SqlGeometryHelper.ToOracleSdoGeometry(sqlGeometry);
        }

        public static StGeometry ToOracleStGeometry(byte[] bytes)
        {
            SqlGeometry sqlGeometry = ToSqlGeometry(bytes);

            return SqlGeometryHelper.ToOracleStGeometry(sqlGeometry);
        }
        #endregion
    }
}
