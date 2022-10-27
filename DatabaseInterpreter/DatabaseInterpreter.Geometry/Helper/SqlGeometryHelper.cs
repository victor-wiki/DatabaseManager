using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Geometry
{
    public class SqlGeometryHelper
    {
        #region Common Method
        public static OpenGisGeometryType GetGeometryType(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geometry.STGeometryType().Value);
            return geometryType;
        }
        #endregion

        #region Postgres       
        public static PgGeom.Geometry ToPostgresGeography(SqlGeometry geometry)
        {
            PgGeom.Geometry geom = ToPostgresGeometry(geometry);

            if (geom != null)
            {
                PostgresGeometryHelper.ReverseCoordinates(geom);
            }

            return geom;
        }

        public static PgGeom.Geometry ToPostgresGeometry(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = GetGeometryType(geometry);

            PgGeom.Geometry geom = null;

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    geom = new Point(new Coordinate(geometry.STX.Value, geometry.STY.Value));
                    break;
                case OpenGisGeometryType.LineString:
                    geom = new LineString(ToPostgresPoints(geometry));
                    break;
                case OpenGisGeometryType.Polygon:
                    geom = ToPostgresPolygon(geometry);
                    break;
                case OpenGisGeometryType.MultiPoint:
                    geom = new MultiPoint(ToPostgresMultiPoints(geometry));
                    break;
                case OpenGisGeometryType.MultiLineString:
                    geom = ToPostgresMultiLineString(geometry);
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    geom = ToPostgresMultiPolygon(geometry);
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    geom = ToPostgresGeometryCollection(geometry);
                    break;
            }

            if (geom != null)
            {
                geom.SRID = geometry.STSrid.Value;
            }

            return geom;
        }

        private static Coordinate[] ToPostgresPoints(SqlGeometry geometry)
        {
            Coordinate[] points = new Coordinate[geometry.STNumPoints().Value];

            for (int i = 1; i <= points.Length; i++)
            {
                SqlGeometry geom = geometry.STPointN(i);

                points[i - 1] = new Coordinate(geom.STX.Value, geom.STY.Value);
            }
            return points;
        }

        private static Point[] ToPostgresMultiPoints(SqlGeometry geometry)
        {
            Point[] points = new Point[geometry.STNumGeometries().Value];

            for (int i = 1; i <= points.Length; i++)
            {
                points[i - 1] = new Point(new Coordinate(geometry.STPointN(i).STX.Value, geometry.STPointN(i).STY.Value));
            }

            return points;
        }

        private static Polygon ToPostgresPolygon(SqlGeometry geometry)
        {
            LinearRing exterior = new LinearRing(ToPostgresPoints(geometry.STExteriorRing()));

            LinearRing[] interior = null;

            if (geometry.STNumInteriorRing() > 0)
            {
                interior = new LinearRing[geometry.STNumInteriorRing().Value];

                for (int i = 1; i <= interior.Length; i++)
                {
                    interior[i - 1] = new LinearRing(ToPostgresPoints(geometry.STInteriorRingN(i)));
                }
            }

            return new Polygon(exterior, interior);
        }

        private static MultiPolygon ToPostgresMultiPolygon(SqlGeometry geometry)
        {
            Polygon[] polygons = new Polygon[geometry.STNumGeometries().Value];

            for (var i = 1; i <= polygons.Length; i++)
            {
                polygons[i - 1] = ToPostgresPolygon(geometry.STGeometryN(i));
            }

            return new MultiPolygon(polygons);
        }

        private static MultiLineString ToPostgresMultiLineString(SqlGeometry geometry)
        {
            LineString[] lineStrings = new LineString[geometry.STNumGeometries().Value];

            for (int i = 1; i <= lineStrings.Length; i++)
            {
                lineStrings[i - 1] = new LineString(ToPostgresPoints(geometry.STGeometryN(i)));
            }

            return new MultiLineString(lineStrings);
        }

        private static GeometryCollection ToPostgresGeometryCollection(SqlGeometry geometry)
        {
            PgGeom.Geometry[] geoms = new PgGeom.Geometry[geometry.STNumGeometries().Value];

            for (int i = 1; i <= geoms.Length; i++)
            {
                geoms[i - 1] = ToPostgresGeometry(geometry.STGeometryN(i));
            }

            return new GeometryCollection(geoms);
        }

        public static PgGeom.Geometry ToPostgresGeometry(string wkt)
        {
            return ToPostgresGeometry(SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(wkt as string), 0));
        }

        public static PgGeom.Geometry ToPostgresGeography(string wkt)
        {
            PgGeom.Geometry geom = ToPostgresGeometry(wkt);

            if (geom != null)
            {
                if (PostgresGeometryHelper.CanConvertToGeography(geom))
                {
                    PostgresGeometryHelper.ReverseCoordinates(geom);
                }
            }

            return geom;
        }
        #endregion

        #region MySql
        public static MySqlGeometry ToMySqlGeometry(SqlGeometry geometry)
        {
            return MySqlGeometry.FromMySql(ToMySqlBytes(geometry));
        }

        public static byte[] ToMySqlBytes(SqlGeometry geometry, bool isBeginning = true)
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

            Action<int> AppendToByteList = (value) =>
            {
                bytes.AddRange(BitConverter.GetBytes(value));
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
                    AppendToByteList((int)geometryType);
                    AppendPointsToByteList(geometry.STNumPoints().Value, geometry);
                    break;
                case OpenGisGeometryType.LineString:
                    AppendToByteList((int)geometryType);
                    AppendToByteList(ComparePointNum(geometryType, geometry.STNumPoints().Value));
                    AppendPointsToByteList(geometry.STNumPoints().Value, geometry);
                    break;
                case OpenGisGeometryType.Polygon:
                    int interiorRingNum = geometry.STNumInteriorRing().Value;

                    AppendToByteList((int)geometryType);
                    AppendToByteList((geometry.STNumGeometries().Value + interiorRingNum));

                    int exteriorPointNum = geometry.STExteriorRing().STNumPoints().Value;
                    var cpm = ComparePointNum(geometryType, exteriorPointNum);

                    AppendToByteList(cpm);
                    AppendPointsToByteList(cpm, geometry.STExteriorRing());

                    for (int i = 1; i <= interiorRingNum; i++)
                    {
                        var geom = geometry.STInteriorRingN(i);

                        cpm = ComparePointNum(geometryType, geom.STNumPoints().Value);

                        AppendToByteList(cpm);
                        AppendPointsToByteList(cpm, geom);
                    }
                    break;
                case OpenGisGeometryType.MultiPoint:
                    int pointNum = geometry.STNumGeometries().Value;
                    AppendToByteList((int)geometryType); //type
                    AppendToByteList(pointNum); //point num

                    for (int i = 1; i <= pointNum; i++)
                    {
                        SqlGeometry geom = geometry.STPointN(i);

                        bytes.AddRange(ToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.MultiLineString:
                    int linestringNum = geometry.STNumGeometries().Value;
                    AppendToByteList((int)geometryType); //type
                    AppendToByteList(linestringNum); //linestring num

                    for (int i = 1; i <= linestringNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(ToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    int polygonNum = geometry.STNumGeometries().Value;
                    AppendToByteList((int)geometryType); //type
                    AppendToByteList(polygonNum); //polygon num

                    for (int i = 1; i <= polygonNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(ToMySqlBytes(geom, false));
                    }
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    int geometryNum = geometry.STNumGeometries().Value;
                    AppendToByteList((int)geometryType); //type
                    AppendToByteList(geometryNum); //geometry num

                    for (int i = 1; i <= geometryNum; i++)
                    {
                        SqlGeometry geom = geometry.STGeometryN(i);

                        bytes.AddRange(ToMySqlBytes(geom, false));
                    }
                    break;
            }

            return bytes.ToArray();
        }

        public static MySqlGeometry ToMySqlGeometry(string wkt)
        {
            return ToMySqlGeometry(SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(wkt as string), 0));
        }
        #endregion

        #region Oracle
        public static StGeometry ToOracleStGeometry(SqlGeometry geometry)
        {
            if (!geometry.IsNull)
            {
                SdoGeometry sg = ToOracleSdoGeometry(geometry);

                if (sg != null)
                {
                    return new StGeometry() { Geom = sg };
                }
            }

            return null;
        }

        public static SdoGeometry ToOracleSdoGeometry(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = GetGeometryType(geometry);

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                    int type = geometry.HasZ ? 3001 : 2001;
                    return new SdoGeometry()
                    {
                        Type = type,
                        Srid = geometry.STSrid.Value,
                        Point = new SdoPoint()
                        {
                            X = (decimal)geometry.STX.Value,
                            Y = (decimal)geometry.STY.Value,
                            Z = (geometry.HasZ && !geometry.Z.IsNull) ? (decimal)geometry.Z.Value : default(decimal?)
                        }
                    };
                case OpenGisGeometryType.LineString:
                    return ToOracleLineString(geometry);
                case OpenGisGeometryType.Polygon:
                    return ToOraclePolygon(geometry);
                case OpenGisGeometryType.MultiPoint:
                    return ToOracleMultiPoints(geometry);
                case OpenGisGeometryType.MultiLineString:
                    return ToOracleMultiLineString(geometry);
                case OpenGisGeometryType.MultiPolygon:
                    return ToOracleMultiPolygon(geometry);
                case OpenGisGeometryType.GeometryCollection:
                    return ToOracleGeometryCollection(geometry);
            }

            return null;
        }

        internal static decimal[] GetOracleSdoElemInfo(int offset, int type, int interpretation = 1)
        {
            return new decimal[] { offset, type, interpretation };
        }

        internal static decimal[] GetOracleSdoOrdinates(SqlGeometry geometry)
        {
            int pointNum = geometry.STNumPoints().Value;

            List<double> list = new List<double>();

            for (int i = 1; i <= pointNum; i++)
            {
                var point = geometry.STPointN(i);

                list.Add(point.STX.Value);
                list.Add(point.STY.Value);

                if (geometry.HasZ)
                {
                    list.Add(point.Z.Value);
                }
            }

            return list.Select(item => (decimal)item).ToArray();
        }

        private static SdoGeometry ToOracleMultiPoints(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3005 : 2005;
            sg.Srid = geometry.STSrid.Value;
            sg.ElemInfo = GetOracleSdoElemInfo(1, 1, 2);
            sg.Ordinates = GetOracleSdoOrdinates(geometry);

            return sg;
        }

        private static SdoGeometry ToOracleLineString(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3006 : 2006;
            sg.Srid = geometry.STSrid.Value;
            sg.ElemInfo = GetOracleSdoElemInfo(1, 2);
            sg.Ordinates = GetOracleSdoOrdinates(geometry);

            return sg;
        }

        private static SdoGeometry ToOracleMultiLineString(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3002 : 2002;
            sg.Srid = geometry.STSrid.Value;

            int numOfEachPoint = geometry.HasZ ? 3 : 2;
            int geomNum = geometry.STNumGeometries().Value;

            List<decimal[]> list = new List<decimal[]>();

            int offset = 1;

            for (int i = 1; i <= geomNum; i++)
            {
                list.Add(GetOracleSdoElemInfo(offset, 2));

                offset += geometry.STGeometryN(i).STNumPoints().Value * numOfEachPoint;
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geometry);

            return sg;
        }

        private static SdoGeometry ToOraclePolygon(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3003 : 2003;
            sg.Srid = geometry.STSrid.Value;

            int numOfEachPoint = geometry.HasZ ? 3 : 2;

            List<decimal[]> list = new List<decimal[]>();

            list.Add(GetOracleSdoElemInfo(1, 1003));

            var exterior = geometry.STExteriorRing();

            int exteriorLength = exterior.STNumPoints().Value * numOfEachPoint;

            var interiorNum = geometry.STNumInteriorRing().Value;

            int offset = 1 + exteriorLength;

            for (int i = 1; i <= interiorNum; i++)
            {
                list.Add(GetOracleSdoElemInfo(offset, 2003));

                offset += geometry.STInteriorRingN(i).STNumPoints().Value * numOfEachPoint;
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geometry);

            return sg;
        }

        private static SdoGeometry ToOracleMultiPolygon(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3007 : 2007;
            sg.Srid = geometry.STSrid.Value;

            int numOfEachPoint = geometry.HasZ ? 3 : 2;

            List<decimal[]> list = new List<decimal[]>();

            int geomNum = geometry.STNumGeometries().Value;

            int offset = 1;

            for (int i = 1; i <= geomNum; i++)
            {
                var geom = geometry.STGeometryN(i);

                list.Add(GetOracleSdoElemInfo(offset, 1003));

                var exterior = geom.STExteriorRing();

                int exteriorLength = exterior.STNumPoints().Value * numOfEachPoint;

                var interiorNum = geom.STNumInteriorRing().Value;

                offset += exteriorLength;

                for (int j = 1; j <= interiorNum; j++)
                {
                    list.Add(GetOracleSdoElemInfo(offset, 2003));

                    offset += geom.STInteriorRingN(j).STNumPoints().Value * numOfEachPoint;
                }
            }

            sg.ElemInfo = list.SelectMany(item => item).ToArray();
            sg.Ordinates = GetOracleSdoOrdinates(geometry);

            return sg;
        }

        private static SdoGeometry ToOracleGeometryCollection(SqlGeometry geometry)
        {
            SdoGeometry sg = new SdoGeometry();

            sg.Type = geometry.HasZ ? 3004 : 2004;
            sg.Srid = geometry.STSrid.Value;

            List<decimal[]> elemInfos = new List<decimal[]>();
            List<decimal[]> ordinates = new List<decimal[]>();

            int geomNum = geometry.STNumGeometries().Value;

            int count = 0;

            for (int i = 1; i <= geomNum; i++)
            {
                var geom = ToOracleSdoGeometry(geometry.STGeometryN(i));

                geom.ElemInfo[0] = count + 1;

                count += geom.Ordinates.Length;

                elemInfos.Add(geom.ElemInfo);
                ordinates.Add(geom.Ordinates);
            }

            sg.ElemInfo = elemInfos.SelectMany(item => item).ToArray();
            sg.Ordinates = ordinates.SelectMany(item => item).ToArray();

            return sg;
        }
        #endregion
    }
}
