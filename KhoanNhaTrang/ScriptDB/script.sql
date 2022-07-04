CREATE SCHEMA `grouting` DEFAULT CHARACTER SET utf8 COLLATE utf8_unicode_ci ;

CREATE TABLE `data` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `flow_rate` float DEFAULT NULL,
  `fluid` float DEFAULT NULL,
  `insert_date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `pressure` float DEFAULT NULL,
  `wc` float DEFAULT NULL,
  `management_id` bigint DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=61 DEFAULT CHARSET=utf8mb3 COLLATE=utf8_unicode_ci


CREATE TABLE `management` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `number_equipment` int NOT NULL DEFAULT '0',
  `insert_date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `cement_total` float DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3 COLLATE=utf8_unicode_ci

CREATE TABLE `config` (
  `id` int NOT NULL AUTO_INCREMENT,
  `time_update_ui` int DEFAULT '1',
  `time_store_db` int DEFAULT '60',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3 COLLATE=utf8_unicode_ci
INSERT INTO `grouting`.`config` ( `time_update_ui`, `time_store_db`) VALUES ( '1', '60');