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

### Details
This is as minimal a blazor example as we can create that demonstrates interop with Elements.

The `Program.cs` file is required as it contains the entry point to the application and the `WebAssemblyHost` setup which is required by the blazor framework. The http client that is created in `Program.cs` is used for demonstration purposes only and can be removed as well.
