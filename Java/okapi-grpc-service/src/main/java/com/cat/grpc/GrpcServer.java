package com.cat.grpc;

import io.grpc.Server;
import io.grpc.ServerBuilder;

public class GrpcServer {
    public static void main(String[] args) throws Exception {
        Server server = ServerBuilder.forPort(50051) // Choose a port
            .addService(new OkapiService()) // Add your gRPC service implementation
            .build();

        server.start();
        System.out.println("Server started on port 50051");
        server.awaitTermination();
    }
}
