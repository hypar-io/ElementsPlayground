class Elements {
    async createGrid2d(uCount, vCount) {
        return DotNet.invokeMethodAsync('ElementsWasm', 'CreateGrid2dAsync', uCount, vCount)
    }
}

window.elements = new Elements();