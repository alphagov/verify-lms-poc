# Create the LMS database
CREATE SCHEMA `LMS` ;

# Select that one to use
USE LMS;

# Create the local matching table
CREATE TABLE Matches ( ID BIGINT AUTO_INCREMENT, PiD varchar(80) NOT NULL, TimeS DATETIME, AccountID varchar(80), PRIMARY KEY (ID), key (PiD) );

# Makes the test database
CREATE TABLE OneHundredThousandPeople ( ID BIGINT AUTO_INCREMENT, FirstName varchar(80), MiddleName varchar(80), SurName varchar(80), Address1 varchar(80), Address2 varchar(80),PostCode varchar(80), DateofBirth DATETIME, PhoneNumber varchar(80), Email varchar(80), Gender varchar(80), PRIMARY KEY (ID), key (DateOfBirth), key (PostCode) );
