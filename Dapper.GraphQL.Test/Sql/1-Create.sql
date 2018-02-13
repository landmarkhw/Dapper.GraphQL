CREATE TABLE Person (
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	FirstName TEXT,
	LastName TEXT,
	-- Known issue with FK reference to non-null numeric types
	-- https://github.com/StackExchange/Dapper/issues/917
	SupervisorId INTEGER NOT NULL,
	CareerCounselorId INTEGER NOT NULL,
	FOREIGN KEY (SupervisorId) REFERENCES Person(Id),
	FOREIGN KEY (CareerCounselorId) REFERENCES Person(Id)
);

CREATE TABLE Email (
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Address] TEXT,
	PersonId INTEGER,
	FOREIGN KEY(PersonId) REFERENCES Person(Id)
);

CREATE TABLE Phone (
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	Number TEXT,
	[Type] INTEGER,
	PersonId INTEGER,
	FOREIGN KEY(PersonId) REFERENCES Person(Id)
);
