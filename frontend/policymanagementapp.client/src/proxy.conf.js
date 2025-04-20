const { env } = require('process');

// Get the target from environment variables or use a fallback
const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:57296';

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/scalar", // .NET 9 no longer includes Swagger UI by default; /scalar is used as its replacement for OpenAPI UI.      "/health"
      "/health"
    ],
    target,
    secure: false
  }
]

module.exports = PROXY_CONFIG;
