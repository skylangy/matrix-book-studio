const PROXY_CONFIG = [
  {
    context: [
      "/api/v1",
    ],
    target: "https://localhost:7101",
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
    headers: {
      Connection: "keep-alive"
    }
  }
]

module.exports = PROXY_CONFIG;
