using Oracle.ManagedDataAccess.Types;
using System;
using System.Text;

namespace DatabaseInterpreter.Geometry
{
    [OracleCustomTypeMappingAttribute("MDSYS.SDO_POINT_TYPE")]
    public class SdoPoint : OracleCustomTypeBase<SdoPoint>
    {
        private string wkt;

        [OracleObjectMappingAttribute("X")]
        public decimal? X { get; set; }

        [OracleObjectMappingAttribute("Y")]
        public decimal? Y { get; set; }

        [OracleObjectMappingAttribute("Z")]
        public decimal? Z { get; set; }

        public override void MapFromCustomObject()
        {
            this.SetValue("X", this.X);
            this.SetValue("Y", this.Y);
            this.SetValue("Z", this.Z);
        }

        public override void MapToCustomObject()
        {
            this.X = this.GetValue<decimal?>("X");
            this.Y = this.GetValue<decimal?>("Y");
            this.Z = this.GetValue<decimal?>("Z");
        }

        public string GetWKT()
        {
            if (this.wkt != null)
            {
                return this.wkt;
            }

            if (this == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("POINT");

                if (this.X == null)
                {
                    sb.Append("EMPTY");
                }
                else
                {
                    string value = $"{GeometryUtility.ToInvariantString(this.X)} {GeometryUtility.ToInvariantString(this.Y)} {GeometryUtility.ToInvariantString(this.Z)}".Trim();

                    sb.Append($"({value})");
                }

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            if (this.wkt == null)
            {
                this.wkt = this.GetWKT();
            }

            return this.wkt;
        }
    }
}