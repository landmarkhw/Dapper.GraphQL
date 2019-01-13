INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId, CreateDate) VALUES (1, 1, 'Hyrum', 'Clyde', NULL, NULL, '2019-01-01');
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId, CreateDate) VALUES (2, 2, 'Doug', 'Day', NULL, NULL, '2019-01-02');
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId, CreateDate) VALUES (3, 3, 'Kevin', 'Russon', 1, 2, '2019-01-03');
INSERT INTO Person (Id, MergedToPersonId, FirstName, LastName, SupervisorId, CareerCounselorId, CreateDate) VALUES (4, 4, 'Douglas', 'Day', NULL, 1, '2019-01-04');
-- Update the identity value
SELECT setval(pg_get_serial_sequence('person', 'id'), (SELECT MAX(Id) FROM Person));

-- Merge people (Doug == Douglas)
UPDATE Person
SET MergedToPersonId = 4
WHERE Id = 2;

INSERT INTO Company (Id, Name) VALUES (1, 'Landmark Home Warranty, LLC');
INSERT INTO Company (Id, Name) VALUES (2, 'Navitaire, LLC');
SELECT setval(pg_get_serial_sequence('company', 'id'), (SELECT MAX(Id) FROM Company));

INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (1, 1, 1, '2016-01-01', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (2, 2, 1, '2016-05-16', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (3, 3, 1, '2016-10-05', NULL);
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (4, 4, 2, '2011-04-06', '2016-05-13');
INSERT INTO PersonCompany (Id, PersonId, CompanyId, StartDate, EndDate) VALUES (5, 3, 2, '2011-08-15', '2016-10-02');
SELECT setval(pg_get_serial_sequence('personcompany', 'id'), (SELECT MAX(Id) FROM PersonCompany));

INSERT INTO Email (Id, Address) VALUES (1, 'hclyde@landmarkhw.com');
INSERT INTO Email (Id, Address) VALUES (2, 'dday@landmarkhw.com');
INSERT INTO Email (Id, Address) VALUES (3, 'dougrday@gmail.com');
INSERT INTO Email (Id, Address) VALUES (4, 'krusson@landmarkhw.com');
INSERT INTO Email (Id, Address) VALUES (5, 'whole-company@landmarkhw.com');
INSERT INTO Email (Id, Address) VALUES (6, 'whole-company@navitaire.com');
SELECT setval(pg_get_serial_sequence('email', 'id'), (SELECT MAX(Id) FROM Email));

INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (1, 1, 1);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (2, 2, 2);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (3, 3, 4);
INSERT INTO PersonEmail (Id, EmailId, PersonId) VALUES (4, 4, 3);
SELECT setval(pg_get_serial_sequence('personemail', 'id'), (SELECT MAX(Id) FROM PersonEmail));

INSERT INTO CompanyEmail (Id, CompanyId, EmailId) VALUES (1, 1, 5);
INSERT INTO CompanyEmail (Id, CompanyId, EmailId) VALUES (2, 2, 6);
SELECT setval(pg_get_serial_sequence('companyemail', 'id'), (SELECT MAX(Id) FROM CompanyEmail));

INSERT INTO Phone (Id, Number, Type) VALUES (1, '8011234567', 3);
INSERT INTO Phone (Id, Number, Type) VALUES (2, '8019876543', 3);
INSERT INTO Phone (Id, Number, Type) VALUES (3, '8011111111', 1);
INSERT INTO Phone (Id, Number, Type) VALUES (4, '8663062999', 1);
INSERT INTO Phone (Id, Number, Type) VALUES (5, '8019477800', 1);
SELECT setval(pg_get_serial_sequence('phone', 'id'), (SELECT MAX(Id) FROM Phone));

INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (1, 1, 2);
INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (2, 2, 3);
INSERT INTO PersonPhone (Id, PhoneId, PersonId) VALUES (3, 3, 3);
SELECT setval(pg_get_serial_sequence('personphone', 'id'), (SELECT MAX(Id) FROM PersonPhone));

INSERT INTO CompanyPhone (Id, PhoneId, CompanyId) VALUES (1, 4, 1);
INSERT INTO CompanyPhone (Id, PhoneId, CompanyId) VALUES (2, 5, 2);
SELECT setval(pg_get_serial_sequence('companyphone', 'id'), (SELECT MAX(Id) FROM CompanyPhone));
