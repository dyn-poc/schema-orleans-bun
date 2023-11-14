/** @type {import('@remix-run/dev').AppConfig} */
export default {
  ignoredRouteFiles: ["**/.*"],
  // appDirectory: "app",
  // assetsBuildDirectory: "public/build",
  serverBuildPath: "build/index.js",
  // publicPath: "/build/",
  serverModuleFormat: "esm",
  browserNodeBuiltinsPolyfill:{
    modules: {
      buffer: true, // Provide a JSPM polyfill
      fs: true, // Provide an empty polyfill,
      path: true ,
      process: true,
      dom:true,
      util: true,
      http: true,
        https: true,
        url: true

    },
    globals: {
      Buffer: true,
    },
  }
};

//
// exports.browserNodeBuiltinsPolyfill = {
//   modules: {
//     buffer: true, // Provide a JSPM polyfill
//     fs: "empty", // Provide an empty polyfill,
//     path: true ,
//     process: true,
//     dom:true
//   },
//   globals: {
//     Buffer: true,
//   },
// };
