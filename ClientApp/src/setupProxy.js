const { createProxyMiddleware } = require('http-proxy-middleware');

const target = process.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}` :
  process.env.ASPNETCORE_URLS ? process.env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:57500';

module.exports = function(app) {
  app.use(
    '/api', 
    createProxyMiddleware({
      target: process.env.REACT_APP_API_HOST || target,
      changeOrigin: true,
    })
  );
};
