CREATE TABLE `MySqlCdc` (
  `Id` int NOT NULL AUTO_INCREMENT,
  
  /* Numeric types */
  `TinyColumn` tinyint,
  `ShortColumn` smallint,
  `Int24Column` mediumint,
  `Int32Column` int,
  `LongColumn` bigint,
  `FloatColumn` float,
  `DoubleColumn` double,
  `DecimalColumn` decimal(65,10),
  
  /* String types */
  `CharColumn` char(200),
  `VarcharColumn` varchar(700),
  `VarstringColumn` varbinary(800),

  /* Blob types */
  `JsonColumn` json,
  `BlobColumn` blob,
  
  /* BIT, ENUM, SET */
  `BitColumn` bit(28),  
  `EnumColumn` enum('Low', 'Medium', 'High'),
  `SetColumn` set('Green', 'Yellow', 'Red'),

  /* Date & time types */
  `YearColumn` year,
  `DateColumn` date,
  `DateTimeColumn` datetime(3),
  `TimeColumn` time(2),
  `TimestampColumn` timestamp(3),

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4;

insert into `MySqlCdc` (
  `TinyColumn`, `ShortColumn`, `Int24Column`, `Int32Column`, `LongColumn`, `FloatColumn`, `DoubleColumn`, `DecimalColumn`,
  `CharColumn`, `VarcharColumn`, `VarstringColumn`, 
  `JsonColumn`, `BlobColumn`,
  `BitColumn`, `EnumColumn`, `SetColumn`,
  `YearColumn`, `DateColumn`, `DateTimeColumn`, `TimeColumn`, `TimestampColumn`
  )
values(
  123, 12345, 1234567, 123456789, 1234567890987654321, 12345.6789, 123456789.0987654321, 1234567890112233445566778899001112223334445556667778889.9900011112,
  'Asd234234 234234 zdfsfsdfsdf', 'Hello world!', x'1234564534454534',
  '{"id": 1, "name": "John"}', x'152354',
  b'0111101101010101011110000111', 'Medium', 'Red',
  1989, '1985-10-23', '1988-11-27 14:12:13.345', '15:22:33.67','1985-10-13 13:22:23.567' 
)

SELECT *, bin(BitColumn), EnumColumn+0, SetColumn+0 FROM MySqlCdc;