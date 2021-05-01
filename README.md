# ElementsPlayground
A minimal example of elements using wasm.

### Running
`dotnet build`

Visit http://localhost:5000.

### Organization
`/wwwroot/js/elements.js` - The javascript API for Elements.
`/ElementsInterop.cs` - The .net wrapper methods to elements.

### Calling elements methods from javascript
```javascript
// Elements methods are asynchronous.
let l = await elements.createGrid2d(5,5)
console.log(l)
```
