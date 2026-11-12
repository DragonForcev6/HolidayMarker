-- Holiday Maker – testdata
-- Minst: 5 boenden på minst 2 orter, 20 rum (minst 3 olika kapaciteter),
--        5 kunder, 25 bokningar i december 2026 med varierande längd.
-- Se till att det går att visa både sökningar MED och UTAN träff.



INSERT INTO hotel.Boende (stad,hotell_namn,pool,resturang,strand_avstand,centrum_avstand,ombome,Beskrivning,betyg) VALUES
	 ('Landskrona','Kastellet',0,1,1800,1200,'Stora lokaler med et rustikt gotiskt tema.','Stora sviter med en luftig atmosvär med svarta väggar o en gotiska utsmyckningen ger hotellet dess mystiska atmosvär, underhållning så som rollspel utlåvas ca varannan vecka.',5),
	 ('Askersund','The Mansion',1,1,500,1000,'Väl städat stora fina rum med en stort spa på under våningen.','En äldre byggnad som en gång i tiden varit hem för mindre begåvade män o kvinnor även fattig hus. I nutid husas lokalerna som hotell rum, spa, pool och resturang där en av ägarna hemma varande Norge till lagar sina utsökta rätter.',4),
	 ('Göteborg','St. Sesam',0,1,250,500,'Trevlig personal och helt uderbat mat i topp klass!','Mindre hotell som är ganska  tyst o försynt av sig som ligger 200 meter ifrån tåg station. Små mörka men trivsamma rum. Hotellet har stora sociala ytor.',4),
	 ('Malmö','SjundeHimmelen',1,1,150,50,'Hemtrevligt med trevlig miljö, mycket konst på väggaren.','HBTQ vänligt hotell nära till centrum.',3),
	 ('Veinge','Glamping',1,1,50,100,'Jätte häftigt ställe där vi fick bo i en fårhage i ett tält med stor dubbel säng o grillstaden precis utanför. Tackor med lamm gick utanför o det finns en damm som man kan bada i.','Gör något annorlunda sov under stjärnorna med lugna bräken ifrån våra vänliga får o lamm.',5);

INSERT INTO hotel.Kund (fornamn,efternamn,`e-post`,telefon) VALUES
	 ('Anna','Lindqvist','anna.lindqvist@example.com','070-1740605'),
	 ('Erik','Johansson','erik.johansson@example.com','070-1740606'),
	 ('Sara','Nilsson','sara.nilsson@example.com','070-1740607'),
	 ('Johan','Berg','johan.berg@example.com','070-1740608'),
	 ('Maja','Holm','maja.holm@example.com','070-1740609'),
	 ('Oskar','Lund','oskar.lund@example.com','070-1740610'),
	 ('Elin','Sandberg','elin.sandberg@example.com','070-1740611'),
	 ('Viktor','Ek','viktor.ek@example.com','070-1740612'),
	 ('Linnea','Ström','linnea.strom@example.com','070-1740613'),
	 ('Albin','Wallin','albin.wallin@example.com','070-1740614');
INSERT INTO hotel.Kund (fornamn,efternamn,`e-post`,telefon) VALUES
	 ('Ida','Forsberg','ida.forsberg@example.com','070-1740615'),
	 ('Hugo','Sjöberg','hugo.sjoberg@example.com','070-1740616'),
	 ('Klara','Engström','klara.engstrom@example.com','070-1740617'),
	 ('Nils','Dahl','nils.dahl@example.com','070-1740618'),
	 ('Ebba','Lindgren','ebba.lindgren@example.com','070-1740619'),
	 ('Axel','Björk','axel.bjork@example.com','070-1740620'),
	 ('Wilma','Hedlund','wilma.hedlund@example.com','070-1740621'),
	 ('Leo','Nyström','leo.nystrom@example.com','070-1740622'),
	 ('Freja','Åberg','freja.aberg@example.com','070-1740623'),
	 ('Isak','Mattsson','isak.mattsson@example.com','070-1740624');

INSERT INTO hotel.Rum (boende_id,pris_per_natt,max_personer,rums_nummer) VALUES
	 (1,650,1,'100'),
	 (1,650,1,'101'),
	 (1,950,2,'102'),
	 (1,1100,2,'103'),
	 (1,1800,4,'201'),
	 (2,500,1,'1'),
	 (2,800,2,'2'),
	 (2,1500,4,'3'),
	 (2,1650,4,'4'),
	 (3,1200,2,'A1'),
	 (3,1250,2,'A2'),
	 (3,2100,4,'B1'),
	 (3,900,1,'B2'),
	 (4,450,1,'11'),
	 (4,700,2,'12'),
	 (4,700,2,'13'),
	 (4,1300,4,'14'),
	 (5,1400,2,'301'),
	 (5,2400,4,'302'),
	 (5,1000,1,'303'),
	 (5,1450,2,'304');


INSERT INTO hotel.Bokningar (kund_id,rum_id,antal_personer,incheckning,utcheckning) VALUES
	 (1,2,1,'2026-12-10','2026-12-13'),
	 (2,2,1,'2026-12-20','2026-12-24'),
	 (4,4,2,'2026-12-18','2026-12-27'),
	 (5,1,1,'2026-12-01','2026-12-04'),
	 (13,3,2,'2026-12-15','2026-12-17'),
	 (10,6,1,'2026-12-12','2026-12-19'),
	 (4,7,2,'2026-12-10','2026-12-14'),
	 (1,8,3,'2026-12-10','2026-12-14'),
	 (15,7,2,'2026-12-22','2026-12-29'),
	 (2,9,4,'2026-12-03','2026-12-06');
INSERT INTO hotel.Bokningar (kund_id,rum_id,antal_personer,incheckning,utcheckning) VALUES
	 (14,10,1,'2026-12-24','2026-12-31'),
	 (5,11,2,'2026-12-01','2026-12-08'),
	 (1,11,2,'2026-12-26','2026-12-31'),
	 (2,12,4,'2026-12-14','2026-12-21'),
	 (9,13,1,'2026-12-02','2026-12-05'),
	 (4,14,1,'2026-12-11','2026-12-18'),
	 (7,15,2,'2026-12-07','2026-12-11'),
	 (18,17,4,'2026-12-19','2026-12-23'),
	 (11,19,2,'2026-12-05','2026-12-07');

-- Extra bokningar för testfallen i README:
-- * Rum 15 och rum 2: incheckning samma dag som föregående gäst checkar ut (ska vara tillåtet).
-- * Malmö (boende 4, rum 14–17) är fullbokat 12–18 dec -> sökning utan träff.
INSERT INTO hotel.Bokningar (kund_id,rum_id,antal_personer,incheckning,utcheckning) VALUES
	 (6,15,2,'2026-12-11','2026-12-18'),
	 (8,16,2,'2026-12-10','2026-12-18'),
	 (10,17,3,'2026-12-12','2026-12-18'),
	 (12,2,1,'2026-12-13','2026-12-17'),
	 (13,5,4,'2026-12-27','2026-12-31'),
	 (15,18,2,'2026-12-05','2026-12-06');
     


