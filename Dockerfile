FROM microsoft/dotnet:latest

# Create directory for the app source code
RUN mkdir -p /usr/src/local-matching
WORKDIR /usr/src/local-matching

# Copy the source and restore dependencies
COPY . /usr/src/local-matching
RUN dotnet restore

# Expose the port and start the app
EXPOSE 6000
EXPOSE 3306
CMD [ "dotnet", "run" ]

