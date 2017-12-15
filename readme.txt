


docker run -p 3306:3306 --name mariadbtest -e MYSQL_ROOT_PASSWORD=mypass -d mariadb


docker exec -it mariadbtest bash

CREATE SCHEMA `LMS` ;

