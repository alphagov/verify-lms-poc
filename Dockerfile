FROM microsoft/dotnet:latest


# Create directory for the app source code
RUN mkdir -p /usr/src/local-matching
WORKDIR /usr/src/local-matching

# Copy the source and restore dependencies
COPY . /usr/src/local-matching

RUN	mv local-matching-config-linux.yml local-matching-config.yml

RUN 	   apt-get update -qqy \
	&& apt-get install -qqy \
	     vim \
	     mysql-client \
             unixodbc 

ENV ODBCSYSINI=/etc
ENV ODBCINI=/etc/odbc.ini

#RUN mv odbc/odbcinst.ini /etc/odbcinst.ini
#RUN mv odbc/odbc.ini /etc/odbc.ini

#--------------------------------------------------- Use MySQL
RUN wget https://dev.mysql.com/get/Downloads/Connector-ODBC/5.3/mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit.tar.gz
RUN tar xvfz mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit.tar.gz
RUN cp mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit/bin/* /usr/local/bin/.
RUN cp mysql-connector-odbc-5.3.9-linux-debian9-x86-64bit/lib/* /usr/local/lib/.
RUN myodbc-installer -a -d -n "MySQL ODBC 5.3 Driver" -t "Driver=/usr/local/lib/libmyodbc5w.so"
RUN myodbc-installer -a -d -n "MySQL ODBC 5.3 Driver" -t "Driver=/usr/local/lib/libmyodbc5a.so"

#--------------------------------------------------- trying to use maria
#RUN wget https://downloads.mariadb.com/Connectors/odbc/connector-odbc-3.0.2/mariadb-connector-odbc-3.0.2-ga-debian-i686.tar.gz
#RUN tar xvfz mariadb-connector-odbc-3.0.2-ga-debian-i686.tar.gz
#RUN cp mariadb-connector-odbc-3.0.2-ga-debian-i686/lib/libmaodbc.so /usr/local/lib/.
#RUN odbcinst -i -d -f /etc/odbcinst.ini
#RUN odbcinst -i -s -l -f /etc/odbc.ini

RUN dotnet restore

# Expose the port and start the app
EXPOSE 6000
CMD [ "dotnet", "run" ]

