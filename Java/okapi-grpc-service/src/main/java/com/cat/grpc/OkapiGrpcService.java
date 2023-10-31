package com.cat.grpc;

import io.grpc.stub.StreamObserver;

import static io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall;

import com.cat.grpc.OkapiGrpc.OkapiImplBase;
import com.cat.grpc.OkapiService.*;
import com.google.protobuf.ByteString;
import io.grpc.*;

public class OkapiGrpcService extends OkapiImplBase {
	@Override
	public void sayHello(HelloRequest request, StreamObserver<HelloResponse> responseObserver) {
		String greeting = "Hello, " + request.getName() + "!";

		HelloResponse response = HelloResponse.newBuilder().setMessage(greeting).build();
		responseObserver.onNext(response);
		responseObserver.onCompleted();
	}
	
	@Override
	public void createXliffFromDocument(com.cat.grpc.OkapiService.CreateXliffFromDocumentRequest request,
			io.grpc.stub.StreamObserver<com.cat.grpc.OkapiService.CreateXliffFromDocumentResponse> responseObserver) {
		try {
			var okapiService = new com.tm.okapi.service.OkapiService();
			var xliffContent = okapiService.createXliffFromDocument(request.getFileName(), request.getFileContent().toByteArray(), 
					request.getFilterName(), request.getFilterContent().toByteArray(), 
					request.getSourceLangISO6391(), request.getTargetLangISO6391());
			
			var response = CreateXliffFromDocumentResponse.newBuilder().setXliffContent(xliffContent).build();
			responseObserver.onNext(response);
			responseObserver.onCompleted();
		} catch (Exception ex) {
	        // Return an error to the gRPC client
	        responseObserver.onError(Status.INTERNAL.withDescription("An error occurred during document creation").asException());			
		}		
	}

	@Override
	public void createDocumentFromXliff(com.cat.grpc.OkapiService.CreateDocumentFromXliffRequest request,
			io.grpc.stub.StreamObserver<com.cat.grpc.OkapiService.CreateDocumentFromXliffResponse> responseObserver) {		
		try {
			var okapiService = new com.tm.okapi.service.OkapiService();
			var bytes = okapiService.createDocumentFromXliff(request.getFileName(), request.getFileContent().toByteArray(), 
					request.getFilterName(), request.getFilterContent().toByteArray(), request.getSourceLangISO6391(), 
					request.getTargetLangISO6391(), request.getXliffContent());
			
			var response = CreateDocumentFromXliffResponse.newBuilder().setCreatedDocument(ByteString.copyFrom(bytes)).build();
			responseObserver.onNext(response);
			responseObserver.onCompleted();
		} catch (Exception ex) {
	        // Return an error to the gRPC client
	        responseObserver.onError(Status.INTERNAL.withDescription("An error occurred during document creation").asException());			
		}		
	}
}
