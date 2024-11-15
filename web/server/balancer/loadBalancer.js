const http = require("http");
const { createProxyServer } = require("http-proxy");
const proxy = createProxyServer({});
const servers = ["http://localhost:5001", "http://localhost:5002/"];
const activeRequests = Array(servers.length).fill(0);

const server = http.createServer((req, res) => {
    res.setHeader("Access-Control-Allow-Origin", "http://localhost:5173");
    res.setHeader(
        "Access-Control-Allow-Methods",
        "GET, POST, PUT, DELETE, OPTIONS"
    );
    res.setHeader(
        "Access-Control-Allow-Headers",
        "Content-Type, Authorization"
    );

    const leastLoadedServerIndex = activeRequests.indexOf(
        Math.min(...activeRequests)
    );
    activeRequests[leastLoadedServerIndex]++;
    
    proxy.web(req, res, { target: servers[leastLoadedServerIndex] });
    console.log(`Forwarding request to server: ${servers[leastLoadedServerIndex]}`);

    res.on("finish", () => {
        activeRequests[leastLoadedServerIndex]--;
    });
});

server.listen(5001, () => console.log("Load Balancer running on port 5001"));