using DatabaseInterpreter.Geometry;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmWktViewer : Form
    {
        private Color linePenColor = Color.Red;
        private Color polygonPenColor = Color.Green;
        private Pen linePen { get { return new Pen(new SolidBrush(this.linePenColor), 0); } }
        private Pen polygonPen { get { return new Pen(new SolidBrush(this.polygonPenColor), 0); } }

        private GeometryInfo geomInfo;
        private float? scale = default(float?);
        private float defaultLimitMaxScale = 30;
        private bool isSettingZoomBar = false;
        public frmWktViewer()
        {
            InitializeComponent();
        }

        public frmWktViewer(bool isGeography, string content)
        {
            InitializeComponent();

            if (isGeography)
            {
                this.rbGeography.Checked = true;
            }

            if (!string.IsNullOrEmpty(content))
            {
                this.txtContent.Text = content.Trim();

                this.ShowGeometry(content);
            }
        }

        private void frmGeometryViewer_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            string content = this.txtContent.Text.Trim();

            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please enter content.");
                return;
            }

            this.ResetValues();

            this.ShowGeometry(content);
        }

        private void ResetValues()
        {
            this.ResetScale();
            this.geomInfo = null;
        }

        private void ResetScale()
        {
            this.scale = default(float?);
            this.isSettingZoomBar = true;
            this.tbZoom.Value = 0;
            this.isSettingZoomBar = false;
        }

        private GeometryInfo GetGeometryInfo(SqlGeometry geometry)
        {
            OpenGisGeometryType geometryType = SqlGeometryHelper.GetGeometryType(geometry);

            GeometryInfo info = new GeometryInfo() { Type = geometryType };

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                case OpenGisGeometryType.MultiPoint:
                    info.Points.AddRange(this.GetGeometryPoints(geometry));

                    break;
                case OpenGisGeometryType.LineString:
                    info.Points.AddRange(this.GetGeometryPoints(geometry));

                    break;
                case OpenGisGeometryType.Polygon:
                    info.Items = this.GetPolygonItems(geometry);

                    break;
                case OpenGisGeometryType.MultiLineString:
                    int num = geometry.STNumGeometries().Value;

                    for (int i = 1; i <= num; i++)
                    {
                        var geom = geometry.STGeometryN(i);

                        info.Items.Add(new GeometryInfoItem() { Points = this.GetGeometryPoints(geom) });
                    }

                    break;
                case OpenGisGeometryType.MultiPolygon:
                    int polygonNum = geometry.STNumGeometries().Value;

                    for (int i = 1; i <= polygonNum; i++)
                    {
                        info.Collection.Add(new GeometryInfo() { Type = OpenGisGeometryType.Polygon, Items = this.GetPolygonItems(geometry.STGeometryN(i)) });
                    }

                    break;
                case OpenGisGeometryType.GeometryCollection:
                    int geomNum = geometry.STNumGeometries().Value;

                    for (int i = 1; i <= geomNum; i++)
                    {
                        var geom = geometry.STGeometryN(i);

                        info.Collection.Add(this.GetGeometryInfo(geom));
                    }

                    break;
            }

            return info;
        }

        private GeometryInfo GetGeometryInfo(SqlGeography geography)
        {
            OpenGisGeometryType geometryType = SqlGeographyHelper.GetGeometryType(geography);

            GeometryInfo info = new GeometryInfo() { Type = geometryType };

            switch (geometryType)
            {
                case OpenGisGeometryType.Point:
                case OpenGisGeometryType.MultiPoint:
                    info.Points.AddRange(this.GetGeometryPoints(geography));

                    break;
                case OpenGisGeometryType.LineString:
                    info.Points.AddRange(this.GetGeometryPoints(geography));
                    break;

                case OpenGisGeometryType.Polygon:
                    info.Items = this.GetPolygonItems(geography);

                    break;
                case OpenGisGeometryType.MultiLineString:
                    int num = geography.STNumGeometries().Value;

                    for (int i = 1; i <= num; i++)
                    {
                        var geom = geography.STGeometryN(i);

                        info.Items.Add(new GeometryInfoItem() { Points = this.GetGeometryPoints(geom) });
                    }

                    break;
                case OpenGisGeometryType.MultiPolygon:
                    int polygonNum = geography.STNumGeometries().Value;

                    for (int i = 1; i <= polygonNum; i++)
                    {
                        info.Collection.Add(new GeometryInfo() { Type = OpenGisGeometryType.Polygon, Items = this.GetPolygonItems(geography.STGeometryN(i)) });
                    }

                    break;
                case OpenGisGeometryType.GeometryCollection:
                    int geomNum = geography.STNumGeometries().Value;

                    for (int i = 1; i <= geomNum; i++)
                    {
                        var geom = geography.STGeometryN(i);

                        info.Collection.Add(this.GetGeometryInfo(geom));
                    }

                    break;
            }

            return info;
        }

        private List<PointF> GetGeometryPoints(SqlGeometry geometry)
        {
            int pointNum = geometry.STNumPoints().Value;

            List<PointF> points = new List<PointF>();

            for (int i = 1; i <= pointNum; i++)
            {
                SqlGeometry point = geometry.STPointN(i);

                points.Add(new PointF((float)point.STX.Value, (float)point.STY.Value));
            }

            return points;
        }

        private List<PointF> GetGeometryPoints(SqlGeography geography)
        {
            int pointNum = geography.STNumPoints().Value;

            List<PointF> points = new List<PointF>();

            for (int i = 1; i <= pointNum; i++)
            {
                SqlGeography point = geography.STPointN(i);

                points.Add(new PointF((float)point.Long.Value, (float)point.Lat.Value * -1));
            }

            return points;
        }

        private List<GeometryInfoItem> GetPolygonItems(SqlGeometry geometry)
        {
            List<GeometryInfoItem> infoItems = new List<GeometryInfoItem>();

            var exteriorRing = geometry.STExteriorRing();

            List<PointF> exteriorPoints = this.GetGeometryPoints(exteriorRing);

            GeometryInfoItem item = new GeometryInfoItem() { Points = exteriorPoints };

            infoItems.Add(item);

            var interiorNum = geometry.STNumInteriorRing().Value;

            for (int i = 1; i <= interiorNum; i++)
            {
                infoItems.Add(new GeometryInfoItem() { Points = this.GetGeometryPoints(geometry.STInteriorRingN(i)) });
            }

            return infoItems;
        }

        private List<GeometryInfoItem> GetPolygonItems(SqlGeography geography)
        {
            List<GeometryInfoItem> infoItems = new List<GeometryInfoItem>();

            var ringNum = geography.NumRings();

            for (int i = 1; i <= ringNum; i++)
            {
                infoItems.Add(new GeometryInfoItem() { Points = this.GetGeometryPoints(geography.RingN(i)) });
            }

            return infoItems;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowGeometry(string content)
        {
            if (this.rbGeometry.Checked)
            {
                SqlGeometry geom = null;

                try
                {
                    geom = SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(content), 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                if (geom.IsNull)
                {
                    MessageBox.Show("The geometry is null.");
                    return;
                }

                if (!geom.STIsValid())
                {
                    MessageBox.Show("The content is invalid.");
                    return;
                }

                if (geom.STNumPoints().Value == 0)
                {
                    MessageBox.Show("This a empty geometry.");
                    return;
                }

                this.geomInfo = this.GetGeometryInfo(geom);

                this.DrawGeometry(this.geomInfo);
            }
            else if (this.rbGeography.Checked)
            {
                SqlGeography geography = null;

                try
                {
                    geography = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(content), 4326);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                if (geography.IsNull)
                {
                    MessageBox.Show("The geography is null.");
                    return;
                }

                if (!geography.STIsValid())
                {
                    MessageBox.Show("The content is invalid.");
                    return;
                }

                if (geography.STNumPoints().Value == 0)
                {
                    MessageBox.Show("This a empty geography.");
                    return;
                }

                this.geomInfo = this.GetGeometryInfo(geography);

                this.DrawGeometry(this.geomInfo);
            }
        }

        private void DrawGeometry(GeometryInfo info, bool isContinuous = false)
        {
            if (!isContinuous)
            {
                this.DrawCoordinate();
            }

            if (info == null)
            {
                return;
            }

            switch (info.Type)
            {
                case OpenGisGeometryType.Point:
                case OpenGisGeometryType.MultiPoint:
                    this.DrawPoints(info.Points);
                    break;
                case OpenGisGeometryType.LineString:
                    this.DrawLineString(info.Points);
                    break;
                case OpenGisGeometryType.MultiLineString:
                    this.DrawMultiLineString(info);
                    break;
                case OpenGisGeometryType.Polygon:
                    this.DrawPolygon(info);
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    this.DrawMultiPolygon(info);
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    var collection = info.Collection;

                    foreach (GeometryInfo gi in collection)
                    {
                        this.DrawGeometry(gi, true);
                    }
                    break;
            }
        }

        private Graphics GetGraphics()
        {
            var g = Graphics.FromImage(this.picGeometry.Image);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            return g;
        }

        private void DrawCoordinate()
        {
            GeometryViewportInfo viewportInfo = this.GetViewportInfo();

            if (viewportInfo.Width == 0 || viewportInfo.Height == 0)
            {
                return;
            }

            Bitmap img = new Bitmap((int)viewportInfo.Width, (int)viewportInfo.Height);
            Graphics g = Graphics.FromImage(img);

            g.TranslateTransform(viewportInfo.MaxX, viewportInfo.MaxY);

            Pen pen = new Pen(new SolidBrush(Color.LightGray), 1);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            g.DrawLine(pen, -viewportInfo.MaxX, 0, viewportInfo.MaxX, 0);
            g.DrawLine(pen, 0, -viewportInfo.MaxY, 0, viewportInfo.MaxY);

            this.picGeometry.Image = img;

            this.DisposeGraphics(g);
        }

        private GeometryViewportInfo GetViewportInfo()
        {
            return new GeometryViewportInfo() { Width = this.panelContent.ClientSize.Width, Height = this.panelContent.ClientSize.Height };
        }

        private void TranslateTransform(Graphics g)
        {
            GeometryViewportInfo viewport = this.GetViewportInfo();

            g.TranslateTransform(viewport.MaxX, viewport.MaxY);
        }
        private void DrawPoints(List<PointF> points)
        {
            var g = this.GetGraphics();

            this.TranslateTransform(g);

            GeometryViewportInfo viewport = this.GetViewportInfo();

            foreach (var point in points)
            {
                if (Math.Abs(point.X) <= viewport.MaxX && Math.Abs(point.Y) <= viewport.MaxY)
                {
                    Font font = new Font(this.Font.FontFamily, 12, FontStyle.Bold);

                    g.DrawRectangle(new Pen(new SolidBrush(Color.Red), 2), new Rectangle((int)point.X, (int)point.Y, 1, 1));
                }
                else
                {
                    MessageBox.Show($"The point ({point.X},{point.Y}) not in the viewport.");
                }
            }

            this.DisposeGraphics(g);
        }

        private void CheckScale(Graphics g)
        {
            if (this.scale.HasValue && this.scale.Value > 0)
            {
                g.ScaleTransform(this.scale.Value, this.scale.Value);
            }
        }

        private void DrawLineString(List<PointF> points)
        {
            var g = this.GetGraphics();

            this.TranslateTransform(g);

            this.CheckScale(g);

            g.DrawLines(this.linePen, points.ToArray());

            this.DisposeGraphics(g);
        }

        private void DrawMultiLineString(GeometryInfo info)
        {
            var g = this.GetGraphics();

            this.TranslateTransform(g);

            this.CheckScale(g);

            foreach (var item in info.Items)
            {
                g.DrawLines(this.linePen, item.Points.ToArray());
            }

            this.DisposeGraphics(g);
        }

        private void DrawPolygon(GeometryInfo info)
        {
            var g = this.GetGraphics();

            var allPoints = info.Items.SelectMany(item => item.Points).ToList();

            this.TranslateAndScale(g, allPoints);

            int count = 0;

            foreach (var item in info.Items)
            {
                g.DrawPolygon(polygonPen, item.Points.ToArray());

                g.FillPolygon(count == 0 ? this.polygonPen.Brush : new SolidBrush(Color.White), item.Points.ToArray());

                count++;
            }

            this.DisposeGraphics(g);
        }

        private void TranslateAndScale(Graphics g, List<PointF> points)
        {
            float minPointX = points.Min(item => item.X);
            float minPointY = points.Min(item => item.Y);
            float maxPointX = points.Max(item => item.X);
            float maxPointY = points.Max(item => item.Y);

            float maxDistanceX = Math.Abs(maxPointX - minPointX);
            float maxDistanceY = Math.Abs(maxPointY - minPointY);

            bool isNear180X = false;

            if (Math.Abs(minPointX + maxPointX) < Math.Abs(minPointX) + Math.Abs(maxPointX))
            {
                if (this.rbGeography.Checked)
                {
                    isNear180X = points.Any(item => 180 - Math.Abs(maxPointX) <= 10);

                    minPointX = points.Where(item => item.X < 0).Max(item => item.X);
                    maxPointX = points.Where(item => item.X > 0).Min(item => item.X);

                    maxDistanceX = 360 - Math.Abs(minPointX) - Math.Abs(maxPointX);
                }
            }

            if (Math.Abs(minPointY + maxPointY) < Math.Abs(minPointY) + Math.Abs(maxPointY))
            {
                if (this.rbGeography.Checked)
                {
                    minPointY = points.Where(item => item.Y < 0).Max(item => item.Y);
                    maxPointY = points.Where(item => item.Y > 0).Min(item => item.Y);

                    maxDistanceY = 180 - Math.Abs(minPointY) - Math.Abs(maxPointY);
                }
            }

            GeometryViewportInfo viewport = this.GetViewportInfo();

            float scale;

            if (!this.scale.HasValue || this.scale.Value == 0)
            {
                scale = Math.Max(viewport.Width / maxDistanceX, viewport.Height / maxDistanceY);

                if (scale > this.defaultLimitMaxScale)
                {
                    scale = this.defaultLimitMaxScale;
                }
                else
                {
                    scale = scale * 0.7f;
                }
            }
            else
            {
                scale = this.scale.Value;
            }

            if (scale <= 0)
            {
                scale = 1;
            }

            float centerRelativeX = (maxPointX - minPointX) / 2;
            float centerRelativeY = (maxPointY - minPointY) / 2;

            PointF centerRelativePoint = new PointF(centerRelativeX, centerRelativeY);
            PointF centerAbsolutePoint = new PointF(centerRelativePoint.X + minPointX, centerRelativePoint.Y + minPointY);

            if(isNear180X)
            {
                if(centerAbsolutePoint.X + 180 >180)
                {
                    centerAbsolutePoint.X -= 180;
                }
                else
                { 
                    centerAbsolutePoint.X += 180;
                }
            }                

            float translateX = (centerAbsolutePoint.X > 0 ? -1 : 1) * Math.Abs(centerAbsolutePoint.X) * scale + viewport.MaxX;
            float translateY = (centerAbsolutePoint.Y > 0 ? -1 : 1) * Math.Abs(centerAbsolutePoint.Y) * scale + viewport.MaxY;

            g.TranslateTransform(translateX, translateY);

            g.ScaleTransform(scale, scale);

            this.isSettingZoomBar = true;
            this.tbZoom.Value = (int)scale;
            this.isSettingZoomBar = false;
        }

        private void DrawMultiPolygon(GeometryInfo info)
        {
            var g = this.GetGraphics();

            var allPoints = info.Collection.SelectMany(item => item.Items).SelectMany(item => item.Points).ToList();

            this.TranslateAndScale(g, allPoints);

            foreach (var gi in info.Collection)
            {
                int count = 0;

                foreach (var item in gi.Items)
                {
                    g.DrawPolygon(polygonPen, item.Points.ToArray());

                    g.FillPolygon(count == 0 ? this.polygonPen.Brush : new SolidBrush(Color.White), item.Points.ToArray());

                    count++;
                }
            }

            this.DisposeGraphics(g);
        }

        private void DisposeGraphics(Graphics g)
        {
            if (g != null)
            {
                g.Dispose();
            }
        }
        private void rbGeography_CheckedChanged(object sender, EventArgs e)
        {
            this.SwitchMode();
        }
        private void SwitchMode()
        {
            string content = this.txtContent.Text.Trim();

            if (!string.IsNullOrEmpty(content))
            {
                this.ShowGeometry(content);
            }
        }

        private void tbZoom_ValueChanged(object sender, EventArgs e)
        {
            if (this.isSettingZoomBar)
            {
                return;
            }

            this.scale = this.tbZoom.Value;

            this.DrawGeometry(this.geomInfo);
        }

        private void picGeometry_SizeChanged(object sender, EventArgs e)
        {
            if (this.geomInfo != null)
            {
                this.ResetScale();

                this.DrawGeometry(this.geomInfo);
            }
        }
    }

    internal class GeometryInfo
    {
        internal OpenGisGeometryType Type { get; set; }

        internal List<PointF> Points = new List<PointF>();
        internal List<GeometryInfoItem> Items { get; set; } = new List<GeometryInfoItem>();

        internal List<GeometryInfo> Collection = new List<GeometryInfo>();

    }

    internal class GeometryInfoItem
    {
        internal List<PointF> Points = new List<PointF>();
    }

    internal struct GeometryViewportInfo
    {
        internal float Width { get; set; }
        internal float Height { get; set; }
        internal float MaxX { get { return this.Width / 2; } }
        internal float MaxY { get { return this.Height / 2; } }
    }
}
