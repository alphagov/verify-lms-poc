# The following is an example of using the default MARIADB test docker to be built and used
# for local matching database.

# To get the mariadb docker image
docker pull mariadb

# These commands allow you to start the mariadb partition.
docker run -p 3306:3306 --name mariadbtest -e MYSQL_ROOT_PASSWORD=mypass -d mariadb

# Once you have created the partition, the next command allows you to access it.
docker exec -it mariadbtest bash

# This gets you into MYSQL (maria) you must also provide the password
mysql -u root -p
(mypass)

#MySQL ODBC 5.3 Driver

# Create the LMS database
CREATE SCHEMA `LMS` ;

# Select that one to use
USE LMS;

# Create the local matching table
CREATE TABLE Matches ( ID BIGINT AUTO_INCREMENT, PiD varchar(80) NOT NULL, TimeS DATETIME, 
			AccountID varchar(80), PRIMARY KEY (ID), key (PiD) );

# List everything in the table
SELECT * from Matches

# Delete an ID
DELETE from Matches WHERE ID=xxxx;

#-----------------------------------------------------------------------------------------------------
# Creating a linux version
#
docker build -t local-matching .
docker run -d -p 6000:6000 --name verifylms local-matching

# This takes you to the bash in the container, here you could edit the YAML file
docker exec -it verifylms bash
