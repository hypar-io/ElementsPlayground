using System.Threading.Tasks;
using Elements.Geometry;
using Elements.Spatial;
using Microsoft.JSInterop;
using System;
using System.Linq;
using Elements;
using Elements.Serialization.glTF;
using Elements.Geometry.Solids;
using System.Collections.Generic;

namespace ElementsWasm
{
    public static class ElementsInterop
    {
        [JSInvokable]
        public static Task<Byte[]> ModelToBytes(string modelJson)
        {
            Console.WriteLine("starting modelJson");
            Console.WriteLine(DateTime.Now.ToString());
            var model = Model.FromJson(modelJson);
            Console.WriteLine("done with model from Json");
            Console.WriteLine(DateTime.Now.ToString());
            var glb = model.ToGlTF();
            Console.WriteLine("done with model to gltf");
            Console.WriteLine(DateTime.Now.ToString());
            return Task.FromResult<Byte[]>(glb);
        }

        [JSInvokable]
        public static Task<Line[]> CreateGrid2dAsync(Polygon polygon)
        {
            Grid2d grid2d = new Grid2d(polygon);
            grid2d.U.DivideByCount(10);
            grid2d.V.DivideByCount(10);
            return Task.FromResult(Enumerable.ToArray(Enumerable.Cast<Line>(Enumerable.Concat(grid2d.GetCellSeparators(GridDirection.U), grid2d.GetCellSeparators(GridDirection.V)))));
        }

        [JSInvokable]
        public static Line[] CreateGrid2d(Polygon polygon, int uDivisions, int vDivisions)
        {
            Grid2d grid2d = new Grid2d(polygon);
            grid2d.U.DivideByCount(uDivisions);
            grid2d.V.DivideByCount(vDivisions);
            var segments = grid2d.GetCells().SelectMany(c => c.GetTrimmedCellGeometry()).OfType<Polygon>().SelectMany(c => c.Segments()).ToArray();
            return segments;
            // return Enumerable.ToArray(Enumerable.Cast<Line>(Enumerable.Concat(grid2d.GetCellSeparators(GridDirection.U), grid2d.GetCellSeparators(GridDirection.V))));
        }

        [JSInvokable]
        public static Byte[] LoadGlb()
        {
            var model = new Model();
            var mass = new Mass(Polygon.Rectangle(5, 5), 1, BuiltInMaterials.Mass);
            model.AddElement(mass);
            var glbBytes = model.ToGlTF();
            // var str = System.Text.Encoding.ASCII.GetString(glbBytes);
            return glbBytes;
        }

        [JSInvokable]
        public static ExecutionResult EnvelopeBySketch(Polygon sketch, double height)
        {
            if (sketch == null)
            {
                return null;
            }
            var representation = new Representation(new[] { new Extrude(sketch, height, Vector3.ZAxis, false) });
            var envelope = new Envelope(sketch, 0, height, Vector3.ZAxis, 0, new Transform(), BuiltInMaterials.Mass, representation);
            return new ExecutionResult(envelope);
        }

        [JSInvokable]
        public static Task<ExecutionResult> EnvelopeBySketchAsync(Polygon sketch, double height)
        {
            return Task.FromResult(EnvelopeBySketch(sketch, height));
        }

        [JSInvokable]
        public static ExecutionResult LevelsByEnvelope(string envelopeJson, double levelHeight)
        {
            Model model;
            try
            {
                model = Model.FromJson(envelopeJson);
            }
            catch
            {
                return null;
            }
            var envelopes = model.AllElementsOfType<Envelope>();
            var levelsOut = new List<LevelVolume>();
            foreach (var envelope in envelopes)
            {
                var profile = envelope.Profile;
                for (var i = envelope.Elevation; i < envelope.Elevation + envelope.Height; i += levelHeight)
                {
                    var rep = new Representation(new[] { new Extrude(profile, levelHeight, Vector3.ZAxis, false) });
                    var lv = new LevelVolume(profile, levelHeight, profile.Area(), new Transform(0, 0, i), BuiltInMaterials.Glass, rep);
                    levelsOut.Add(lv);
                }
            }
            return new ExecutionResult(levelsOut);
        }

        [JSInvokable]
        public static Task<ExecutionResult> LevelsByEnvelopeAsync(string envelopeJson, double levelHeight)
        {
            return Task.FromResult(LevelsByEnvelope(envelopeJson, levelHeight));
        }

        [JSInvokable]
        public static ExecutionResult FloorsByLevels(string levelsJson, double floorThickness)
        {
            if (levelsJson == null)
            {
                return new ExecutionResult();
            }
            var model = Model.FromJson(levelsJson);
            var levels = model.AllElementsOfType<LevelVolume>();
            var floors = new List<Floor>();
            foreach (var level in levels)
            {
                var floor = new Floor(level.Profile, floorThickness, level.Transform, BuiltInMaterials.Concrete);
                floors.Add(floor);
            }
            return new ExecutionResult(floors);
        }

        [JSInvokable]
        public static Task<ExecutionResult> FloorsByLevelsAsync(string levelsJson, double floorThickness)
        {
            return Task.FromResult(FloorsByLevels(levelsJson, floorThickness));
        }

        [JSInvokable]
        public static ExecutionResult FacadeFromLevels(string levelsJson, double gridSize)
        {
            try
            {

                if (levelsJson == null)
                {
                    return new ExecutionResult();
                }
                var model = Model.FromJson(levelsJson);
                var levels = model.AllElementsOfType<LevelVolume>();
                var outputModel = new Model();
                Dictionary<string, Panel> panels = new Dictionary<string, Panel>();
                Dictionary<string, ModelCurve> panelCrvs = new Dictionary<string, ModelCurve>();
                var rand = new Random();
                foreach (var lv in levels)
                {
                    var height = round(lv.Height);
                    var profile = lv.Profile;
                    var perimeter = profile.Perimeter.Offset(0.1);
                    foreach (var segment in perimeter.SelectMany(p => p.Segments()))
                    {
                        var grid = new Grid1d(segment);
                        grid.DivideByFixedLength(gridSize, FixedDivisionMode.RemainderAtBothEnds);
                        foreach (var cell in grid.GetCells())
                        {
                            var width = round(cell.Domain.Length);
                            var line = cell.GetCellGeometry() as Line;
                            var key = $"{width:0.00}, {height:0.00}";
                            if (!panels.ContainsKey(key))
                            {
                                var a = Vector3.Origin;
                                var b = a + new Vector3(width, 0, 0);
                                var c = b + new Vector3(0, 0, height);
                                var d = a + new Vector3(0, 0, height);
                                var rect = new Polygon(new[] { a, b, c, d });
                                var panel = new Panel(rect, rand.NextMaterial());
                                panel.IsElementDefinition = true;
                                panels[key] = panel;
                                outputModel.AddElement(panel);
                                // var mc = new ModelCurve(rect, isElementDefinition: true);
                                // outputModel.AddElement(mc);
                                // panelCrvs[key] = mc;
                            }
                            var panelDef = panels[key];
                            // var crvs = panelCrvs[key];
                            var transform = lv.Transform.Concatenated(new Transform(line.Start, line.Direction(), Vector3.ZAxis, 0));
                            var panelInstance = panelDef.CreateInstance(transform, null);
                            var crvsInstance = panelDef.Perimeter.TransformedPolygon(transform);
                            outputModel.AddElement(panelInstance);
                            outputModel.AddElement(crvsInstance);
                        }
                    }
                }

                return new ExecutionResult(outputModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new ExecutionResult();
            }
        }

        [JSInvokable]
        public static Task<ExecutionResult> FacadeFromLevelsAsync(string levelsJson, double gridSize)
        {
            return Task.FromResult(FacadeFromLevels(levelsJson, gridSize));
        }
        private static double round(double val)
        {
            return Math.Round(val * 100) / 100.0;
        }

    }

    public class ExecutionResult
    {

        public ExecutionResult()
        {

        }
        public ExecutionResult(Element e)
        {
            var model = new Model();
            model.AddElement(e);
            Glb = model.ToGlTF();
            ElementsJson = model.ToJson();
        }

        public ExecutionResult(Model m)
        {
            Glb = m.ToGlTF();
            ElementsJson = m.ToJson();
        }

        public ExecutionResult(IEnumerable<Element> elements)
        {
            var model = new Model();
            model.AddElements(elements);
            Glb = model.ToGlTF();
            ElementsJson = model.ToJson();
        }
        public Byte[] Glb { get; set; }
        public string ElementsJson { get; set; }
    }
}
