# Use the official Tomcat base image from DockerHub
FROM tomcat:8.5.95-jdk17-corretto-al2

# Remove the default Tomcat example apps
RUN rm -rf /usr/local/tomcat/webapps/*

# Copy your custom tomcat-users.xml to the Tomcat configuration directory
#COPY tomcat-users.xml /usr/local/tomcat/conf/

# Copy your WAR file to the expected location
COPY OkapiService-1.1.0-SNAPSHOT.war /usr/local/tomcat/webapps/OkapiService.war

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
