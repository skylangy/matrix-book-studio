const PROXY_CONFIG = [
  {
    context: [
      "/api/v1",
    ],
    target: "https://127.0.0.1:7110",
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
    headers: {
      Connection: "keep-alive"
    }
  },
  {
    context: [
      "/workprogress"
    ],
    target: "https://127.0.0.1:7110",
    secure: false,
    ws: true
  }
]

module.exports = PROXY_CONFIG;
