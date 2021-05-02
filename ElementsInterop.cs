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
        public static Task<Line[]> CreateGrid2dAsync(int uCount, int vCount)
        {
            var u = new Grid1d();
            u.DivideByCount(uCount);
            var v = new Grid1d();
            v.DivideByCount(vCount);
            var g = new Grid2d(u, v);

            // Everything will be serialized using System.Text.Json. We don't
            // have access to our custom serialization. So for now, we need
            // to return simple types.
            return Task.FromResult(g.GetCellSeparators(GridDirection.U).Concat(g.GetCellSeparators(GridDirection.V)).Cast<Line>().ToArray());
        }
    }
}