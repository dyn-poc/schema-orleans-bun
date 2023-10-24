const webpack = require('webpack');
module.exports = {

  "resolve.fallback": {
    "path": require.resolve("path-browserify"),
    'util': require.resolve('util/'),
    'fs': require.resolve('browserify-fs'),
    "buffer": require.resolve("buffer/"),
    "http": require.resolve("stream-http"),
    "https": require.resolve("https-browserify"),
    "url": require.resolve("url"),
  },
  plugins: [
    new webpack.ProvidePlugin({
      Buffer: ['buffer', 'Buffer']
    })]

};
