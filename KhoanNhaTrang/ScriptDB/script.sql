CREATE SCHEMA `grouting` DEFAULT CHARACTER SET utf8 COLLATE utf8_unicode_ci ;

CREATE TABLE `grouting`.`data` (
   `id` bigint NOT NULL AUTO_INCREMENT,
   `flow_rate` float,
   `fluid` float,
   `insert_date` timestamp default now(),
   PRIMARY KEY (`id`))
 ENGINE = InnoDB
 DEFAULT CHARACTER SET = utf8
 COLLATE = utf8_unicode_ci	