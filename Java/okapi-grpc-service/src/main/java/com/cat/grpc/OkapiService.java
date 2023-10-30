package com.cat.grpc;

import io.grpc.stub.StreamObserver;
import com.cat.grpc.OkapiServiceGrpc.OkapiServiceImplBase;
import com.cat.grpc.OkapiServiceOuterClass.*;

public class OkapiService extends OkapiServiceImplBase {
    @Override
    public void sayHello(HelloRequest request, StreamObserver<HelloResponse> responseObserver) {
        String greeting = "Hello, " + request.getName() + "!";
        HelloResponse response = HelloResponse.newBuilder().setMessage(greeting).build();
        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }
}
