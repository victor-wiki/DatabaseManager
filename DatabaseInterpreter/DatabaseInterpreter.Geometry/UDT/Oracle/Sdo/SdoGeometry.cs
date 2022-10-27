using Microsoft.SqlServer.Types;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseInterpreter.Geometry
{
    [OracleCustomTypeMappingAttribute("MDSYS.SDO_GEOMETRY")]
    public class SdoGeometry : OracleCustomTypeBase<SdoGeometry>
    {
        private string text;
        private List<SdoElemInfoItem> elemInfoItems;

        [OracleObjectMappingAttribute(0)]
        public int Type { get; set; }

        [OracleObjectMappingAttribute(1)]
        public int Srid { get; set; }

        [OracleObjectMappingAttribute(2)]
        public SdoPoint Point { get; set; }

        [OracleObjectMappingAttribute(3)]
        public decimal[] ElemInfo { get; set; }

        [OracleObjectMappingAttribute(4)]
        public decimal[] Ordinates { get; set; }

        public override void MapFromCustomObject()
        {
            this.SetValue((int)OracleSdoGeometryColumns.SDO_GTYPE, this.Type);
            this.SetValue((int)OracleSdoGeometryColumns.SDO_SRID, this.Srid);
            this.SetValue((int)OracleSdoGeometryColumns.SDO_POINT, this.Point);
            this.SetValue((int)OracleSdoGeometryColumns.SDO_ELEM_INFO, this.ElemInfo);
            this.SetValue((int)OracleSdoGeometryColumns.SDO_ORDINATES, this.Ordinates);
        }

        public override void MapToCustomObject()
        {
            this.Type = this.GetValue<int>((int)OracleSdoGeometryColumns.SDO_GTYPE);
            this.Srid = this.GetValue<int>((int)OracleSdoGeometryColumns.SDO_SRID);
            this.Point = this.GetValue<SdoPoint>((int)OracleSdoGeometryColumns.SDO_POINT);
            this.ElemInfo = this.GetValue<decimal[]>((int)OracleSdoGeometryColumns.SDO_ELEM_INFO);
            this.Ordinates = this.GetValue<decimal[]>((int)OracleSdoGeometryColumns.SDO_ORDINATES);

            this.SetElemInfoItems();
        }

        private void SetElemInfoItems()
        {
            if (this.ElemInfo == null)
            {
                return;
            }

            this.elemInfoItems = new List<SdoElemInfoItem>();

            var groups = this.ElemInfo.Select((x, i) => new { Index = i, Value = (int)x })
                                      .GroupBy(x => x.Index / 3);

            foreach (var g in groups)
            {
                var items = g.ToList();

                this.elemInfoItems.Add(new SdoElemInfoItem() { Offset = items[0].Value, Type = items[1].Value, Interpretation = items[2].Value });
            }
        }

        public override string ToString()
        {
            if (this.text == null)
            {
                this.text = this.GetText();
            }

            return this.text;
        }

        /// <summary>
        /// refer to: https://datacadamia.com/oracle_spatial/geometry#about
        /// or: https://docs.oracle.com/database/121/SPATL/sdo_geometry-object-type.htm#SPATL494
        /// </summary> 
        public string GetText()
        {
            if (this == null)
            {
                throw new NullReferenceException("Cannot write Well-Known Text: geometry was null");
            }

            if (this.text != null)
            {
                return this.text;
            }           

            if (this.Point != null && this.ElemInfo == null && this.Ordinates == null) //point
            {
                return this.Point.ToString();
            }

            StringBuilder sb = new StringBuilder();

            int type = (int)this.Type;
            string typeName = "";
            string content = "";

            if (this.Type == 2002 || this.Type == 3002)
            {
                typeName = nameof(OpenGisGeometryType.LineString);
                content = this.GetLineString(type);
            }
            else if (this.Type == 2003 || this.Type == 3003)
            {
                typeName = nameof(OpenGisGeometryType.Polygon);
                content = this.GetPolygon(type);
            }
            else if (this.Type == 2004 || this.Type == 3004)
            {
                typeName = nameof(OpenGisGeometryType.GeometryCollection);
                content = this.GetGeometryCollection(type);
            }
            else if (this.Type == 2005 || this.Type == 3005)
            {
                typeName = nameof(OpenGisGeometryType.MultiPoint);
                content = this.GetMultiPoints(type);
            }
            else if (this.Type == 2006 || this.Type == 3006)
            {
                typeName = nameof(OpenGisGeometryType.MultiLineString);
                content = this.GetMultiLineStrings(type);
            }
            else if (this.Type == 2007 || this.Type == 3007)
            {
                typeName = nameof(OpenGisGeometryType.MultiPolygon);
                content = this.GetMultiPolygons(type);
            }

            sb.Append($"{typeName.ToUpper()}({content})");

            return sb.ToString();
        }

        private IEnumerable<IEnumerable<string>> GetOrdinatesGroups(IEnumerable<decimal> ordinates, int chunkSize)
        {
            return ordinates.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / chunkSize).Select(x => x.Select(v => GeometryUtility.ToInvariantString(v.Value)));
        }

        private string JoinContents(IEnumerable<SdoElemInfoItemContent> contents)
        {
            return string.Join(", ", contents.Select(item => item.Content));
        }

        private string GetLineString(int type)
        {
            return this.GetSimpleOrdinatesValues(this.Ordinates, this.GetPointNumOfChunk(type));
        }

        private string GetPolygon(int type)
        {
            return this.JoinContents(this.GetOrdinatesContentsByElemInfo(type));
        }

        private string GetMultiPoints(int type)
        {
            int chunkSize = this.GetPointNumOfChunk(type);

            var groups = this.GetOrdinatesGroups(this.Ordinates, chunkSize);

            var items = groups.Select(item => String.Join(" ", item));

            return $"{string.Join(", ", items.Select(item => $"({item})"))}";
        }

        private string GetMultiLineStrings(int type)
        {
            return this.JoinContents(this.GetOrdinatesContentsByElemInfo(type));
        }

        private string GetMultiPolygons(int type)
        {
            var contents = this.GetOrdinatesContentsByElemInfo(type);

            List<List<string>> groupContents = new List<List<string>>();

            foreach (var content in contents)
            {
                int subType = content.Type;

                if (subType == 1003)
                {
                    groupContents.Add(new List<string>() { content.Content });
                }
                else if (subType == 2003)
                {
                    groupContents.Last().Add(content.Content);
                }
            }

            return String.Join(", ", groupContents.Select(item => $"({string.Join(", ", item)})"));
        }

        private string GetGeometryCollection(int type)
        {
            var contents = this.GetOrdinatesContentsByElemInfo(type);

            List<SdoGeomCollectionItem> groupContents = new List<SdoGeomCollectionItem>();

            int? lastType = default(int?);

            foreach (var content in contents)
            {
                int subType = content.Type;

                SdoGeomCollectionItem geoItem = new SdoGeomCollectionItem();

                if (subType == 1)
                {
                    if (lastType == 1)
                    {
                        groupContents.Last().TypeName = nameof(OpenGisGeometryType.MultiPoint).ToUpper();
                        groupContents.Last().Contents.Add(content.Content);
                    }
                    else
                    {
                        geoItem.TypeName = nameof(OpenGisGeometryType.Point).ToUpper();
                        geoItem.Contents.Add(content.Content);
                    }
                }
                else if (subType == 2)
                {
                    if (lastType == 2)
                    {
                        groupContents.Last().TypeName = nameof(OpenGisGeometryType.MultiLineString).ToUpper();
                        groupContents.Last().Contents.Add(content.Content);
                    }
                    else
                    {
                        geoItem.TypeName = nameof(OpenGisGeometryType.LineString).ToUpper();
                        geoItem.Contents.Add(content.Content);
                    }
                }
                else if (subType == 1003)
                {
                    geoItem.TypeName = nameof(OpenGisGeometryType.Polygon).ToUpper();
                    geoItem.Contents.Add(content.Content);
                }
                else if (subType == 2003)
                {
                    groupContents.Last().TypeName = nameof(OpenGisGeometryType.MultiPolygon).ToUpper();
                    groupContents.Last().Contents.Add(content.Content);
                }

                lastType = subType;

                if (geoItem.Contents.Count > 0)
                {
                    groupContents.Add(geoItem);
                }
            }

            return String.Join(", ", groupContents.Select(item => $"{item.TypeName}{this.GetGeomItemValues(item)}"));
        }

        private string GetGeomItemValues(SdoGeomCollectionItem item)
        {
            bool isSimpleGeom = item.TypeName == nameof(OpenGisGeometryType.Point).ToUpper() || item.TypeName == nameof(OpenGisGeometryType.LineString).ToUpper();

            string content = $"{string.Join(", ", item.Contents)}";

            if (!isSimpleGeom)
            {
                content = $"({content})";
            }

            return content;
        }

        private string GetSimpleOrdinatesValues(IEnumerable<decimal> ordinates, int chunkSize)
        {
            var groups = this.GetOrdinatesGroups(ordinates, chunkSize);

            var items = groups.Select(item => String.Join(" ", item));

            return $"{string.Join(", ", items)}";
        }

        private List<SdoElemInfoItemContent> GetOrdinatesContentsByElemInfo(int type)
        {
            int chunkSize = this.GetPointNumOfChunk(type);

            int count = 0;

            List<SdoElemInfoItemContent> contents = new List<SdoElemInfoItemContent>();

            foreach (SdoElemInfoItem item in this.elemInfoItems)
            {
                count++;

                int start = item.Offset;
                int? end = count < this.elemInfoItems.Count ? this.elemInfoItems[count].Offset : default(int?);

                int length = end.HasValue ? (end.Value - start) : (this.Ordinates.Length - (start - 1));

                var points = this.Ordinates.Skip(start - 1).Take(length);

                string content = $"({this.GetSimpleOrdinatesValues(points, chunkSize)})";

                contents.Add(new SdoElemInfoItemContent() { Type = item.Type, Content = content });
            }

            return contents;
        }

        private int GetPointNumOfChunk(int type)
        {
            return int.Parse(type.ToString().Substring(0, 1));
        }
    }

    public struct SdoElemInfoItem
    {
        public int Offset { get; set; }
        public int Type { get; set; }
        public int Interpretation { get; set; }
    }

    public struct SdoElemInfoItemContent
    {
        public int Type { get; set; }
        public string Content { get; set; }
    }

    public class SdoGeomCollectionItem
    {
        public string TypeName { get; set; }
        public List<string> Contents { get; set; } = new List<string>();
    }

    [OracleCustomTypeMappingAttribute("MDSYS.SDO_ELEM_INFO_ARRAY")]
    public class SdoElemArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

    [OracleCustomTypeMappingAttribute("MDSYS.SDO_ORDINATE_ARRAY")]
    public class SdoOrdinatesArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

    public enum OracleSdoGeometryColumns
    {
        SDO_GTYPE,
        SDO_SRID,
        SDO_POINT,
        SDO_ELEM_INFO,
        SDO_ORDINATES
    }
}
