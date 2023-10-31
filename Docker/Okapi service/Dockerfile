# Use an appropriate base image with the desired JDK version
FROM openjdk:17-alpine

# Copy your JAR file to the image
COPY okapi-grpc-service-1.0-jar-with-dependencies.jar /app/okapi-grpc-service.jar

# Expose the port on which your gRPC service will listen
EXPOSE 50051

# Expose port 5005 for remote debugging (if needed)
EXPOSE 5005

# Set JVM options for remote debugging (if needed)
ENV JAVA_OPTS="-agentlib:jdwp=transport=dt_socket,address=0.0.0.0:5005,server=y,suspend=n"

# Specify the entry point for your application
CMD ["java", "-jar", "/app/okapi-grpc-service.jar"]
