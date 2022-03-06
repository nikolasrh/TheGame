https://grpc.io/docs/protoc-installation/

```
apt install -y protobuf-compiler
protoc --version  # Ensure compiler version is 3+
protoc --csharp_out=. messages.proto
```
