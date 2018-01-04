# The following is an example of using the default MARIADB test docker to be built and used
# for local matching database.

# To get the mariadb docker image (This will be replaced once Dockerfile is included in the test data)
docker pull mariadb

# These commands allow you to start the mariadb partition.
docker run -p 3306:3306 --name mariadbtest -e MYSQL_ROOT_PASSWORD=mypass -d mariadb

# Once you have created the partition, the next command allows you to access it.
docker exec -it mariadbtest bash

# This gets you into MYSQL (maria) you must also provide the password
mysql -uroot -pmypass

# As a single command outside the container
docker exec -it mariadbtest mysql -uroot -pmypass

# Create the LMS database
CREATE SCHEMA `LMS` ;

# Select that one to use
USE LMS;

# Create the local matching table
CREATE TABLE Matches ( ID BIGINT AUTO_INCREMENT, PiD varchar(80) NOT NULL, TimeS DATETIME, AccountID varchar(80), PRIMARY KEY (ID), key (PiD) );

# Makes the test database
CREATE TABLE OneHundredThousandPeople ( ID BIGINT AUTO_INCREMENT, FirstName varchar(80), MiddleName varchar(80), SurName varchar(80), Address1 varchar(80), Address2 varchar(80),PostCode varchar(80), DateofBirth DATETIME, PhoneNumber varchar(80), Email varchar(80), Gender varchar(80), PRIMARY KEY (ID), key (DateOfBirth), key (PostCode) );

# List everything in the table
SELECT * from Matches

# Delete an ID
DELETE from Matches WHERE ID=xxxx;

#-----------------------------------------------------------------------------------------------------
# Creating The Test Data (for People100k)
#
# To do this, you need to be running bash in some form and be within the ../data-gen/ directory

./generate-verify-data.sh > People100k.csv
docker cp People100k.csv mariadbtest:/tmp/.

# Loading the test data file
LOAD DATA INFILE '/tmp/People100k.csv' INTO TABLE OneHundredThousandPeople  FIELDS TERMINATED BY ',' ENCLOSED BY '"'  LINES TERMINATED BY '\n'  IGNORE 1 LINES;

#-----------------------------------------------------------------------------------------------------
# Creating a linux version using the MySQL ODBC 5.3 Driver
#
docker build -t local-matching .

# Execute the image
docker run -d -p 6000:6000 --name verifylms local-matching

# This takes you to the bash in the container, here you could edit the YAML file
docker exec -it verifylms bash

# This command to modify the configuration YAML file
vi local-matching-config.yml

#-----------------------------------------------------------------------------------------------------
# To find out the IP addresses within the Docker Bridge use this command
#
docker network inspect bridge

# The IP address of the mariadbtest must be set into the local-matching-config.yml file

