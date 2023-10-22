# Use the official Tomcat base image from DockerHub
FROM tomcat:9.0.80-jdk21-openjdk-slim

# Remove the default Tomcat example apps
RUN rm -rf /usr/local/tomcat/webapps/*

# Copy your WAR file to the expected location
COPY OkapiService-1.1.0-SNAPSHOT.war /usr/local/tomcat/webapps/ROOT.war

# Expose the default Tomcat port
EXPOSE 8080

# The CMD instruction is inherited from the Tomcat base image, 
# so we don't have to specify it here. It will start Tomcat.

# Expose port 5005 for remote debugging
EXPOSE 5005

# Set JVM options for remote debugging
ENV CATALINA_OPTS="-agentlib:jdwp=transport=dt_socket,address=0.0.0.0:5005,server=y,suspend=n"

# The CMD instruction is inherited from the Tomcat base image, 
# so we don't have to specify it here. It will start Tomcat.
