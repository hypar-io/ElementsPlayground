using System.Linq;
using System.Threading.Tasks;
using Elements.Geometry;
using Elements.Spatial;
using Microsoft.JSInterop;

namespace ElementsWasm
{
    public static class ElementsInterop
    {
        [JSInvokable]
        public static Task<Line[]> CreateGrid2dAsync(Polygon polygon)
        {
            var g = new Grid2d(polygon);
            g.U.DivideByCount(5);
            g.V.DivideByCount(5);

            // Everything will be serialized using System.Text.Json. We don't
            // have access to our custom serialization. So for now, we need
            // to return simple types.
            return Task.FromResult(g.GetCellSeparators(GridDirection.U).Concat(g.GetCellSeparators(GridDirection.V)).Cast<Line>().ToArray());
        }

        [JSInvokable]
        public static Line[] CreateGrid2d(Polygon polygon)
        {
            var g = new Grid2d(polygon);
            g.U.DivideByCount(5);
            g.V.DivideByCount(5);

            // Everything will be serialized using System.Text.Json. We don't
            // have access to our custom serialization. So for now, we need
            // to return simple types.
            return g.GetCellSeparators(GridDirection.U).Concat(g.GetCellSeparators(GridDirection.V)).Cast<Line>().ToArray();
        }
    }
}