FROM microsoft/dotnet:latest

# Create directory for the app source code
RUN mkdir -p /usr/src/test-website
WORKDIR /usr/src/test-website

# Copy the source and restore dependencies
COPY . /usr/src/test-website

#--------------------------------------------------- Use MySQL
RUN wget https://dev.mysql.com/get/Downloads/Connector-ODBC/5.3/mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit.tar.gz
RUN tar xvfz mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit.tar.gz
RUN cp mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit/bin/* /usr/local/bin/.
RUN cp mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit/lib/* /usr/local/lib/.
RUN myodbc-installer -a -d -n "MySQL ODBC 5.3 Driver" -t "Driver=/usr/local/lib/libmyodbc5w.so"
RUN myodbc-installer -a -d -n "MySQL ODBC 5.3 Driver" -t "Driver=/usr/local/lib/libmyodbc5a.so"

RUN dotnet restore

# Expose the port and start the app
EXPOSE 5000
CMD [ "dotnet", "run" ]

