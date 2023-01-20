# SmolHttp.NET
A simple HTTP server written for .NET 7 based on the plain TcpListener. Serves static files.
Makes extensive use of async/await with Task-based programming and can infinitely scale on multithreaded systems.

Supports AOT compilation to avoid JIT overhead.

**This project is designed for fun and learning purposes only**. It is not intended for production use. This HTTP server doesn't implement any failsafe features and is prone to vulnerabilities.
 
It can also fry your system if you have 1000+ concurrent TCP connections.