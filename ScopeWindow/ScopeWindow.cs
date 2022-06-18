using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using DGScope.Library;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using OpenTK.Mathematics;

namespace ScopeWindow
{
    public class ScopeWindow : GameWindow
    {
        private string adaptationfilename;
        Matrix4 rotationmatrix;
        Matrix4 screenmatrix;
        Matrix4 viewmatrix;
        Matrix4 projectionmatrix;
        [JsonIgnore]
        public Facility Facility { get; set; } = new Facility();
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public PrefSet CurrentPrefSet { get; set; }
        public string AdaptationFileName
        {
            get => adaptationfilename;
            set
            {
                try
                {
                    Facility.Adaptation = Adaptation.DeserializeFromJsonFile(value);
                    adaptationfilename = value;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
        }
        public WSType WorkstationType { get; set; }

        GeoPoint Center
        {
            get
            {
                if (CurrentPrefSet.DisplayCenter != null)
                    return CurrentPrefSet.DisplayCenter;
                return Facility.Adaptation.FacilityCenter;
            }
        }
        GeoPoint? RangeRingCenter => CurrentPrefSet.RangeRingCenter;
        RadarSite RadarSite => Facility.Adaptation.RadarSites.Where(x => x.ID == CurrentPrefSet.RadarSite).FirstOrDefault();
        Adaptation Adaptation => Facility.Adaptation;
        ColorSet ColorSet
        {
            get
            {
                switch (WorkstationType)
                {
                    case (WSType.TCW):
                        return Adaptation.TCWColors;
                    default:
                        return Adaptation.TDWColors;
                }
            }
        }
        ScopeText text;
        public ScopeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, ScopeWindowSettings scopeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            AdaptationFileName = scopeWindowSettings.AdaptationFileName;
            WorkstationType = scopeWindowSettings.WSType;
            text = new ScopeText()
            {
                Font = new System.Drawing.Font("Consolas", 12),
                Color = ScopeColor.BlinkingCyan,
                Text = "Go Fuck Yourself",
                Location = new System.Drawing.PointF(0, 0)
            };
        }
        protected override void OnLoad()
        {
            Shader = new Shader(@"..\vertexshader.txt", @"..\fragmentshader.txt");
            //ScopeGraphics.GenerateVideoMapVertexBuffer(Adaptation.VideoMaps);
            base.OnLoad();
            if (CurrentPrefSet == null)
                CurrentPrefSet = new PrefSet(Adaptation);
            CurrentPrefSet.DisplayedMaps.Add(29);
            CurrentPrefSet.DisplayedMaps.Add(139);
            CurrentPrefSet.DisplayedMaps.Add(206);
            CurrentPrefSet.DisplayCenter = new GeoPoint(41.9390322, -72.6843158);
            new BriteForm(CurrentPrefSet.BrightnessSettings).Show();
            new AdaptationForm(this).Show();
            new PropertyForm(this).Show();

        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.OffsetY > 0 && CurrentPrefSet.DisplayRange > 1)
                CurrentPrefSet.DisplayRange--;
            else
                CurrentPrefSet.DisplayRange++; 
            base.OnMouseWheel(e);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            Matrix2 screenscale = Matrix2.CreateScale(2f / ClientSize.X, 2f / ClientSize.X);
            Matrix2 projection = Matrix2.CreateScale((float)(geoScale * longitudeScale), (float)geoScale);
            Matrix2 rotation = Matrix2.CreateRotation((float)MathHelper.DegreesToRadians(Adaptation.MagVar));
            projection.Invert();
            if (MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right))
            {
                var change = e.Delta * screenscale * rotation * projection;
                Center.Latitude += change.Y;
                Center.Longitude -= change.X;
            }
            base.OnMouseMove(e);
        }
        int templine = 0;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Vector2 mousepoint = new Vector2(2f * MouseState.X / ClientSize.X - 1, 1 -  2f * MouseState.Y / ClientSize.Y);
            var clickedpoint = ScreenToGeoPoint(mousepoint);
            // TEMP DRAW LINE
            if (templine == 0)
                templine = GL.GenBuffer();
            double[] vertexarray = new double[] { clickedpoint.Latitude, clickedpoint.Longitude, 0, Center.Longitude, Center.Latitude, 0 };
            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, templine);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexarray.Length * sizeof(double), vertexarray, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, 3 * sizeof(double), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
            // TEMP DRAW LINE


            base.OnMouseDown(e);
        }
        private GeoPoint ScreenToGeoPoint(Vector2 Point)
        {
            double r = Math.Sqrt(Math.Pow(Point.X, 2) + Math.Pow(Point.Y / aspect_ratio, 2));
            double angle = Math.Atan((Point.Y / aspect_ratio) / Point.X);
            if (Point.X < 0)
                angle += Math.PI;
            double bearing = 90 - (angle * 180 / Math.PI) + Adaptation.MagVar;
            double distance = r * CurrentPrefSet.DisplayRange;
            return Center.FromPoint(distance, bearing);
        }
        protected override void OnUnload()
        {
            Shader.Dispose();
            base.OnUnload();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,ClientRectangle.Size.X, ClientRectangle.Size.Y);
        }

        private int fps = 0;
        private double longitudeScale = 0;
        private double geoScale = 0;
        private double pixelScale = 0;
        private float aspect_ratio = 1;

        protected override void OnRenderFrame(FrameEventArgs obj)
        {
            fps = (int)(1f / obj.Time);
            longitudeScale = Math.Cos(MathHelper.DegreesToRadians(Center.Latitude));
            geoScale = 60.0 / CurrentPrefSet.DisplayRange;
            pixelScale = 2.0 / ClientSize.X;
            aspect_ratio = (float)ClientSize.X / (float)ClientSize.Y;
            var rotationangle = MathHelper.DegreesToRadians(Adaptation.MagVar);
            rotationmatrix = Matrix4.CreateRotationZ((float)rotationangle);
            screenmatrix = Matrix4.CreateScale(1, aspect_ratio, 1);
            viewmatrix = Matrix4.CreateTranslation((float)-Center.Longitude, (float)-Center.Latitude, 0);
            projectionmatrix = Matrix4.CreateScale((float)(geoScale * longitudeScale), (float)geoScale, 1);
            Shader.SetMatrix4("rotation", rotationmatrix);
            Shader.SetMatrix4("screen", screenmatrix);
            Shader.SetMatrix4("view", viewmatrix);
            Shader.SetMatrix4("projection", projectionmatrix);
            var backColor = ColorSet.Background.GetColor(CurrentPrefSet.BrightnessSettings.BKC);
            if (backColor.A > 0)
                GL.ClearColor(ColorSet.Background.GetColor(CurrentPrefSet.BrightnessSettings.BKC));
            else
                GL.ClearColor(ScopeColor.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            DrawWeather();
            DrawVideoMaps();
            if (templine == 0)
                templine = GL.GenBuffer();
            DrawLines(templine, 1, Color.White);
            GL.Flush();
            Context.SwapBuffers();
            base.OnRenderFrame(obj);
        }

        public enum WSType
        {
            TCW,
            TDW
        }

        public static string SerializeToJson(ScopeWindow scopeWindow)
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            return JsonSerializer.Serialize(scopeWindow, options);
        }
        public static ScopeWindow DeserializeFromJson(string jsonString)
        {
            return (ScopeWindow)JsonSerializer.Deserialize(jsonString, typeof(ScopeWindow));
        }
        public static ScopeWindow DeserializeFromJsonFile(string filename)
        {
            string json = File.ReadAllText(filename);
            var result = DeserializeFromJson(json);
            return result;
        }
        public static void SerializeToJsonFile(ScopeWindow scopeWindow, string filename)
        {
            File.WriteAllText(filename, SerializeToJson(scopeWindow));
        }
        public static Shader Shader;
        public int[] wxVertexBuffers;
        public int[] wxVAOs = new int[Constants.MAX_WX_LEVELS];
        public void DrawWeather()
        {
            var wx = Adaptation.WeatherProcessor;
            if (wxVertexBuffers == null)
            {
                wxVertexBuffers = new int[wx.Levels.Length];
                for (int i = 0; i < wxVertexBuffers.Length; i++)
                {
                    wxVertexBuffers[i] = GL.GenBuffer();
                }
            }
            for (int i = 0; i < wxVertexBuffers.Length; i++)
            {
                wxVAOs[i] = GL.GenVertexArray();
                var vertexarray = wx.GetPolygons(i);
                
                if (vertexarray.Length > 0)
                {
                    GL.BindVertexArray(wxVAOs[i]);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, wxVertexBuffers[i]);
                    GL.BufferData(BufferTarget.ArrayBuffer, vertexarray.Length * sizeof(double), vertexarray, BufferUsageHint.DynamicDraw);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, 3 * sizeof(double), 0);
                    
                    GL.EnableVertexAttribArray(0);
                    GL.BindVertexArray(0);
                    DrawQuads(wxVAOs[i], vertexarray.Length / 3, ColorSet.WXColors[i].GetColor(CurrentPrefSet.BrightnessSettings.WX));
                }
            }
        }

        public void DrawVideoMaps()
        {
            var maps = Adaptation.VideoMaps;
            GenerateVideoMapVertexBuffer(maps);
            foreach (var map in maps.Where(x => x.Category == MapCategory.B))
            {
                if (CurrentPrefSet.DisplayedMaps.Contains(map.Number))
                    DrawVideoMap(map, ColorSet.MapBColor.GetColor(CurrentPrefSet.BrightnessSettings.MPB));
            }
            foreach (var map in maps.Where(x => x.Category == MapCategory.A))
            {
                if (CurrentPrefSet.DisplayedMaps.Contains(map.Number))
                    DrawVideoMap(map, ColorSet.MapAColor.GetColor(CurrentPrefSet.BrightnessSettings.MPA));
            }
        }

        public static void Init()
        {
            
        }



        public static void GenerateVideoMapVertexBuffer(VideoMapList maps)
        {
            foreach (var map in maps.Where(x => x.VertexBuffer == 0))
            {
                List<double> vertices = new List<double>();
                foreach (var line in map.Lines)
                {
                    vertices.Add(line.End1.Longitude);
                    vertices.Add(line.End1.Latitude);
                    vertices.Add(0.0f);
                    vertices.Add(line.End2.Longitude);
                    vertices.Add(line.End2.Latitude);
                    vertices.Add(0.0f);
                }
                var vertexarray = vertices.ToArray();
                map.VertexBuffer = GL.GenBuffer();
                var vertexArrayObject = GL.GenVertexArray();
                GL.BindVertexArray(vertexArrayObject);
                GL.BindBuffer(BufferTarget.ArrayBuffer, map.VertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexarray.Length * sizeof(double), vertexarray, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, 3 * sizeof(double), 0);
                GL.EnableVertexAttribArray(0);
                GL.BindVertexArray(0);
            }
        }

        private void DrawVideoMap(VideoMap map, Color color)
        {
            DrawLines(map.VertexBuffer, map.Lines.Count, color);
        }

        private void DrawLines(int vertexBuffer, int lineCount, Color color)
        {
            Shader.Use();
            Shader.SetColor(GetColor4(color));

            GL.BindVertexArray(vertexBuffer);
            GL.DrawArrays(PrimitiveType.Lines, 0, lineCount * 2);
            GL.BindVertexArray(0);
        }

        private void DrawQuads(int vertexBuffer, int points, Color color)
        {
            Shader.Use();
            Shader.SetColor(GetColor4(color));

            GL.BindVertexArray(vertexBuffer);
            GL.DrawArrays(PrimitiveType.Quads, 0, points);
            GL.BindVertexArray(0);
        }

        private static Color4 GetColor4(Color color)
        {
            var r = color.R / 255f;
            var g = color.G / 255f;
            var b = color.B / 255f;
            var a = color.A / 255f;
            return new Color4(r, g, b, a);
        }

    }

    
}
