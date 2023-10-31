package com.cat.grpc;

import io.grpc.stub.StreamObserver;

import static io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall;

import com.cat.grpc.OkapiGrpc.OkapiImplBase;
import com.cat.grpc.OkapiService.*;
import com.google.protobuf.ByteString;

public class OkapiGrpcService extends OkapiImplBase {
	@Override
	public void sayHello(HelloRequest request, StreamObserver<HelloResponse> responseObserver) {
		String greeting = "Hello, " + request.getName() + "!";

		HelloResponse response = HelloResponse.newBuilder().setMessage(greeting).build();
		responseObserver.onNext(response);
		responseObserver.onCompleted();
	}

	@Override
	public void createDocumentFromXliff(com.cat.grpc.OkapiService.CreateDocumentFromXliffRequest request,
			io.grpc.stub.StreamObserver<com.cat.grpc.OkapiService.CreateDocumentFromXliffResponse> responseObserver) {
		var response = CreateDocumentFromXliffResponse.newBuilder().setCreatedDocument(ByteString.copyFrom(new byte[] {10, 20})).build();
		responseObserver.onNext(response);
		responseObserver.onCompleted();
	}

	@Override
	public void createXliffFromDocument(com.cat.grpc.OkapiService.CreateXliffFromDocumentRequest request,
			io.grpc.stub.StreamObserver<com.cat.grpc.OkapiService.CreateXliffFromDocumentResponse> responseObserver) {
		var response = CreateXliffFromDocumentResponse.newBuilder().setXliffContent("xliff content comes here.").build();
		responseObserver.onNext(response);
		responseObserver.onCompleted();
	}
}
