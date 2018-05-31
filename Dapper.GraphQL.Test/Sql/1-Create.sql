CREATE TABLE Person (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	-- Used to identify the person that this has been merged with
	-- (for deduplicating person entities in the db)
	MergedToPersonId INTEGER,
	FirstName NVARCHAR(50),
	LastName NVARCHAR(50),
	-- Known issue with FK reference to non-null numeric types
	-- https://github.com/StackExchange/Dapper/issues/917
	SupervisorId INTEGER,
	CareerCounselorId INTEGER,
	FOREIGN KEY (MergedToPersonId) REFERENCES Person(Id),
	FOREIGN KEY (SupervisorId) REFERENCES Person(Id),
	FOREIGN KEY (CareerCounselorId) REFERENCES Person(Id)
);

CREATE TABLE Company (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	Name NVARCHAR(100)
);

CREATE TABLE PersonCompany (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	PersonId INTEGER NOT NULL,
	CompanyId INTEGER NOT NULL,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME
	FOREIGN KEY(PersonId) REFERENCES Person(Id),
	FOREIGN KEY(CompanyId) REFERENCES Company(Id)
);

CREATE TABLE Email (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	[Address] NVARCHAR(250)
);

CREATE TABLE PersonEmail (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	PersonId INTEGER NOT NULL,
	EmailId INTEGER NOT NULL,
	FOREIGN KEY(PersonId) REFERENCES Person(Id),
	FOREIGN KEY(EmailId) REFERENCES Email(Id)
);

CREATE TABLE CompanyEmail (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	CompanyId INTEGER NOT NULL,
	EmailId INTEGER NOT NULL,
	FOREIGN KEY(CompanyId) REFERENCES Company(Id),
	FOREIGN KEY(EmailId) REFERENCES Email(Id)
);

CREATE TABLE Phone (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	Number NVARCHAR(16),
	[Type] INTEGER,	
);

CREATE TABLE PersonPhone (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	PersonId INTEGER NOT NULL,
	PhoneId INTEGER NOT NULL,
	FOREIGN KEY(PersonId) REFERENCES Person(Id),
	FOREIGN KEY(PhoneId) REFERENCES Phone(Id)
);

CREATE TABLE CompanyPhone (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY(1, 1),
	CompanyId INTEGER NOT NULL,
	PhoneId INTEGER NOT NULL,
	FOREIGN KEY(CompanyId) REFERENCES Company(Id),
	FOREIGN KEY(PhoneId) REFERENCES Phone(Id)
);