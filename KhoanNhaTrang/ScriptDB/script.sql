CREATE SCHEMA `grouting` DEFAULT CHARACTER SET utf8 COLLATE utf8_unicode_ci ;

CREATE TABLE `grouting`.`data` (
   `id` bigint NOT NULL AUTO_INCREMENT,
   `flow_rate` float,
   `fluid` float,
   `wc` float,
  `pressure` float,
  `id_management` bigint,
   `insert_date` timestamp default now(),
   PRIMARY KEY (`id`))
 ENGINE = InnoDB
 DEFAULT CHARACTER SET = utf8
 COLLATE = utf8_unicode_ci	


 CREATE TABLE `grouting`.`management` (
   `id` bigint NOT NULL AUTO_INCREMENT,
   `number_equipment` INT NOT NULL DEFAULT 0,
   `cement_total`float,
   `insert_date` timestamp default now(),
   PRIMARY KEY (`id`))
 ENGINE = InnoDB
 DEFAULT CHARACTER SET = utf8
 COLLATE = utf8_unicode_ci	