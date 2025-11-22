ProxyNet
---

A simple tool to proxy git requests to http proxy on windows

```
# ~/.ssh/config

Host github.com
  HostName github.com
  User git
  Port 22
  ProxyCommand pn --proxy-type http --proxy <host>:<port> --target-host %h --target-port %p
```