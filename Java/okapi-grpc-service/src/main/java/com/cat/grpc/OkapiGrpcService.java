package com.cat.grpc;

import io.grpc.stub.StreamObserver;
import com.cat.grpc.OkapiGrpc.OkapiImplBase;
import com.cat.grpc.OkapiService.*;

public class OkapiGrpcService extends OkapiImplBase {
    @Override
    public void sayHello(HelloRequest request, StreamObserver<HelloResponse> responseObserver) {
        String greeting = "Hello, " + request.getName() + "!";
        
        HelloResponse response = HelloResponse.newBuilder().setMessage(greeting).build();
        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }
}
