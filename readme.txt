


docker run -p 3306:3306 --name mariadbtest -e MYSQL_ROOT_PASSWORD=mypass -d mariadb


docker exec -it mariadbtest bash

CREATE SCHEMA `LMS` ;

CREATE TABLE Matches ( ID BIGINT AUTO_INCREMENT, PiD varchar(80) NOT NULL, TimeS DATETIME, AccountID varc
har(80), PRIMARY KEY (ID), key (PiD) );