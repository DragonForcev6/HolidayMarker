-- Holiday Maker – skapa databas och tabeller
-- Kör detta skript först.

CREATE DATABASE IF NOT EXISTS hotel;
USE hotel;

-- TODO: Tabeller enligt ditt E/R-diagram.
--       Tänk på: primärnycklar, främmande nycklar, NOT NULL, UNIQUE, CHECK
--       (t.ex. omdöme 1–5, kapacitet > 0, pris >= 0, utcheckning > incheckning,
--        avstånd >= 0, rumsnummer unikt inom ett boende).
--       Ska ort vara en egen tabell? Motivera i reflektionen.

-- hotel.Boende definition

CREATE TABLE `Boende` (
  `boende_id` int NOT NULL AUTO_INCREMENT,
  `stad` varchar(200) NOT NULL,
  `hotell_namn` varchar(255) NOT NULL,
  `pool` tinyint(1) NOT NULL,
  `resturang` tinyint(1) NOT NULL,
  `strand_avstand` int NOT NULL CHECK (`strand_avstand` >= 0),
  `centrum_avstand` int NOT NULL CHECK (`centrum_avstand` >= 0),
  `ombome` varchar(255) NOT NULL,
  `Beskrivning` varchar(255) NOT NULL,
  `betyg` int NOT NULL CHECK (`betyg` BETWEEN 1 AND 5),
  PRIMARY KEY (`boende_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- hotel.Kund definition

CREATE TABLE `Kund` (
  `id` int NOT NULL AUTO_INCREMENT,
  `fornamn` varchar(255) NOT NULL,
  `efternamn` varchar(255) NOT NULL,
  `e-post` varchar(255) NOT NULL,
  `telefon` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `kund_fornamn_index` (`fornamn`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- hotel.Rum definition

CREATE TABLE `Rum` (
  `id` int NOT NULL AUTO_INCREMENT,
  `boende_id` int NOT NULL,
  `pris_per_natt` decimal(10,0) NOT NULL CHECK (`pris_per_natt` >= 0),
  `max_personer` int NOT NULL DEFAULT 1 CHECK (`max_personer` > 0),
  `rums_nummer` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rum_nummer` (`boende_id`, `rums_nummer`),
  KEY `rum_boende_id_foreign` (`boende_id`),
  CONSTRAINT `rum_boende_id_foreign` FOREIGN KEY (`boende_id`) REFERENCES `Boende` (`boende_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- hotel.Bokningar definition

CREATE TABLE `Bokningar` (
  `boknings_id` int NOT NULL AUTO_INCREMENT,
  `kund_id` int NOT NULL,
  `rum_id` int NOT NULL,
  `antal_personer` int NOT NULL,
  `incheckning` date NOT NULL,
  `utcheckning` date NOT NULL,
  PRIMARY KEY (`boknings_id`),
  CONSTRAINT `chk_datum` CHECK (`utcheckning` > `incheckning`),
  KEY `bokningar_kund_id_foreign` (`kund_id`),
  KEY `bokningar_rum_id_foreign` (`rum_id`),
  CONSTRAINT `bokningar_kund_id_foreign` FOREIGN KEY (`kund_id`) REFERENCES `Kund` (`id`),
  CONSTRAINT `bokningar_rum_id_foreign` FOREIGN KEY (`rum_id`) REFERENCES `Rum` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
        